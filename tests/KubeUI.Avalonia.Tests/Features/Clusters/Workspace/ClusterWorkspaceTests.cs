using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Workspace;

public class ClusterWorkspaceTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];

    public override void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    [AvaloniaFact]
    public void creating_workspace_does_not_initialize_resource_configs_until_requested()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var workspace = CreateWorkspace(runtime);

        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task added_crd_adds_resource_config_and_model_cache_entry()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(crd);

        var resourceType = await WaitForValueAsync(() => GetCustomResourceType(runtime, crd));
        resourceType.ShouldNotBeNull();

        var resourceConfig = await WaitForValueAsync(() => GetCustomResourceConfig(workspace, crd));
        resourceConfig.ShouldNotBeNull();
        resourceConfig.Type.ShouldBe(resourceType);
        resourceConfig.IsCustomResource.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task updated_crd_replaces_resource_config_model_cache_entry_and_seeded_informer()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(originalCrd);

        var originalType = await WaitForValueAsync(() => GetCustomResourceType(runtime, originalCrd));
        originalType.ShouldNotBeNull();
        await SeedResourceAsync(runtime, originalType);

        var originalContainer = GetSeededContainer(runtime, originalType);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        await runtime.AddOrUpdateResource(updatedCrd);

        var updatedType = await WaitForValueAsync(() => GetCustomResourceType(runtime, updatedCrd));
        updatedType.ShouldNotBeNull();
        updatedType.ShouldNotBe(originalType);

        var updatedResourceConfig = await WaitForValueAsync(() => GetCustomResourceConfig(workspace, updatedCrd));
        updatedResourceConfig.ShouldNotBeNull();
        updatedResourceConfig.Type.ShouldBe(updatedType);

        var updatedContainer = await WaitForValueAsync(() => GetSeededContainer(runtime, updatedType));
        updatedContainer.ShouldNotBeNull();
        updatedContainer.ShouldNotBeSameAs(originalContainer);
        GetInformers(originalContainer).Count.ShouldBe(0);
        GetInformers(updatedContainer).Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task metadata_only_crd_update_does_not_rebuild_resource_config_or_reseed_informer()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(originalCrd);

        var originalType = await WaitForValueAsync(() => GetCustomResourceType(runtime, originalCrd));
        originalType.ShouldNotBeNull();
        await SeedResourceAsync(runtime, originalType);

        var originalContainer = GetSeededContainer(runtime, originalType);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var originalResourceConfig = await WaitForValueAsync(() => GetCustomResourceConfig(workspace, originalCrd));
        originalResourceConfig.ShouldNotBeNull();

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        updatedCrd.Metadata.Annotations = new Dictionary<string, string>
        {
            ["metadata-only"] = "true"
        };

        await runtime.AddOrUpdateResource(updatedCrd);

        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var updatedResourceConfig = GetCustomResourceConfig(workspace, updatedCrd);
        updatedResourceConfig.ShouldBeSameAs(originalResourceConfig);
        GetInformers(originalContainer).Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task seeding_resource_raises_resource_seeded_event()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        GroupApiVersionKind? seededKind = null;
        runtime.ResourceSeeded += (_, resourceKind) => seededKind = resourceKind;

        await workspace.SeedResource<V1Pod>();

        seededKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
    }

    [AvaloniaFact]
    public async Task failed_resource_seed_does_not_raise_resource_seeded_event()
    {
        var runtime = new TestClusterRuntime
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            DefaultPermissionAllowed = false,
        };
        var workspace = CreateWorkspace(runtime);
        var seeded = false;
        runtime.ResourceSeeded += (_, _) => seeded = true;

        await workspace.SeedResource<Corev1Event>();

        seeded.ShouldBeFalse();
        runtime.Objects[GroupApiVersionKind.From<Corev1Event>()]
            .ShouldBeOfType<ContainerClass<Corev1Event>>()
            .Informers.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task event_resource_seeding_uses_default_wait_for_ready_behavior()
    {
        var runtime = new TestClusterRuntime();
        var countingRuntime = new CountingClusterRuntime(runtime)
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."),
            countingRuntime);
        _disposables.Add(workspace);

        await workspace.Connect();

        countingRuntime.EventSeedCalls.ShouldBe(1);
        (countingRuntime.EventSeedWaitForReady == false).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task event_resource_seeding_does_not_use_wait_for_ready()
    {
        var runtime = new TestClusterRuntime();
        var countingRuntime = new CountingClusterRuntime(runtime)
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(countingRuntime);

        await workspace.Connect();

        countingRuntime.EventSeedCalls.ShouldBe(1);
        (countingRuntime.EventSeedWaitForReady == false).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task initializing_workspace_seeds_custom_resource_definitions_when_allowed()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        await workspace.Connect();

        var kind = GroupApiVersionKind.From<V1CustomResourceDefinition>();
        runtime.Objects.TryGetValue(kind, out var container).ShouldBeTrue();
        container.ShouldBeOfType<ContainerClass<V1CustomResourceDefinition>>()
            .Informers.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task deleted_crd_removes_resource_config_model_cache_entry_and_seeded_informer()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(crd);

        var resourceType = await WaitForValueAsync(() => GetCustomResourceType(runtime, crd));
        resourceType.ShouldNotBeNull();
        await SeedResourceAsync(runtime, resourceType);

        var seededContainer = GetSeededContainer(runtime, resourceType);
        seededContainer.ShouldNotBeNull();
        GetInformers(seededContainer).Count.ShouldBe(1);

        await runtime.DeleteResource(crd);

        await WaitForAsync(() => GetCustomResourceConfig(workspace, crd) == null);
        GetCustomResourceType(runtime, crd).ShouldBeNull();
        runtime.Objects.ContainsKey(GroupApiVersionKind.From(resourceType)).ShouldBeFalse();
        GetInformers(seededContainer).Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task seeding_namespaced_resource_uses_known_namespaces_without_eager_resource_config_initialization()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-a" }
        });

        workspace.GetResourceConfigs().ShouldBeEmpty();

        await workspace.SeedResource<V1Pod>();

        runtime.GetResource<V1Namespace>(null, "team-a").ShouldNotBeNull();
        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task seeding_namespaced_resource_creates_informers_for_each_known_namespace_with_list_and_watch_access()
    {
        var runtime = new TestCluster { DefaultPermissionAllowed = false };
        runtime.SetPermission<V1Pod>(Verb.List, true, "team-a");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "team-a");
        runtime.SetPermission<V1Pod>(Verb.List, true, "team-b");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "team-b");

        var workspace = CreateWorkspace(runtime);
        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-a" }
        });
        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-b" }
        });

        await workspace.SeedResource<V1Pod>();

        runtime.GetResource<V1Namespace>(null, "team-a").ShouldNotBeNull();
        runtime.GetResource<V1Namespace>(null, "team-b").ShouldNotBeNull();

        var container = GetSeededContainer(runtime, typeof(V1Pod));
        container.ShouldNotBeNull();
        GetInformers(container).Count.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task disconnect_disposes_seeded_informers_and_registrations()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        await workspace.SeedResource<V1Pod>();

        var container = GetSeededContainer(runtime, typeof(V1Pod));
        container.ShouldNotBeNull();

        var informers = GetInformers(container);
        informers.Count.ShouldBe(1);
        var informer = informers[0].ShouldBeOfType<TestResourceInformer>();

        var registrations = GetInformerRegistrations(container);
        registrations.Count.ShouldBe(1);
        var registration = registrations[0].ShouldBeOfType<TestResourceInformerRegistration>();

        await workspace.Disconnect();

        informer.Disposed.ShouldBeTrue();
        registration.Disposed.ShouldBeTrue();
        runtime.Objects.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task disconnect_allows_resource_types_to_be_seeded_again()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        await workspace.SeedResource<V1Pod>();

        var initialContainer = GetSeededContainer(runtime, typeof(V1Pod));
        initialContainer.ShouldNotBeNull();
        GetInformers(initialContainer).Count.ShouldBe(1);

        await workspace.Disconnect();

        runtime.Objects.ShouldBeEmpty();

        await workspace.SeedResource<V1Pod>();

        var reseededContainer = GetSeededContainer(runtime, typeof(V1Pod));
        reseededContainer.ShouldNotBeNull();
        GetInformers(reseededContainer).Count.ShouldBe(1);
        GetInformerRegistrations(reseededContainer).Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task disconnect_removes_dynamic_crd_model_cache_entries()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(crd);

        var resourceType = await WaitForValueAsync(() => GetCustomResourceType(runtime, crd));
        resourceType.ShouldNotBeNull();

        await workspace.Disconnect();

        GetCustomResourceType(runtime, crd).ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task connect_skips_workspace_initialization_when_runtime_remains_disconnected()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.Errored,
            ConnectBehavior = () => Task.CompletedTask,
        };

        var workspace = CreateWorkspace(runtime);

        await workspace.Connect();

        runtime.Connected.ShouldBeFalse();
        workspace.GetResourceConfigs().ShouldNotBeEmpty();
    }

    [AvaloniaFact]
    public async Task added_crd_does_not_refresh_authorization_index_for_generated_resource()
    {
        var runtime = new RecordingAuthorizationClusterRuntime(new TestCluster());
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."),
            runtime);
        _disposables.Add(workspace);

        await workspace.Connect();
        var authorizationPlanCount = runtime.RecordedAuthorizationRequests.Count;

        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await runtime.AddOrUpdateResource(crd);

        var resourceType = await WaitForValueAsync(() => GetCustomResourceType(runtime.Inner, crd));
        resourceType.ShouldNotBeNull();

        await WaitForAsync(() => GetCustomResourceConfig(workspace, crd) != null);

        runtime.RecordedAuthorizationRequests.Count.ShouldBe(authorizationPlanCount);
    }

    private ClusterWorkspace CreateWorkspace(TestCluster runtime)
    {
        var workspace = runtime.CreateWorkspace();
        _disposables.Add(workspace);
        return workspace;
    }

    private ClusterWorkspace CreateWorkspace(IClusterRuntime runtime)
    {
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."),
            runtime);

        _disposables.Add(workspace);
        return workspace;
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static async Task<T?> WaitForValueAsync<T>(Func<T?> valueFactory, int timeoutMs = 10000) where T : class
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            var value = valueFactory();
            if (value != null)
            {
                return value;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        return valueFactory();
    }

    private static Type? GetCustomResourceType(TestClusterRuntime runtime, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        return version == null ? null : runtime.ModelCache.GetResourceType(crd.Spec.Group, version, crd.Spec.Names.Kind);
    }

    private static IResourceConfig? GetCustomResourceConfig(ClusterWorkspace workspace, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        if (version == null)
        {
            return null;
        }

        return workspace.GetResourceConfigs().FirstOrDefault(x =>
            x.IsCustomResource
            && string.Equals(x.Kind.Group, crd.Spec.Group, StringComparison.Ordinal)
            && string.Equals(x.Kind.ApiVersion, version, StringComparison.Ordinal)
            && string.Equals(x.Kind.Kind, crd.Spec.Names.Kind, StringComparison.Ordinal));
    }

    private static async Task SeedResourceAsync(TestClusterRuntime runtime, Type resourceType)
    {
        var method = runtime.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(TestClusterRuntime.SeedResource) && x.IsGenericMethodDefinition && x.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        await (Task)method.Invoke(runtime, [false])!;
    }

    private static object? GetSeededContainer(TestClusterRuntime runtime, Type resourceType)
    {
        return runtime.Objects.TryGetValue(GroupApiVersionKind.From(resourceType), out var container)
            ? container
            : null;
    }

    private static IList<IResourceInformer> GetInformers(object container)
    {
        return (IList<IResourceInformer>)(container.GetType().GetProperty("Informers")?.GetValue(container)
            ?? throw new InvalidOperationException("Container does not expose Informers."));
    }

    private static IList<IResourceInformerRegistration> GetInformerRegistrations(object container)
    {
        return (IList<IResourceInformerRegistration>)(container.GetType().GetProperty("InformerRegistrations")?.GetValue(container)
            ?? throw new InvalidOperationException("Container does not expose InformerRegistrations."));
    }
}

