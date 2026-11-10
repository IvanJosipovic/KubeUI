using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

internal sealed class ApiDiscoveryRefreshCoordinator : IAsyncDisposable
{
    private static readonly TimeSpan s_customResourceDefinitionRefreshDebounce = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan s_initialDiscoveryRetryDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan s_maximumDiscoveryRetryDelay = TimeSpan.FromSeconds(5);

    private readonly Func<CancellationToken, Task<ApiDiscoverySnapshot?>> _refreshAsync;
    private readonly CancellationTokenSource _shutdownTokenSource;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, ObservedCustomResourceDefinition> _observedCustomResourceDefinitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, EstablishedCustomResourceDefinition> _pendingEstablishedCustomResourceDefinitions = new(StringComparer.Ordinal);
    private readonly List<ImmediateRefreshWaiter> _immediateRefreshWaiters = [];
    private Task? _workerTask;
    private bool _refreshRequested;
    private bool _immediateRefreshRequested;
    private bool _stopping;
    private long _requestVersion;
    private int _retryAttempt;
    private TaskCompletionSource _requestSignal = CreateRequestSignal();

    internal ApiDiscoveryRefreshCoordinator(
        Func<CancellationToken, Task<ApiDiscoverySnapshot?>> refreshAsync,
        CancellationToken connectionCancellationToken)
    {
        _refreshAsync = refreshAsync ?? throw new ArgumentNullException(nameof(refreshAsync));
        _shutdownTokenSource = CancellationTokenSource.CreateLinkedTokenSource(connectionCancellationToken);
    }

    internal Task RefreshImmediatelyAsync()
        => RequestRefresh(immediate: true);

