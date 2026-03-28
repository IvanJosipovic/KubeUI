using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public class ClusterWorkspaceViewModelTests : AvaloniaTestBase
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
    public async Task seeding_namespaced_resource_adds_configured_fallback_namespaces_without_eager_resource_config_initialization()
    {
        var runtime = new TestCluster
        {
            ListNamespaces = false,
        };
        var workspace = CreateWorkspace(runtime);

        Application.Current.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace)
            .Namespaces!
            .Add("team-a");

        workspace.GetResourceConfigs().ShouldBeEmpty();

        await workspace.SeedResource<V1Pod>();

        runtime.GetResource<V1Namespace>(null, "team-a").ShouldNotBeNull();
        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task seeding_namespaced_resource_creates_informers_for_each_known_namespace_with_list_and_watch_access()
    {
        var runtime = new TestCluster
        {
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };
        runtime.SetPermission<V1Pod>(Verb.List, true, "team-a");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "team-a");
        runtime.SetPermission<V1Pod>(Verb.List, true, "team-b");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "team-b");

        var workspace = CreateWorkspace(runtime);
        var clusterSettings = Application.Current.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace);
        clusterSettings.Namespaces!.Add("team-a");
        clusterSettings.Namespaces!.Add("team-b");

        await workspace.SeedResource<V1Pod>();

        runtime.GetResource<V1Namespace>(null, "team-a").ShouldNotBeNull();
        runtime.GetResource<V1Namespace>(null, "team-b").ShouldNotBeNull();

        var container = GetSeededContainer(runtime, typeof(V1Pod));
        container.ShouldNotBeNull();
        GetInformers(container).Count.ShouldBe(2);
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
        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task permission_refresh_completes_other_resource_configs_before_pod_permissions_finish()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            DefaultPermissionAllowed = false,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var podRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fastRefreshCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        workspace.AddResourceConfigForTest(new BlockingPodPermissionResourceConfig(runtime, podRelease.Task));
        workspace.AddResourceConfigForTest(new ImmediatePermissionResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource", fastRefreshCompleted));

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "my-app" }
        });

        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() => fastRefreshCompleted.Task.IsCompleted);
        workspace.GetResourceConfig<TestPermissionResourceAlpha>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<V1Pod>().PermissionsLoaded.ShouldBeFalse();

        podRelease.TrySetResult(null);

        await WaitForAsync(() => workspace.GetResourceConfig<V1Pod>().PermissionsLoaded);
        workspace.GetResourceConfig<V1Pod>().PermissionsLoaded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task connected_property_change_does_not_block_ui_thread_while_workspace_refresh_runs()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var workspace = CreateWorkspace(runtime);
        var releaseRefresh = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        workspace.AddResourceConfigForTest(new BlockingPodPermissionResourceConfig(runtime, releaseRefresh.Task));

        var sw = Stopwatch.StartNew();
        await Dispatcher.UIThread.InvokeAsync(() => runtime.Connected = true);
        sw.Stop();

        sw.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(250));

        var uiCallbackCompleted = false;
        await Dispatcher.UIThread.InvokeAsync(() => uiCallbackCompleted = true);
        uiCallbackCompleted.ShouldBeTrue();
        workspace.GetResourceConfig<V1Pod>().PermissionsLoaded.ShouldBeFalse();

        releaseRefresh.TrySetResult(null);

        await WaitForAsync(() => workspace.GetResourceConfig<V1Pod>().PermissionsLoaded);
        workspace.GetResourceConfig<V1Pod>().PermissionsLoaded.ShouldBeTrue();
    }

    private ClusterWorkspaceViewModel CreateWorkspace(TestCluster runtime)
    {
        var workspace = runtime.CreateWorkspace();
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

    private static IResourceConfig? GetCustomResourceConfig(ClusterWorkspaceViewModel workspace, V1CustomResourceDefinition crd)
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
    public IStyle ListStyle() => null;
    public Type Type { get; } = typeof(V1Pod);
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public async Task UpdatePermissions()
    {
        await _releaseTask.ConfigureAwait(false);
        _runtime.SetPermission<V1Pod>(Verb.Create, true, subresource: "portforward");
        CanListAndWatch = true;
        PermissionsLoaded = true;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
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
    public IStyle ListStyle() => null;
    public Type Type { get; }
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public Task UpdatePermissions()
    {
        CanListAndWatch = true;
        PermissionsLoaded = true;
        _completion.TrySetResult(null);
        return Task.CompletedTask;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
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