internal sealed class CountingClusterRuntime : IClusterRuntime, INotifyPropertyChanged
{
    private readonly TestClusterRuntime _inner;
    private event Action<IClusterRuntime>? NamespaceSelectionRequiredCore;
    private event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeededCore;

    public CountingClusterRuntime(TestClusterRuntime inner)
    {
        _inner = inner;
        _inner.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
        _inner.NamespaceSelectionRequired += ForwardNamespaceSelectionRequired;
        _inner.ResourceSeeded += ForwardResourceSeeded;
    }

    private void ForwardNamespaceSelectionRequired(IClusterRuntime _) => NamespaceSelectionRequiredCore?.Invoke(this);

    private void ForwardResourceSeeded(IClusterRuntime _, GroupApiVersionKind kind) => ResourceSeededCore?.Invoke(this, kind);

    public int EventSeedCalls { get; private set; }
    public bool EventSeedWaitForReady { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange
    {
        add => _inner.OnChange += value;
        remove => _inner.OnChange -= value;
    }

    public event Action<V1CustomResourceDefinition>? OnCustomResourceDefinitionReady
    {
        add => _inner.OnCustomResourceDefinitionReady += value;
        remove => _inner.OnCustomResourceDefinitionReady -= value;
    }

    public event Action<IClusterRuntime>? NamespaceSelectionRequired
    {
        add => NamespaceSelectionRequiredCore += value;
        remove => NamespaceSelectionRequiredCore -= value;
    }

    public event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeeded
    {
        add => ResourceSeededCore += value;
        remove => ResourceSeededCore -= value;
    }

    public IReadOnlyDictionary<GroupApiVersionKind, object> Objects => _inner.Objects;
    public bool Connected { get => _inner.Connected; set => _inner.Connected = value; }
    public ClusterStatus Status { get => _inner.Status; set => _inner.Status = value; }
    public string? LastError { get => _inner.LastError; set => _inner.LastError = value; }
    public bool IsMetricsAvailable => _inner.IsMetricsAvailable;
    public bool ListNamespaces { get => _inner.ListNamespaces; set => _inner.ListNamespaces = value; }
    public IKubernetes? Client { get => _inner.Client; set => _inner.Client = value; }
    public K8SConfiguration KubeConfig { get => _inner.KubeConfig; set => _inner.KubeConfig = value; }
    public ModelCache ModelCache { get => _inner.ModelCache; set => _inner.ModelCache = value; }
    public string KubeConfigPath { get => _inner.KubeConfigPath; set => _inner.KubeConfigPath = value; }
    public string Name { get => _inner.Name; set => _inner.Name = value; }
    public ReadOnlyObservableCollection<V1Namespace> Namespaces => _inner.Namespaces;
    public ObservableCollection<NodeMetrics> NodeMetrics => _inner.NodeMetrics;
    public ObservableCollection<PodMetrics> PodMetrics => _inner.PodMetrics;
    public ObservableCollection<PortForwarder> PortForwarders => _inner.PortForwarders;
    public IClusterAuthorization Permissions => _inner.Permissions;
    public bool IsResourceNamespaced(Type type) => _inner.IsResourceNamespaced(type);
    public bool IsResourceNamespaced<T>() => _inner.IsResourceNamespaced<T>();
    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort) => _inner.AddPodPortForward(@namespace, podName, containerPort);
    public Task AddPodEphemeralDebugContainer(V1Pod pod, string? targetContainerName, string image) => _inner.AddPodEphemeralDebugContainer(pod, targetContainerName, image);
    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort) => _inner.AddServicePortForward(@namespace, serviceName, servicePort);
    public void RemovePortForward(PortForwarder pf) => _inner.RemovePortForward(pf);
    public Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.AddOrUpdateResource(item);
    public Task Connect() => _inner.Connect();
    public Task Disconnect() => _inner.Disconnect();
    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.DeleteResource(item);
    public Task DryRunYaml(Stream stream) => _inner.DryRunYaml(stream);
    public Task ImportFolder(string path) => _inner.ImportFolder(path);
    public Task ImportYaml(Stream stream) => _inner.ImportYaml(stream);
    public Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.IsResourceReady<T>(token);
    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResource<T>(@namespace, name);
    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceList<T>();
    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceSourceCache<T>();
    public IObservable<int> GetResourceCount(Type type) => _inner.GetResourceCount(type);
    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceCount<T>();

    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (typeof(T) == typeof(global::k8s.Models.Corev1Event))
        {
            EventSeedCalls++;
            EventSeedWaitForReady = waitForReady;
        }

        return _inner.SeedResource<T>(waitForReady);
    }

    public Task SeedResource(Type resourceType, bool waitForReady = false)
    {
        if (resourceType == typeof(Corev1Event))
        {
            EventSeedCalls++;
            EventSeedWaitForReady = waitForReady;
        }

        return _inner.SeedResource(resourceType, waitForReady);
    }
}