    internal void ObserveCustomResourceDefinitionEvent(
        WatchEventType eventType,
        V1CustomResourceDefinition definition)
    {
        bool shouldRefresh;
        lock (_gate)
        {
            if (_stopping)
            {
                return;
            }

            shouldRefresh = ShouldRefreshForCustomResourceDefinition(eventType, definition);
            if (shouldRefresh)
            {
                var name = definition.Name() ?? string.Empty;
                if (eventType == WatchEventType.Deleted)
                {
                    _pendingEstablishedCustomResourceDefinitions.Remove(name);
                }
                else if (IsEstablished(definition)
                    && TryCreateEstablishedDefinition(definition, out var establishedDefinition))
                {
                    _pendingEstablishedCustomResourceDefinitions[name] = establishedDefinition;
                }
                else
                {
                    _pendingEstablishedCustomResourceDefinitions.Remove(name);
                }
            }
        }

        if (shouldRefresh)
        {
            _ = RequestRefresh(immediate: false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task? workerTask;
        TaskCompletionSource requestSignal;
        lock (_gate)
        {
            _stopping = true;
            _refreshRequested = false;
            _immediateRefreshRequested = false;
            _requestVersion++;
            workerTask = _workerTask;
            requestSignal = _requestSignal;
            _requestSignal = CreateRequestSignal();
        }

        _shutdownTokenSource.Cancel();
        CancelImmediateRefreshWaiters(_shutdownTokenSource.Token);
        requestSignal.TrySetResult();
        if (workerTask is not null)
        {
            await workerTask.ConfigureAwait(false);
        }

        lock (_gate)
        {
            _observedCustomResourceDefinitions.Clear();
            _pendingEstablishedCustomResourceDefinitions.Clear();
        }

        _shutdownTokenSource.Dispose();
    }

    private Task RequestRefresh(bool immediate)
    {
        TaskCompletionSource requestSignal;
        TaskCompletionSource? completionSource = null;
        lock (_gate)
        {
            if (_stopping)
            {
                return Task.CompletedTask;
            }

            _refreshRequested = true;
            _immediateRefreshRequested |= immediate;
            var requestVersion = ++_requestVersion;
            if (immediate)
            {
                completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _immediateRefreshWaiters.Add(new ImmediateRefreshWaiter(requestVersion, completionSource));
            }

            requestSignal = _requestSignal;
            _requestSignal = CreateRequestSignal();
            if (_workerTask is null || _workerTask.IsCompleted)
            {
                _workerTask = Task.Run(RunWorkerAsync);
            }
        }

        requestSignal.TrySetResult();
        return completionSource?.Task ?? Task.CompletedTask;
    }

    private async Task RunWorkerAsync()
    {
        var cancellationToken = _shutdownTokenSource.Token;
        try
        {
            while (true)
            {
                Task requestSignalTask;
                long requestVersion;
                bool immediate;
                lock (_gate)
                {
                    if (_stopping || cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!_refreshRequested)
                    {
                        _workerTask = null;
                        return;
                    }

                    immediate = _immediateRefreshRequested;
                    requestVersion = _requestVersion;
                    requestSignalTask = _requestSignal.Task;
                    if (immediate)
                    {
                        _refreshRequested = false;
                        _immediateRefreshRequested = false;
                    }
                }

                if (!immediate)
                {
                    var debounceTask = Task.Delay(s_customResourceDefinitionRefreshDebounce, cancellationToken);
                    await Task.WhenAny(debounceTask, requestSignalTask).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    lock (_gate)
                    {
                        if (_stopping || requestVersion != _requestVersion)
                        {
                            continue;
                        }

                        if (!_refreshRequested)
                        {
                            continue;
                        }

                        _refreshRequested = false;
                        _immediateRefreshRequested = false;
                    }
                }

                var snapshot = await _refreshAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                CompleteImmediateRefreshWaiters(requestVersion);
                if (HasPendingEstablishedDefinitions(snapshot))
                {
                    await WaitForDiscoveryRetryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            CancelImmediateRefreshWaiters(cancellationToken);
        }
        finally
        {
            lock (_gate)
            {
                if (_stopping)
                {
                    _refreshRequested = false;
                    _immediateRefreshRequested = false;
                    _workerTask = null;
                }
            }
        }
    }

    private bool ShouldRefreshForCustomResourceDefinition(
        WatchEventType eventType,
        V1CustomResourceDefinition definition)
    {
        var name = definition.Name() ?? string.Empty;
        if (eventType == WatchEventType.Deleted)
        {
            _observedCustomResourceDefinitions.Remove(name);
            return true;
        }

        var generation = definition.Metadata?.Generation;
        var hasGeneration = generation is > 0;
        var established = IsEstablished(definition);
        var wasObserved = _observedCustomResourceDefinitions.TryGetValue(name, out var previous);
        var generationChanged = !hasGeneration
            || !wasObserved
            || previous.Generation != generation;
        var becameEstablished = established && (!wasObserved || !previous.Established);
        _observedCustomResourceDefinitions[name] = new ObservedCustomResourceDefinition(
            hasGeneration ? generation : null,
            established);

        if (eventType == WatchEventType.Added)
        {
            return true;
        }

        return generationChanged || becameEstablished;
    }

    private async Task WaitForDiscoveryRetryAsync(CancellationToken cancellationToken)
    {
        Task requestSignalTask;
        TimeSpan retryDelay;
        lock (_gate)
        {
            if (_stopping || _refreshRequested || _pendingEstablishedCustomResourceDefinitions.Count == 0)
            {
                return;
            }

            var multiplier = 1 << Math.Min(_retryAttempt, 5);
            var ticks = Math.Min(
                s_initialDiscoveryRetryDelay.Ticks * multiplier,
                s_maximumDiscoveryRetryDelay.Ticks);
            retryDelay = TimeSpan.FromTicks(ticks);
            _retryAttempt = Math.Min(_retryAttempt + 1, 6);
            requestSignalTask = _requestSignal.Task;
        }

        var delayTask = Task.Delay(retryDelay, cancellationToken);
        await Task.WhenAny(delayTask, requestSignalTask).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            if (!_stopping
                && !_refreshRequested
                && _pendingEstablishedCustomResourceDefinitions.Count > 0
                && delayTask.IsCompleted)
            {
                _refreshRequested = true;
                _immediateRefreshRequested = true;
                _requestVersion++;
            }
        }
    }

    private bool HasPendingEstablishedDefinitions(ApiDiscoverySnapshot? snapshot)
    {
        lock (_gate)
        {
            if (snapshot is not null)
            {
                var satisfiedNames = _pendingEstablishedCustomResourceDefinitions
                    .Where(pair => HasAllServedVersions(snapshot, pair.Value))
                    .Select(static pair => pair.Key)
                    .ToArray();
                foreach (var name in satisfiedNames)
                {
                    _pendingEstablishedCustomResourceDefinitions.Remove(name);
                }
            }

            if (_pendingEstablishedCustomResourceDefinitions.Count == 0)
            {
                _retryAttempt = 0;
                return false;
            }

            return true;
        }
    }

    private void CompleteImmediateRefreshWaiters(long completedRequestVersion)
    {
        List<TaskCompletionSource> completionSources = [];
        lock (_gate)
        {
            for (var index = _immediateRefreshWaiters.Count - 1; index >= 0; index--)
            {
                var waiter = _immediateRefreshWaiters[index];
                if (waiter.RequestVersion > completedRequestVersion)
                {
                    continue;
                }

                completionSources.Add(waiter.CompletionSource);
                _immediateRefreshWaiters.RemoveAt(index);
            }
        }

        foreach (var completionSource in completionSources)
        {
            completionSource.TrySetResult();
        }
    }

    private void CancelImmediateRefreshWaiters(CancellationToken cancellationToken)
    {
        List<TaskCompletionSource> completionSources = [];
        lock (_gate)
        {
            foreach (var waiter in _immediateRefreshWaiters)
            {
                completionSources.Add(waiter.CompletionSource);
            }

            _immediateRefreshWaiters.Clear();
        }

        foreach (var completionSource in completionSources)
        {
            completionSource.TrySetCanceled(cancellationToken);
        }
    }

    private static bool HasAllServedVersions(
        ApiDiscoverySnapshot snapshot,
        EstablishedCustomResourceDefinition definition)
        => definition.ServedVersions.All(version => snapshot.Resources.Any(resource =>
            string.Equals(resource.Kind.Group, definition.Group, StringComparison.Ordinal)
            && string.Equals(resource.Kind.Kind, definition.Kind, StringComparison.Ordinal)
            && string.Equals(resource.Kind.PluralName, definition.PluralName, StringComparison.Ordinal)
            && string.Equals(resource.Kind.ApiVersion, version, StringComparison.Ordinal)));

    private static bool TryCreateEstablishedDefinition(
        V1CustomResourceDefinition definition,
        out EstablishedCustomResourceDefinition establishedDefinition)
    {
        var spec = definition.Spec;
        var servedVersions = spec?.Versions?
            .Where(static version => version.Served && !string.IsNullOrWhiteSpace(version.Name))
            .Select(static version => version.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];
        if (spec is null
            || string.IsNullOrWhiteSpace(spec.Group)
            || string.IsNullOrWhiteSpace(spec.Names?.Kind)
            || string.IsNullOrWhiteSpace(spec.Names?.Plural)
            || servedVersions.Length == 0)
        {
            establishedDefinition = default!;
            return false;
        }

        establishedDefinition = new EstablishedCustomResourceDefinition(
            spec.Group,
            spec.Names.Kind,
            spec.Names.Plural,
            servedVersions);
        return true;
    }

    private static bool IsEstablished(V1CustomResourceDefinition definition)
        => definition.Status?.Conditions?.Any(static condition =>
            string.Equals(condition.Type, "Established", StringComparison.Ordinal)
            && string.Equals(condition.Status, "True", StringComparison.Ordinal)) == true;

    private readonly record struct ObservedCustomResourceDefinition(long? Generation, bool Established);

    private readonly record struct ImmediateRefreshWaiter(long RequestVersion, TaskCompletionSource CompletionSource);

    private sealed record EstablishedCustomResourceDefinition(
        string Group,
        string Kind,
        string PluralName,
        IReadOnlyList<string> ServedVersions);

    private static TaskCompletionSource CreateRequestSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
