// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Net.Sockets;
using k8s;
using k8s.Models;

#pragma warning disable CA2213 // Disposable fields should be disposed

namespace KubeUI.Client.Informer;

/// <summary>
/// Class ResourceInformer.
/// Implements the <see cref="IResourceInformer{TResource}" />.
/// Implements the <see cref="IDisposable" />.
/// </summary>
/// <typeparam name="TResource">The type of the t resource.</typeparam>
/// <seealso cref="IResourceInformer{TResource}" />
/// <seealso cref="IDisposable" />
public class ResourceInformer<TResource> : IResourceInformer<TResource>, IDisposable where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly object _sync = new();
    private readonly IKubernetes _client;
    private readonly GroupApiVersionKind _names;
    private readonly SemaphoreSlim _ready = new SemaphoreSlim(0);
    private ImmutableList<Registration> _registrations = [];
    private IDictionary<NamespacedName, IList<V1OwnerReference>> _cache = new Dictionary<NamespacedName, IList<V1OwnerReference>>();
    private string _lastResourceVersion;
    readonly ILogger<ResourceInformer<TResource>> _logger;
    private readonly string? _namespace;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceInformer{TResource}" /> class.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="logger">The logger.</param>
    public ResourceInformer(ILogger<ResourceInformer<TResource>> logger, IKubernetes client)
    {
        _names = GroupApiVersionKind.From<TResource>();
        _client = client;
        _logger = logger;
    }

    public ResourceInformer(ILogger<ResourceInformer<TResource>> logger, IKubernetes client, string @namespace)
    {
        _names = GroupApiVersionKind.From<TResource>();
        _namespace = @namespace;
        _client = client;
        _logger = logger;
    }

    private enum EventType
    {
        SynchronizeStarted = 101,
        SynchronizeComplete = 102,
        WatchingResource = 103,
        ReceivedError = 104,
        WatchingComplete = 105,
        InformerWatchEvent = 106,
        DisposingToReconnect = 107,
        IgnoringError = 108,
    }

    /// <inheritdoc/>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                _ready.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // ignore redundant exception to allow shutdown sequence to progress uninterupted
            }
        }
        //base.Dispose(disposing);
    }

    public virtual IResourceInformerRegistration Register(ResourceInformerCallback<TResource> callback)
    {
        return new Registration(this, callback);
    }

    public IResourceInformerRegistration Register(ResourceInformerCallback<IKubernetesObject<V1ObjectMeta>> callback)
    {
        return new Registration(this, (eventType, resource) => callback(eventType, resource));
    }

    /// <inheritdoc/>
    public virtual async Task ReadyAsync(CancellationToken cancellationToken)
    {
        await _ready.WaitAsync(cancellationToken).ConfigureAwait(false);

        // Release is called  after each WaitAync because
        // the semaphore is being used as a manual reset event
        _ready.Release();
    }

    /// <summary>
    /// RunAsync starts processing when StartAsync is called, and is terminated when
    /// StopAsync is called.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var limiter = new Limiter(new Limit(0.2), 3);
        var firstSync = true;

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                EventId(EventType.WatchingResource),
                "Starting Informer on {ResourceType}",
                typeof(TResource).Name);

            var shouldSync = true;

            try
            {
                if (shouldSync)
                {
                    await ListAsync(cancellationToken).ConfigureAwait(true);
                    shouldSync = false;
                }

                if (firstSync)
                {
                    _ready.Release();
                    firstSync = false;
                }

                await WatchAsync(cancellationToken).ConfigureAwait(true);
            }
            catch (IOException ex) when (ex.InnerException is SocketException)
            {
                _logger.LogDebug(
                    EventId(EventType.ReceivedError),
                    "Received error watching {ResourceType}: {ErrorMessage}",
                    typeof(TResource).Name,
                    ex.Message);
            }
            catch (KubernetesException ex)
            {
                _logger.LogDebug(
                    EventId(EventType.ReceivedError),
                    "Received error watching {ResourceType}: {ErrorMessage}",
                    typeof(TResource).Name,
                    ex.Message);

                // deal with this non-recoverable condition "too old resource version"
                // with a re-sync to listing everything again ensuring no subscribers miss updates
                if (ex is KubernetesException kubernetesError)
                {
                    if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        shouldSync = true;
                    }
                }
            }
            catch (Exception error)
            {
                _logger.LogError(
                    EventId(EventType.WatchingComplete),
                    error,
                    "No longer watching {ResourceType} resources from API server.",
                    typeof(TResource).Name);
            }
            // rate limiting the reconnect loop
            await limiter.WaitAsync(cancellationToken).ConfigureAwait(true);
        }
    }

    private static EventId EventId(EventType eventType) => new EventId((int)eventType, eventType.ToString());

    private async Task ListAsync(CancellationToken cancellationToken)
    {
        var previousCache = _cache;
        _cache = new Dictionary<NamespacedName, IList<V1OwnerReference>>();

        _logger.LogInformation(
            EventId(EventType.SynchronizeStarted),
            "Started synchronizing {ResourceType} resources from API server.",
            typeof(TResource).Name);

        string? continueParameter = null;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            // request next page of items
            k8s.Autorest.HttpOperationResponse<object>? listWithHttpMessage = null;

            if (string.IsNullOrEmpty(_namespace))
            {
                listWithHttpMessage = await _client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(
                _names.Group,
                _names.ApiVersion,
                _names.PluralName,
                continueParameter: continueParameter,
                cancellationToken: cancellationToken);
            }
            else
            {
                listWithHttpMessage = await _client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
                _names.Group,
                _names.ApiVersion,
                _namespace,
                _names.PluralName,
                continueParameter: continueParameter,
                cancellationToken: cancellationToken);
            }

            using (listWithHttpMessage)
            {
                var list = KubernetesJson.Deserialize<KubernetesList<TResource>>(listWithHttpMessage.Body.ToString());

                foreach (var item in list.Items)
                {
                    // These properties are not already set on items while listing
                    // assigned here for consistency
                    item.ApiVersion = _names.GroupApiVersion;
                    item.Kind = _names.Kind;

                    var key = NamespacedName.From(item);
                    _cache[key] = item?.Metadata?.OwnerReferences;

                    var watchEventType = WatchEventType.Added;
                    if (previousCache.Remove(key))
                    {
                        // an already-known key is provided as a modification for re-sync purposes
                        watchEventType = WatchEventType.Modified;
                    }

                    InvokeRegistrationCallbacks(watchEventType, item);
                }

                foreach (var (key, value) in previousCache)
                {
                    // for anything which was previously known but not part of list
                    // send a deleted notification to clear any observer caches
                    var item = new TResource()
                    {
                        ApiVersion = _names.GroupApiVersion,
                        Kind = _names.Kind,
                        Metadata = new V1ObjectMeta(
                            name: key.Name,
                            namespaceProperty: key.Namespace,
                            ownerReferences: value),
                    };

                    InvokeRegistrationCallbacks(WatchEventType.Deleted, item);
                }

                // keep track of values needed for next page and to start watching
                _lastResourceVersion = list.ResourceVersion();
                continueParameter = list.Continue();
            }
        }
        while (!string.IsNullOrEmpty(continueParameter));

        _logger.LogInformation(
            EventId(EventType.SynchronizeComplete),
            "Completed synchronizing {ResourceType} resources from API server.",
            typeof(TResource).Name);
    }

    private async Task WatchAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            EventId(EventType.WatchingResource),
            "Watching {ResourceType} starting from resource version {ResourceVersion}.",
            typeof(TResource).Name,
            _lastResourceVersion);

        // completion source helps turn OnClose callback into something awaitable
        var watcherCompletionSource = new TaskCompletionSource<int>();

        // begin watching where list left off
        Task<k8s.Autorest.HttpOperationResponse<object>> watchWithHttpMessage = null;

        if (string.IsNullOrEmpty(_namespace))
        {
            watchWithHttpMessage = _client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(
            _names.Group,
            _names.ApiVersion,
            _names.PluralName,
            watch: true,
            resourceVersion: _lastResourceVersion,
            cancellationToken: cancellationToken);
        }
        else
        {
            watchWithHttpMessage = _client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            _names.Group,
            _names.ApiVersion,
            _namespace,
            _names.PluralName,
            watch: true,
            resourceVersion: _lastResourceVersion,
            cancellationToken: cancellationToken);
        }

        var lastEventUtc = DateTime.UtcNow;

        using var watcher = watchWithHttpMessage.Watch<TResource, object>(
            (watchEventType, item) =>
            {
                if (!watcherCompletionSource.Task.IsCompleted)
                {
                    lastEventUtc = DateTime.UtcNow;
                    OnEvent(watchEventType, item);
                }
            },
            error =>
            {
                if (error is KubernetesException kubernetesError)
                {
                    // deal with this non-recoverable condition "too old resource version"
                    if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        // cause this error to surface
                        watcherCompletionSource.TrySetException(error);
                        throw error;
                    }
                }

                if (error is HttpRequestException requestException)
                {
                    watcherCompletionSource.TrySetException(error);
                    throw error;
                }

                _logger.LogDebug(
                    EventId(EventType.IgnoringError),
                    "Ignoring error {ErrorType}: {ErrorMessage}",
                    error.GetType().Name,
                    error.Message);
            },
            () =>
            {
                watcherCompletionSource.TrySetResult(0);
            });

        // reconnect if no events have arrived after a certain time
        using var checkLastEventUtcTimer = new Timer(
            _ =>
            {
                var lastEvent = DateTime.UtcNow - lastEventUtc;
                if (lastEvent > TimeSpan.FromMinutes(9.5))
                {
                    lastEventUtc = DateTime.MaxValue;
                    _logger.LogDebug(
                        EventId(EventType.DisposingToReconnect),
                        "Disposing watcher for {ResourceType} to cause reconnect.",
                        typeof(TResource).Name);

                    watcherCompletionSource.TrySetCanceled();
                    watcher.Dispose();
                }
            },
            state: null,
            dueTime: TimeSpan.FromSeconds(45),
            period: TimeSpan.FromSeconds(45));

        using var registration = cancellationToken.Register(watcher.Dispose);
        try
        {
            await watcherCompletionSource.Task;
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void OnEvent(WatchEventType watchEventType, TResource item)
    {
        if (watchEventType != WatchEventType.Modified)
        {
            _logger.LogDebug(
                EventId(EventType.InformerWatchEvent),
                "Informer {ResourceType} received {WatchEventType} notification for {ItemKind}/{ItemName}.{ItemNamespace} at resource version {ResourceVersion}",
                typeof(TResource).Name,
                watchEventType,
                item.Kind,
                item.Name(),
                item.Namespace(),
                item.ResourceVersion());
        }

        if (watchEventType == WatchEventType.Added ||
            watchEventType == WatchEventType.Modified)
        {
            // BUGBUG: log warning if cache was not in expected state
            _cache[NamespacedName.From(item)] = item.Metadata?.OwnerReferences;
        }

        if (watchEventType == WatchEventType.Deleted)
        {
            _cache.Remove(NamespacedName.From(item));
        }

        if (watchEventType == WatchEventType.Added ||
            watchEventType == WatchEventType.Modified ||
            watchEventType == WatchEventType.Deleted ||
            watchEventType == WatchEventType.Bookmark)
        {
            _lastResourceVersion = item.ResourceVersion();
        }

        if (watchEventType == WatchEventType.Added ||
            watchEventType == WatchEventType.Modified ||
            watchEventType == WatchEventType.Deleted)
        {
            InvokeRegistrationCallbacks(watchEventType, item);
        }
    }

    private void InvokeRegistrationCallbacks(WatchEventType eventType, TResource resource)
    {
        List<Exception>? innerExceptions = default;
        foreach (var registration in _registrations)
        {
            try
            {
                registration.Callback.Invoke(eventType, resource);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception innerException)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (innerExceptions == null)
                {
                    innerExceptions = new List<Exception>();
                }

                innerExceptions.Add(innerException);
            }
        }

        if (innerExceptions != null)
        {
            throw new AggregateException("One or more exceptions thrown by ResourceInformerCallback.", innerExceptions);
        }
    }

    public void Dispose()
    {
    }

    internal class Registration : IResourceInformerRegistration
    {
        private bool _disposedValue;

        public Registration(ResourceInformer<TResource> resourceInformer, ResourceInformerCallback<TResource> callback)
        {
            ResourceInformer = resourceInformer;
            Callback = callback;
            lock (resourceInformer._sync)
            {
                resourceInformer._registrations = resourceInformer._registrations.Add(this);
            }
        }

        ~Registration()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public ResourceInformer<TResource> ResourceInformer { get; }
        public ResourceInformerCallback<TResource> Callback { get; }

        public Task ReadyAsync(CancellationToken cancellationToken) => ResourceInformer.ReadyAsync(cancellationToken);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                lock (ResourceInformer._sync)
                {
                    ResourceInformer._registrations = ResourceInformer._registrations.Remove(this);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