internal sealed class RecordingAuthorizationClusterRuntime : IClusterRuntime, IClusterAuthorization, INotifyPropertyChanged
{
    private readonly TestClusterRuntime _inner;
    private event Action<IClusterRuntime>? NamespaceSelectionRequiredCore;
    private event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeededCore;

    public RecordingAuthorizationClusterRuntime(TestClusterRuntime inner)
    {
        _inner = inner;
        _inner.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
        _inner.NamespaceSelectionRequired += ForwardNamespaceSelectionRequired;
        _inner.ResourceSeeded += ForwardResourceSeeded;
    }

    private void ForwardNamespaceSelectionRequired(IClusterRuntime _) => NamespaceSelectionRequiredCore?.Invoke(this);

    private void ForwardResourceSeeded(IClusterRuntime _, GroupApiVersionKind kind) => ResourceSeededCore?.Invoke(this, kind);

    public TestClusterRuntime Inner => _inner;
    public List<AuthorizationRequest[]> RecordedAuthorizationRequests { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange
    {
        add => _inner.OnChange += value;
        remove => _inner.OnChange -= value;
    }

    public event Action<V1CustomResourceDefinition>? OnCustomResourceDefinitionReady
    {
        add => _inner.OnCustomResourceDefinitionReady += value;
        remove => _inner.OnCustomResourceDefinitionReady -= value;
    }

    public event Action<IClusterRuntime>? NamespaceSelectionRequired
    {
        add => NamespaceSelectionRequiredCore += value;
        remove => NamespaceSelectionRequiredCore -= value;
    }

    public event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeeded
    {
        add => ResourceSeededCore += value;
        remove => ResourceSeededCore -= value;
    }

    public IReadOnlyDictionary<GroupApiVersionKind, object> Objects => _inner.Objects;
    public bool Connected { get => _inner.Connected; set => _inner.Connected = value; }
    public ClusterStatus Status { get => _inner.Status; set => _inner.Status = value; }
    public string? LastError { get => _inner.LastError; set => _inner.LastError = value; }
    public bool IsMetricsAvailable => _inner.IsMetricsAvailable;
    public bool ListNamespaces { get => _inner.ListNamespaces; set => _inner.ListNamespaces = value; }
    public IKubernetes? Client { get => _inner.Client; set => _inner.Client = value; }
    public K8SConfiguration KubeConfig { get => _inner.KubeConfig; set => _inner.KubeConfig = value; }
    public ModelCache ModelCache { get => _inner.ModelCache; set => _inner.ModelCache = value; }
    public string KubeConfigPath { get => _inner.KubeConfigPath; set => _inner.KubeConfigPath = value; }
    public string Name { get => _inner.Name; set => _inner.Name = value; }
    public ReadOnlyObservableCollection<V1Namespace> Namespaces => _inner.Namespaces;
    public ObservableCollection<NodeMetrics> NodeMetrics => _inner.NodeMetrics;
    public ObservableCollection<PodMetrics> PodMetrics => _inner.PodMetrics;
    public ObservableCollection<PortForwarder> PortForwarders => _inner.PortForwarders;
    public IClusterAuthorization Permissions => this;

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null) => _inner.CanI(type, verb, @namespace, subresource);
    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.CanI<T>(verb, @namespace, subresource);
    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null) => _inner.CanIAnyNamespace(type, verb, subresource);
    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.CanIAnyNamespace<T>(verb, subresource);
    public Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null) => _inner.UpdateCanI(type, verb, @namespace, subresource);
    public Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.UpdateCanI<T>(verb, @namespace, subresource);
    public Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null) => _inner.UpdatePermissionsAllNamespaceAsync(type, verb, subresource);
    public Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.UpdatePermissionsAllNamespaceAsync<T>(verb, subresource);
    public bool IsResourceNamespaced(Type type) => _inner.IsResourceNamespaced(type);
    public bool IsResourceNamespaced<T>() => _inner.IsResourceNamespaced<T>();
    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort) => _inner.AddPodPortForward(@namespace, podName, containerPort);
    public Task AddPodEphemeralDebugContainer(V1Pod pod, string? targetContainerName, string image) => _inner.AddPodEphemeralDebugContainer(pod, targetContainerName, image);
    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort) => _inner.AddServicePortForward(@namespace, serviceName, servicePort);
    public void RemovePortForward(PortForwarder pf) => _inner.RemovePortForward(pf);
    public Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.AddOrUpdateResource(item);
    public Task Connect() => _inner.Connect();
    public Task Disconnect() => _inner.Disconnect();
    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.DeleteResource(item);
    public Task DryRunYaml(Stream stream) => _inner.DryRunYaml(stream);
    public Task ImportFolder(string path) => _inner.ImportFolder(path);
    public Task ImportYaml(Stream stream) => _inner.ImportYaml(stream);
    public Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.IsResourceReady<T>(token);
    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResource<T>(@namespace, name);
    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceList<T>();
    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceSourceCache<T>();
    public IObservable<int> GetResourceCount(Type type) => _inner.GetResourceCount(type);
    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceCount<T>();

    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.SeedResource<T>(waitForReady);
    public Task SeedResource(Type resourceType, bool waitForReady = false) => _inner.SeedResource(resourceType, waitForReady);
}

