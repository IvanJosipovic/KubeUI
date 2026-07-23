using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Workspace;

public sealed partial class ClusterWorkspace : ObservableObject, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterWorkspace> _logger;
    private readonly ConcurrentDictionary<GroupApiVersionKind, IResourceConfig> _resourceConfigs = new();
    private readonly ConcurrentDictionary<string, string> _customResourceDefinitionSignatures = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private bool _disposed;
    private bool _workspaceStateInitialized;

    private Instrumentation _instrumentation;

    public ClusterWorkspace(
        IClusterRuntime runtime,
        IServiceProvider serviceProvider,
        ILogger<ClusterWorkspace> logger, Instrumentation instrumentation)
    {
        Runtime = runtime;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _instrumentation = instrumentation;

        SubscribeRuntime();
        UpdateClusterColor();
    }

    [ObservableProperty]
    public partial IClusterRuntime Runtime { get; set; }

    public string Title => Runtime.Name;

    [ObservableProperty]
    public partial IBrush ClusterColor { get; set; } = Brushes.Red;

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];

    /// <summary>
    /// Raised after a resource config has been processed by the workspace.
    /// </summary>
    public event Action<ClusterWorkspace, IResourceConfig>? ResourceConfigProcessed;
    public event Action<ClusterWorkspace, GroupApiVersionKind>? CustomResourceDefinitionRemoved;

    private Activity? StartWorkspaceActivity(string activityName, ActivityKind activityKind = ActivityKind.Internal)
    {
        var activity = _instrumentation.Source.StartActivity(activityName, activityKind);
        activity?.SetTag("kubernetes.cluster.name", Runtime.Name);
        return activity;
    }

    public Task Connect() => Task.Run(ConnectCoreAsync);

    private async Task ConnectCoreAsync()
    {
        using var activity = StartWorkspaceActivity(nameof(Connect), ActivityKind.Client);
        try
        {
            EnsureBuiltInResourceConfigs();
            await Runtime.Connect().ConfigureAwait(false);
            if (!Runtime.Connected)
            {
                return;
            }

            if (_workspaceStateInitialized)
            {
                return;
            }

            await UpdateResourceConfigsPermissionsAsync().ConfigureAwait(false);
            await EvaluateResourceConfigAccessAsync().ConfigureAwait(false);
            await SeedResourcesConfiguredForConnectAsync().ConfigureAwait(false);
            _workspaceStateInitialized = true;
            await Dispatcher.UIThread.InvokeAsync(UpdateClusterColor);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            Runtime.LastError = ex.Message;
            Runtime.Status = ClusterStatus.Errored;
        }
    }

    private async Task  UpdateResourceConfigsPermissionsAsync(
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        using var activity = StartWorkspaceActivity(nameof(UpdateResourceConfigPermissionsAsync));

        var categoryBatches = (resourceConfigs ?? GetResourceConfigs())
            .GroupBy(static config => config.Category, StringComparer.Ordinal)
            .OrderBy(static category => ResourceCategories.GetOrder(category.Key, category.Min(config => config.Order)))
            .ThenBy(static category => category.Key, StringComparer.Ordinal);

        foreach (var categoryBatch in categoryBatches)
        {
            foreach (var orderBatch in categoryBatch.GroupBy(static config => config.Order).OrderBy(static batch => batch.Key))
            {
                await Parallel.ForEachAsync(
                    orderBatch,
                    new ParallelOptions { MaxDegreeOfParallelism = 4 },
                    async (resourceConfig, _) => await UpdateResourceConfigPermissionsAsync(resourceConfig).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }
    }

    private async Task UpdateResourceConfigPermissionsAsync(IResourceConfig resourceConfig)
    {
        foreach (var request in resourceConfig.AuthorizationRequests().Distinct())
        {
            await Runtime.Permissions
                .UpdatePermissionsAllNamespaceAsync(request.ResourceType, request.Verb, request.Subresource)
                .ConfigureAwait(false);
        }
    }

    public async Task Disconnect()
    {
        await Runtime.Disconnect().ConfigureAwait(false);
        _workspaceStateInitialized = false;
        ResetWorkspaceState();
    }

    public bool CanReadEvents(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var @namespace = resource.Metadata?.NamespaceProperty;
        return Runtime.Connected
            && Runtime.Permissions.CanI<Corev1Event>(Verb.List, @namespace)
            && Runtime.Permissions.CanI<Corev1Event>(Verb.Watch, @namespace);
    }

    public IResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        return _resourceConfigs[kind];
    }

    public IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceConfig(GroupApiVersionKind.From<T>());
    }

    public IEnumerable<IResourceConfig> GetResourceConfigs()
    {
        return _resourceConfigs.Values.ToList();
    }

    internal void AddResourceConfigForTest(IResourceConfig resourceConfig)
    {
        resourceConfig.Initialize(this);
        _resourceConfigs[resourceConfig.Kind] = resourceConfig;

        ProcessResourceConfigPermissionsUpdated(resourceConfig);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Runtime.OnChange -= OnRuntimeChange;
        Runtime.OnCustomResourceDefinitionReady -= HandleCustomResourceDefinitionReady;

        _disposeCancellation.Cancel();
        _disposeCancellation.Dispose();
    }

    private void EnsureBuiltInResourceConfigs()
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureBuiltInResourceConfigs));

        var serviceDescriptors = _serviceProvider.GetRequiredService<ServiceDescriptor[]>();

        var types = serviceDescriptors
            .Select(static descriptor => descriptor.ServiceType)
            .Where(static type => !type.IsAbstract
                && !type.ContainsGenericParameters
                && typeof(IResourceConfig).IsAssignableFrom(type))
            .Distinct()
            .ToList();

        foreach (var type in types)
        {
            var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(type);
            resourceConfig.Initialize(this);
            _resourceConfigs[resourceConfig.Kind] = resourceConfig;
        }
    }

    private async Task EvaluateResourceConfigAccessAsync(
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        using var activity = StartWorkspaceActivity(nameof(EvaluateResourceConfigAccessAsync));

        if (_disposed)
        {
            return;
        }

        try
        {
            var configSnapshot = resourceConfigs?.ToArray() ?? GetResourceConfigs().ToArray();

            if (configSnapshot.Length == 0)
            {
                return;
            }

            await Task.WhenAll(configSnapshot.Select(EvaluateResourceConfigAccessCoreAsync)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            _logger.LogDebug(ex, "Unable to evaluate workspace resource access.");
        }
    }

    private async Task EvaluateResourceConfigAccessCoreAsync(IResourceConfig resourceConfig)
    {
        ArgumentNullException.ThrowIfNull(resourceConfig);

        try
        {
            await resourceConfig.EvaluateListWatchAccessAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Activity.Current?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            _logger.LogDebug(ex, "Unable to evaluate permissions for {Kind}", resourceConfig.Kind);
        }

        ProcessResourceConfigPermissionsUpdated(resourceConfig);
    }

    private async Task<IResourceConfig?> BuildCustomResourceConfigAsync(V1CustomResourceDefinition crd)
    {
        using var activity = StartWorkspaceActivity(nameof(BuildCustomResourceConfigAsync));
        activity?.SetTag("kubernetes.crd.name", crd.Name());

        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage);
        if (version == null)
        {
            return null;
        }

        var resourceType = Runtime.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
        if (resourceType == null)
        {
            return null;
        }

        var resourceConfigType = typeof(CRDResourceConfig<>).MakeGenericType(resourceType);
        var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(resourceConfigType);

        resourceConfig.Initialize(this);
        await UpdateResourceConfigsPermissionsAsync([resourceConfig]).ConfigureAwait(false);
        await EvaluateResourceConfigAccessCoreAsync(resourceConfig).ConfigureAwait(false);

        if (!resourceConfig.CanListAndWatch)
        {
            return null;
        }

        if (resourceConfig is not ICustomResourceConfig customResourceConfig)
        {
            return null;
        }

        using var generationActivity = StartWorkspaceActivity("GenerateCustomResourceConfig");
        generationActivity?.SetTag("kubernetes.crd.name", crd.Name());
        customResourceConfig.Generate(crd);
        generationActivity?.Stop();

        return resourceConfig;
    }

    private async Task SeedResourcesConfiguredForConnectAsync()
    {
        using var activity = StartWorkspaceActivity(nameof(SeedResourcesConfiguredForConnectAsync));

        var seedBatches = _resourceConfigs.Values
            .Where(static config => config.SeedOnConnect && config.PermissionsLoaded && config.CanListAndWatch)
            .GroupBy(static config => config.Order)
            .OrderBy(static batch => batch.Key);

        foreach (var seedBatch in seedBatches)
        {
            await Task.WhenAll(seedBatch.Select(EnsureResourceSeededAsync)).ConfigureAwait(false);
        }
    }

    private async Task EnsureResourceSeededAsync(IResourceConfig resourceConfig)
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureResourceSeededAsync));

        if (Runtime.Objects.TryGetValue(resourceConfig.Kind, out var existing)
            && existing is IResourceContainer { IsSeeded: true })
        {
            return;
        }

        await Runtime.SeedResource(resourceConfig.Type).ConfigureAwait(false);
    }

    private void ProcessResourceConfigPermissionsUpdated(IResourceConfig resourceConfig)
    {
        using var activity = StartWorkspaceActivity(nameof(ProcessResourceConfigPermissionsUpdated));

        ResourceConfigProcessed?.Invoke(this, resourceConfig);
    }

    private void SubscribeRuntime()
    {
        Runtime.OnChange += OnRuntimeChange;
        Runtime.OnCustomResourceDefinitionReady += HandleCustomResourceDefinitionReady;
    }

    private void ResetWorkspaceState()
    {
        var removedCustomResourceKinds = _resourceConfigs
            .Where(static pair => pair.Value.IsCustomResource)
            .Select(static pair => pair.Key)
            .ToList();

        if (removedCustomResourceKinds.Count == 0)
        {
            return;
        }

        foreach (var removedKind in removedCustomResourceKinds)
        {
            _resourceConfigs.TryRemove(removedKind, out _);
        }

        _customResourceDefinitionSignatures.Clear();

        foreach (var removedKind in removedCustomResourceKinds)
        {
            NotifyCustomResourceDefinitionRemoved(removedKind);
        }
    }

    private void OnRuntimeChange(WatchEventType eventType, GroupApiVersionKind kind, IKubernetesObject<V1ObjectMeta> item)
    {
        if (item is not V1CustomResourceDefinition crd)
        {
            return;
        }

        if (eventType == WatchEventType.Deleted)
        {
            RemoveCustomResourceDefinition(crd);
            return;
        }
    }

    private void HandleCustomResourceDefinitionReady(V1CustomResourceDefinition crd)
    {
        _ = ProcessCustomResourceDefinitionAsync(crd);
    }

    private async Task ProcessCustomResourceDefinitionAsync(V1CustomResourceDefinition crd)
    {
        using var activity = StartWorkspaceActivity(nameof(ProcessCustomResourceDefinitionAsync));
        activity?.SetTag("kubernetes.crd.name", crd.Name());

        try
        {
            var signature = GetCustomResourceDefinitionSignature(crd);
            if (_customResourceDefinitionSignatures.TryGetValue(crd.Name(), out var existingSignature)
                && string.Equals(existingSignature, signature, StringComparison.Ordinal))
            {
                return;
            }

            var builtConfig = await BuildCustomResourceConfigAsync(crd).ConfigureAwait(false);
            if (builtConfig == null)
            {
                return;
            }

            _customResourceDefinitionSignatures[crd.Name()] = signature;
            _resourceConfigs[builtConfig.Kind] = builtConfig;
            ProcessResourceConfigPermissionsUpdated(builtConfig);
        }
        catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error processing custom resource definition {Crd}", crd.Name());
        }
    }

    private void RemoveCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        _customResourceDefinitionSignatures.TryRemove(crd.Name(), out _);
        var removedKind = TryResolveCustomResourceKind(crd);
        if (removedKind == null)
        {
            return;
        }

        _resourceConfigs.TryRemove(removedKind.Value, out _);
        NotifyCustomResourceDefinitionRemoved(removedKind.Value);
    }


    private GroupApiVersionKind? TryResolveCustomResourceKind(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage);
        if (version == null)
        {
            return null;
        }

        var resourceType = Runtime.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
        if (resourceType != null)
        {
            return GroupApiVersionKind.From(resourceType);
        }

        foreach (var resourceKind in _resourceConfigs.Keys)
        {
            if (string.Equals(resourceKind.Group, crd.Spec.Group, StringComparison.Ordinal)
                && string.Equals(resourceKind.ApiVersion, version.Name, StringComparison.Ordinal)
                && string.Equals(resourceKind.Kind, crd.Spec.Names.Kind, StringComparison.Ordinal))
            {
                return resourceKind;
            }
        }

        return null;
    }

    private static string GetCustomResourceDefinitionSignature(V1CustomResourceDefinition crd)
    {
        return KubernetesJson.Serialize(crd.Spec);
    }

    private void NotifyCustomResourceDefinitionRemoved(GroupApiVersionKind kind)
    {
        CustomResourceDefinitionRemoved?.Invoke(this, kind);
    }

    private void UpdateClusterColor()
    {
        ClusterColor = Runtime.Status switch
        {
            ClusterStatus.Connecting => Brushes.Orange,
            ClusterStatus.Errored => Brushes.Red,
            ClusterStatus.Connected => GetConnectedBrush(),
            _ => Brushes.Red,
        };
    }

    private IBrush GetConnectedBrush()
    {
        if (ClusterColor != Brushes.Red && ClusterColor != Brushes.Orange)
        {
            return ClusterColor;
        }

        var properties = typeof(Brushes)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(Brushes.Red) && x.Name != nameof(Brushes.Orange))
            .ToArray();

        return (IBrush)properties[RandomNumberGenerator.GetInt32(properties.Length)].GetValue(null)!;
    }
}
