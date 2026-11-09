using System.Collections.Concurrent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

internal sealed class ApiDiscoveryRefreshCoordinator : IAsyncDisposable
{
    private static readonly TimeSpan s_customResourceDefinitionRefreshDebounce = TimeSpan.FromMilliseconds(350);

    private readonly Func<CancellationToken, Task> _refreshAsync;
    private readonly CancellationTokenSource _shutdownTokenSource;
    private readonly Lock _gate = new();
    private readonly ConcurrentDictionary<string, long> _observedCustomResourceDefinitionGenerations = new(StringComparer.Ordinal);
    private Task? _workerTask;
    private bool _refreshRequested;
    private bool _immediateRefreshRequested;
    private bool _stopping;
    private long _requestVersion;
    private TaskCompletionSource _requestSignal = CreateRequestSignal();

    internal ApiDiscoveryRefreshCoordinator(
        Func<CancellationToken, Task> refreshAsync,
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
        if (ShouldRefreshForCustomResourceDefinition(eventType, definition))
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
        requestSignal.TrySetResult();
        if (workerTask is not null)
        {
            await workerTask.ConfigureAwait(false);
        }

        _observedCustomResourceDefinitionGenerations.Clear();
        _shutdownTokenSource.Dispose();
    }

    private Task RequestRefresh(bool immediate)
    {
        TaskCompletionSource requestSignal;
        Task workerTask;
        lock (_gate)
        {
            if (_stopping)
            {
                return Task.CompletedTask;
            }

            _refreshRequested = true;
            _immediateRefreshRequested |= immediate;
            _requestVersion++;
            requestSignal = _requestSignal;
            _requestSignal = CreateRequestSignal();
            if (_workerTask is null || _workerTask.IsCompleted)
            {
                _workerTask = Task.Run(RunWorkerAsync);
            }

            workerTask = _workerTask;
        }

        requestSignal.TrySetResult();
        return workerTask;
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

                await _refreshAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
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
            _observedCustomResourceDefinitionGenerations.TryRemove(name, out _);
            return true;
        }

        if (eventType == WatchEventType.Added)
        {
            if (definition.Metadata?.Generation is long addedGeneration && addedGeneration > 0)
            {
                _observedCustomResourceDefinitionGenerations[name] = addedGeneration;
            }

            return true;
        }

        if (definition.Metadata?.Generation is not long generation || generation <= 0)
        {
            return true;
        }

        if (_observedCustomResourceDefinitionGenerations.TryGetValue(name, out var previousGeneration)
            && previousGeneration == generation)
        {
            return false;
        }

        _observedCustomResourceDefinitionGenerations[name] = generation;
        return true;
    }

    private static TaskCompletionSource CreateRequestSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