internal sealed class BlockingPodPermissionResourceConfig : IResourceConfig
{
    private readonly TestCluster _runtime;
    private readonly Task _releaseTask;

    public BlockingPodPermissionResourceConfig(TestCluster runtime, Task releaseTask)
    {
        _runtime = runtime;
        _releaseTask = releaseTask;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; private set; }
    public bool PermissionsLoaded { get; private set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => [];
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
    public int Order => 0;
    public string Name => "Pods";
    public string? Category => "Workloads";
    public Style[] ListStyle() => [];
    public Type Type { get; } = typeof(V1Pod);
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [(Verb.List, null), (Verb.Watch, null), (Verb.Create, "portforward")];

    public async Task EvaluateListWatchAccessAsync()
    {
        await _releaseTask.ConfigureAwait(false);
        _runtime.SetPermission<V1Pod>(Verb.Create, true, subresource: "portforward");
        CanListAndWatch = true;
        PermissionsLoaded = true;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal sealed class ImmediatePermissionResourceConfig : IResourceConfig
{
    private readonly TaskCompletionSource<object?> _completion;

    public ImmediatePermissionResourceConfig(Type type, string name, TaskCompletionSource<object?> completion)
    {
        Type = type;
        Name = name;
        _completion = completion;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; private set; }
    public bool PermissionsLoaded { get; private set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => [];
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
    public int Order => 0;
    public string Name { get; }
    public string? Category => null;
    public Style[] ListStyle() => [];
    public Type Type { get; }
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [(Verb.List, null), (Verb.Watch, null)];

    public Task EvaluateListWatchAccessAsync()
    {
        CanListAndWatch = true;
        PermissionsLoaded = true;
        _completion.TrySetResult(null);
        return Task.CompletedTask;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal static class ClusterWorkspaceTestCustomResourceDefinitionFactory
{
    public static V1CustomResourceDefinition Create(string name, string plural, string schemaProperty)
    {
        return new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = name
            },
            Spec = new()
            {
                Group = "kubeui.com",
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = plural,
                    Singular = "test",
                    Kind = "Test",
                    ListKind = "TestList"
                },
                Versions =
                [
                    new()
                    {
                        Name = "v1beta1",
                        Served = true,
                        Storage = true,
                        Schema = new()
                        {
                            OpenAPIV3Schema = new()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, V1JSONSchemaProps>
                                {
                                    ["apiVersion"] = new() { Type = "string" },
                                    ["kind"] = new() { Type = "string" },
                                    ["metadata"] = new() { Type = "object" },
                                    ["spec"] = new()
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, V1JSONSchemaProps>
                                        {
                                            [schemaProperty] = new() { Type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ]
            }
        };
    }
}
