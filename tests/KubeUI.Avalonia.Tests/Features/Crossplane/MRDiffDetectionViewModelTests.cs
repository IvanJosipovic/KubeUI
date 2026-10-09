using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionViewModelTests
{
    [AvaloniaFact]
    public async Task Provider_log_diffs_are_searchable_and_clear_restarts_tail_replay()
    {
        GroupApiVersionKind providerKind = new("pkg.crossplane.io", "v1", "Provider", "providers");
        GroupApiVersionKind widgetKind = new("example.com", "v1", "Widget", "widgets");
        using var providerCache = new SourceCache<GenericKubernetesObject, ResourceCacheKey>(ResourceCacheKey.From);
        var provider = new GenericKubernetesObject
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new V1ObjectMeta { Name = "provider-demo" }
        };
        var unnamedProvider = new GenericKubernetesObject
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new V1ObjectMeta { Name = " " }
        };
        providerCache.AddOrUpdate([provider, unnamedProvider]);

        var runtime = new Mock<IClusterRuntime>();
        runtime.SetupGet(cluster => cluster.Name).Returns("test-cluster");
        runtime.SetupGet(cluster => cluster.Status).Returns(ClusterStatus.None);
        var widgetResource = new GenericKubernetesObject
        {
            ApiVersion = "example.com/v1",
            Kind = "Widget",
            Metadata = new V1ObjectMeta
            {
                Uid = "uid-1",
                Name = "widget-one",
                NamespaceProperty = "default"
            }
        };
        var widgetContainer = new Mock<IResourceContainer>();
        widgetContainer.Setup(container => container.Snapshot()).Returns([widgetResource]);
        var providerPods = new List<V1Pod>();
        var providerPodContainer = new Mock<IResourceContainer>();
        providerPodContainer.Setup(container => container.Snapshot())
            .Returns(() => providerPods.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray());
        var providerPod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "provider-demo-abc",
                NamespaceProperty = "crossplane-system"
            },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "provider" }] },
            Status = new V1PodStatus
            {
                ContainerStatuses = [new V1ContainerStatus { Name = "provider", RestartCount = 0 }]
            }
        };
        providerPods.Add(providerPod);
        using var podChanges = new Subject<ResourceChange>();
        runtime.SetupGet(cluster => cluster.Objects).Returns(new Dictionary<GroupApiVersionKind, object>
        {
            [providerKind] = new object(),
            [widgetKind] = widgetContainer.Object,
            [GroupApiVersionKind.From<V1Pod>()] = providerPodContainer.Object
        });
        runtime.Setup(cluster => cluster.SeedResource(providerKind, true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        runtime.Setup(cluster => cluster.SeedResource(
            GroupApiVersionKind.From<V1Pod>(),
            true,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        runtime.Setup(cluster => cluster.GetResourceSourceCache<GenericKubernetesObject>(providerKind)).Returns(providerCache);
        runtime.Setup(cluster => cluster.ConnectResources()).Returns(podChanges);

        var yamlSerializer = new Mock<IKubernetesYamlSerializer>();
        yamlSerializer.Setup(serializer => serializer.Serialize(It.IsAny<object>())).Returns("yaml");
        var yamlValidationService = new Mock<IYamlValidationService>();
        yamlValidationService.Setup(validationService =>
            validationService.Validate(It.IsAny<string>(), It.IsAny<ClusterModelCatalog?>()))
            .Returns(Array.Empty<YamlDiagnostic>());
        using var yamlViewModel = new ResourceYamlViewModel(
            NullLogger<ResourceYamlViewModel>.Instance,
            yamlSerializer.Object,
            yamlValidationService.Object);
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(provider => provider.GetService(typeof(ResourceYamlViewModel)))
            .Returns(yamlViewModel);
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var mainViewModel = Application.Current.GetRequiredTestService<MainViewModel>();
        var previousLayout = mainViewModel.Layout;
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        mainViewModel.Layout = layout;
        using var instrumentation = new Instrumentation();
        using var cluster = new ClusterWorkspace(
            runtime.Object,
            serviceProvider.Object,
            NullLogger<ClusterWorkspace>.Instance,
            instrumentation);
        var providerConfig = CreateResourceConfig(providerKind, "Providers", isCustomResource: true);
        var widgetConfig = CreateResourceConfig(widgetKind, "Widgets", isCustomResource: true);
        cluster.AddResourceConfigForTest(providerConfig.Object);
        cluster.AddResourceConfigForTest(widgetConfig.Object);
        var seedStarted = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pendingSeed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        widgetConfig.Setup(resourceConfig => resourceConfig.SeedResource(
            true,
            It.IsAny<CancellationToken>())).Returns((bool _, CancellationToken cancellationToken) =>
        {
            seedStarted.TrySetResult(cancellationToken);
            cancellationToken.Register(() => pendingSeed.TrySetCanceled(cancellationToken));
            return pendingSeed.Task;
        });

        var streamClient = new StaticPodLogStreamClient(string.Join(
            '\n',
            CreateDiffLog("uid-1", "widget-one", "old", "new"),
            CreateDiffLog("uid-2", "widget-two", "old-two", "new-two"),
            CreateDiffLog("uid-1", "widget-one", "new", "updated"),
            CreateDiffLog("uid-4", "core-thing", "old-four", "new-four", gvk: "v1, Kind=CoreThing"),
            CreateDiffLog("uid-5", "unknown-widget", "old-five", "new-five", gvk: "missing.example.com/v1, Kind=Unknown"),
            CreateTruncatedDiffLog("uid-3", "widget-three", "old-three", "new-three")));
        var resolver = new FixedPodResolver(new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "provider-demo-abc", NamespaceProperty = "crossplane-system" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "provider" }] },
            Status = new V1PodStatus()
        });
        using var monitor = new CrossplaneProviderLogMonitor(
            resolver,
            streamClient,
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var viewModel = new MRDiffDetectionViewModel(
            new CrossplaneDiffLogParser(),
            monitor,
            NullLogger<MRDiffDetectionViewModel>.Instance,
            serviceProvider.Object,
            factory);

        viewModel.Initialize(cluster);
        viewModel.Initialize(cluster);

        await WaitForAsync(() => viewModel.Providers.Count == 1);
        viewModel.SelectedProvider = viewModel.Providers.Single();
        await WaitForAsync(() => GetRows(viewModel).Length == 5);
        await WaitForAsync(() => viewModel.Status == Assets.Resources.MRDiffDetectionView_TruncatedLog);
        viewModel.SelectionModel.ShouldBeSameAs(viewModel.TableSource.SelectionModel);
        MRDiffDetectionViewModel.IsAvailable(cluster).ShouldBeTrue();
        var rows = GetRows(viewModel).ToDictionary(row => row.Uid, StringComparer.Ordinal);
        var updatedRow = rows["uid-1"];
        updatedRow.Name.ShouldBe("widget-one");
        updatedRow.DiffField.ShouldBe("spec.value");
        updatedRow.OldValue.ShouldBe("new");
        updatedRow.NewValue.ShouldBe("updated");
        updatedRow.Occurrences.ShouldBe(2);
        rows.Values.Where(row => row.ApiVersion == "example.com/v1")
            .ShouldAllBe(row => row.InstanceCount == 3);
        rows["uid-4"].InstanceCount.ShouldBe(1);
        rows["uid-5"].InstanceCount.ShouldBe(1);
        viewModel.ViewYamlCommand.CanExecute(updatedRow).ShouldBeTrue();
        viewModel.ViewYamlCommand.CanExecute(null).ShouldBeFalse();
        viewModel.ViewYamlCommand.Execute(null);
        foreach (var unmatchedRow in new[]
        {
            CreateRow("missing-uid"),
            CreateRow("uid-1", name: "missing-name"),
            CreateRow("uid-1", ns: "missing-namespace"),
            CreateRow("uid-1", apiVersion: "example.com/v2"),
            CreateRow("uid-1", kind: "OtherWidget")
        })
        {
            viewModel.ViewYamlCommand.CanExecute(unmatchedRow).ShouldBeFalse();
            viewModel.ViewYamlCommand.Execute(unmatchedRow);
        }

        try
        {
            viewModel.ViewYamlCommand.Execute(updatedRow);
            yamlViewModel.Cluster.ShouldBeSameAs(cluster);
            yamlViewModel.Object.ShouldBeSameAs(widgetResource);
            factory.GetDockable<IToolDock>("BottomDock")!.VisibleDockables
                .ShouldContain(yamlViewModel);
        }
        finally
        {
            factory.RemoveDockable(yamlViewModel, collapse: false);
            if (previousLayout is not null)
            {
                factory.InitLayout(previousLayout);
            }

            mainViewModel.Layout = previousLayout;
        }

        providerConfig.SetupGet(config => config.IsCustomResource).Returns(false);
        MRDiffDetectionViewModel.IsAvailable(cluster).ShouldBeFalse();
        providerConfig.SetupGet(config => config.IsCustomResource).Returns(true);
        providerConfig.SetupGet(config => config.PermissionsLoaded).Returns(false);
        MRDiffDetectionViewModel.IsAvailable(cluster).ShouldBeFalse();
        providerConfig.SetupGet(config => config.PermissionsLoaded).Returns(true);
        providerConfig.SetupGet(config => config.CanListAndWatch).Returns(false);
        MRDiffDetectionViewModel.IsAvailable(cluster).ShouldBeFalse();
        providerConfig.SetupGet(config => config.CanListAndWatch).Returns(true);
        MRDiffDetectionViewModel.IsAvailable(cluster).ShouldBeTrue();

        viewModel.SearchQuery = "not found";
        await WaitForAsync(() => GetRows(viewModel).Length == 0);
        viewModel.SearchQuery = "widget-";
        await WaitForAsync(() => GetRows(viewModel).Length == 3);
        providerCache.AddOrUpdate(new GenericKubernetesObject
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new V1ObjectMeta { Name = "provider-another" }
        });
        await WaitForAsync(() => viewModel.Providers.Count == 2);
        viewModel.SelectedProvider?.Name.ShouldBe("provider-demo");

        viewModel.SearchQuery = string.Empty;
        await WaitForAsync(() => GetRows(viewModel).Length == 5);
        streamClient.ResetOpenCount();
        podChanges.OnNext(new ResourceChange(
            WatchEventType.Modified,
            GroupApiVersionKind.From<V1Pod>(),
            providerPod));
        await WaitForProviderReconciliationAsync();
        streamClient.OpenCount.ShouldBe(0);

        providerPod.Status.ContainerStatuses[0].RestartCount = 1;
        podChanges.OnNext(new ResourceChange(
            WatchEventType.Modified,
            GroupApiVersionKind.From<V1Pod>(),
            providerPod));
        await WaitForAsync(() => streamClient.OpenCount == 1, timeoutMilliseconds: 750);
        GetRows(viewModel).Length.ShouldBe(5);

        streamClient.ResetOpenCount();
        providerPod.Status.ContainerStatuses = [];
        podChanges.OnNext(new ResourceChange(
            WatchEventType.Modified,
            GroupApiVersionKind.From<V1Pod>(),
            providerPod));
        await WaitForAsync(() => streamClient.OpenCount == 1, timeoutMilliseconds: 750);
        GetRows(viewModel).Length.ShouldBe(5);

        streamClient.ResetOpenCount();
        podChanges.OnNext(new ResourceChange(
            WatchEventType.Modified,
            GroupApiVersionKind.From<V1Pod>(),
            providerPod));
        await WaitForProviderReconciliationAsync();
        streamClient.OpenCount.ShouldBe(0);

        viewModel.ClearCommand.Execute(null);
        await WaitForAsync(() => streamClient.OpenCount >= 2 && GetRows(viewModel).Length == 0);
        providerCache.Remove(provider);
        await WaitForAsync(() => viewModel.Status == Assets.Resources.MRDiffDetectionView_SelectProvider);
        viewModel.SelectedProvider.ShouldBeNull();
        var seedingToken = await seedStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        viewModel.Dispose();

        seedingToken.IsCancellationRequested.ShouldBeTrue();
        await Should.ThrowAsync<OperationCanceledException>(() => pendingSeed.Task);
    }

    [AvaloniaFact]
    public async Task Initialize_without_provider_configuration_clears_selection_and_prompts_user()
    {
        var runtime = new Mock<IClusterRuntime>();
        runtime.SetupGet(cluster => cluster.Name).Returns("test-cluster");
        runtime.SetupGet(cluster => cluster.Status).Returns(ClusterStatus.None);
        runtime.SetupGet(cluster => cluster.Objects).Returns(new Dictionary<GroupApiVersionKind, object>());
        runtime.Setup(cluster => cluster.ConnectResources()).Returns(Observable.Empty<ResourceChange>());

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        using var instrumentation = new Instrumentation();
        using var cluster = new ClusterWorkspace(
            runtime.Object,
            serviceProvider,
            NullLogger<ClusterWorkspace>.Instance,
            instrumentation);
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "provider-demo-abc", NamespaceProperty = "crossplane-system" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "provider" }] },
            Status = new V1PodStatus()
        };
        using var monitor = new CrossplaneProviderLogMonitor(
            new FixedPodResolver(pod),
            new StaticPodLogStreamClient(string.Empty),
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var viewModel = new MRDiffDetectionViewModel(
            new CrossplaneDiffLogParser(),
            monitor,
            NullLogger<MRDiffDetectionViewModel>.Instance,
            serviceProvider,
            Mock.Of<IFactory>());
        viewModel.Providers.Add(new CrossplaneProviderOption(
            "stale-provider",
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "stale-provider" } }));
        viewModel.SelectedProvider = viewModel.Providers[0];

        viewModel.Initialize(cluster);

        await WaitForAsync(() => viewModel.Status == Assets.Resources.MRDiffDetectionView_SelectProvider);
        viewModel.Providers.ShouldBeEmpty();
        viewModel.SelectedProvider.ShouldBeNull();
        MRDiffDetectionViewModel.IsAvailable(cluster).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Initialize_surfaces_provider_resource_seed_failures()
    {
        GroupApiVersionKind providerKind = new("pkg.crossplane.io", "v1", "Provider", "providers");
        var runtime = new Mock<IClusterRuntime>();
        runtime.SetupGet(cluster => cluster.Name).Returns("test-cluster");
        runtime.SetupGet(cluster => cluster.Status).Returns(ClusterStatus.None);
        runtime.SetupGet(cluster => cluster.Objects).Returns(new Dictionary<GroupApiVersionKind, object>());
        runtime.Setup(cluster => cluster.SeedResource(providerKind, true, It.IsAny<CancellationToken>()))
            .Returns(Task.FromException(new InvalidOperationException("provider seed failed")));
        runtime.Setup(cluster => cluster.SeedResource(
            GroupApiVersionKind.From<V1Pod>(),
            true,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        runtime.Setup(cluster => cluster.ConnectResources()).Returns(Observable.Empty<ResourceChange>());

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        using var instrumentation = new Instrumentation();
        using var cluster = new ClusterWorkspace(
            runtime.Object,
            serviceProvider,
            NullLogger<ClusterWorkspace>.Instance,
            instrumentation);
        cluster.AddResourceConfigForTest(CreateResourceConfig(providerKind, "Providers", isCustomResource: true).Object);
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "provider-demo-abc", NamespaceProperty = "crossplane-system" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "provider" }] },
            Status = new V1PodStatus()
        };
        using var monitor = new CrossplaneProviderLogMonitor(
            new FixedPodResolver(pod),
            new StaticPodLogStreamClient(string.Empty),
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var viewModel = new MRDiffDetectionViewModel(
            new CrossplaneDiffLogParser(),
            monitor,
            NullLogger<MRDiffDetectionViewModel>.Instance,
            serviceProvider,
            Mock.Of<IFactory>());

        viewModel.Initialize(cluster);

        await WaitForAsync(() => viewModel.Status == "provider seed failed");
    }

    [AvaloniaFact]
    public async Task Provider_monitor_failures_are_reported_in_status()
    {
        GroupApiVersionKind providerKind = new("pkg.crossplane.io", "v1", "Provider", "providers");
        using var providerCache = new SourceCache<GenericKubernetesObject, ResourceCacheKey>(ResourceCacheKey.From);
        providerCache.AddOrUpdate(new GenericKubernetesObject
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new V1ObjectMeta { Name = "provider-demo" }
        });

        var runtime = new Mock<IClusterRuntime>();
        runtime.SetupGet(cluster => cluster.Name).Returns("test-cluster");
        runtime.SetupGet(cluster => cluster.Status).Returns(ClusterStatus.None);
        runtime.SetupGet(cluster => cluster.Objects).Returns(new Dictionary<GroupApiVersionKind, object>());
        runtime.Setup(cluster => cluster.SeedResource(It.IsAny<GroupApiVersionKind>(), true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        runtime.Setup(cluster => cluster.GetResourceSourceCache<GenericKubernetesObject>(providerKind))
            .Returns(providerCache);
        runtime.Setup(cluster => cluster.ConnectResources()).Returns(Observable.Empty<ResourceChange>());

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        using var instrumentation = new Instrumentation();
        using var cluster = new ClusterWorkspace(
            runtime.Object,
            serviceProvider,
            NullLogger<ClusterWorkspace>.Instance,
            instrumentation);
        cluster.AddResourceConfigForTest(CreateResourceConfig(providerKind, "Providers", isCustomResource: true).Object);
        using var monitor = new CrossplaneProviderLogMonitor(
            new ThrowingPodResolver(),
            new StaticPodLogStreamClient(string.Empty),
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var viewModel = new MRDiffDetectionViewModel(
            new CrossplaneDiffLogParser(),
            monitor,
            NullLogger<MRDiffDetectionViewModel>.Instance,
            serviceProvider,
            Mock.Of<IFactory>());

        viewModel.Initialize(cluster);
        await WaitForAsync(() => viewModel.Providers.Count == 1);
        viewModel.SelectedProvider = viewModel.Providers.Single();

        await WaitForAsync(() => viewModel.Status == "provider pod resolution failed");
    }

    [AvaloniaFact]
    public void View_yaml_is_a_no_op_before_cluster_initialization()
    {
        using var monitor = new CrossplaneProviderLogMonitor(
            new FixedPodResolver(new V1Pod()),
            new StaticPodLogStreamClient(string.Empty),
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var viewModel = new MRDiffDetectionViewModel(
            new CrossplaneDiffLogParser(),
            monitor,
            NullLogger<MRDiffDetectionViewModel>.Instance,
            Mock.Of<IServiceProvider>(),
            Mock.Of<IFactory>());
        var row = CreateRow("uid-1");

        viewModel.ViewYamlCommand.CanExecute(row).ShouldBeFalse();
        viewModel.ViewYamlCommand.Execute(row);
        viewModel.ViewYamlCommand.Execute(null);
    }

    private static CrossplaneDiffRow CreateRow(
        string uid,
        string name = "widget-one",
        string ns = "default",
        string apiVersion = "example.com/v1",
        string kind = "Widget")
    {
        return new CrossplaneDiffRow(new CrossplaneDiffRecord(
            uid,
            name,
            ns,
            apiVersion,
            kind,
            "spec.value",
            string.Empty,
            string.Empty,
            NewComputed: false,
            NewRemoved: false,
            RequiresNew: false));
    }

    private static string CreateDiffLog(
        string uid,
        string name,
        string oldValue,
        string newValue,
        string gvk = "example.com/v1, Kind=Widget")
    {
        var instanceDiff = "*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\"spec.value\":*terraform.ResourceAttrDiff{Old:\""
            + oldValue
            + "\", New:\""
            + newValue
            + "\"}}}";
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            uid,
            name,
            @namespace = "default",
            gvk,
            instanceDiff
        });
        return $"DEBUG provider Diff detected {payload}";
    }

    private static string CreateTruncatedDiffLog(string uid, string name, string oldValue, string newValue)
    {
        var instanceDiff = "*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\"spec.value\":*terraform.ResourceAttrDiff{Old:\""
            + oldValue
            + "\", New:\""
            + newValue
            + "\"},\"largeField\":*terraform.ResourceAttrDiff{Old:\""
            + new string('x', 20_000)
            + "\", New:\"unused\"}}}";
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            uid,
            name,
            @namespace = "default",
            gvk = "example.com/v1, Kind=Widget",
            instanceDiff
        });
        return $"DEBUG provider Diff detected {payload}"[..16_384];
    }

    private static Mock<IResourceConfig> CreateResourceConfig(
        GroupApiVersionKind kind,
        string name,
        bool isCustomResource)
    {
        var config = new Mock<IResourceConfig>();
        config.SetupGet(resourceConfig => resourceConfig.Kind).Returns(kind);
        config.SetupGet(resourceConfig => resourceConfig.Name).Returns(name);
        config.SetupGet(resourceConfig => resourceConfig.IsCustomResource).Returns(isCustomResource);
        config.SetupGet(resourceConfig => resourceConfig.PermissionsLoaded).Returns(true);
        config.SetupGet(resourceConfig => resourceConfig.CanListAndWatch).Returns(true);
        config.Setup(resourceConfig => resourceConfig.SeedResource(It.IsAny<bool>())).Returns(Task.CompletedTask);
        config.Setup(resourceConfig => resourceConfig.SeedResource(
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return config;
    }

    private static CrossplaneDiffRow[] GetRows(MRDiffDetectionViewModel viewModel)
        => viewModel.TableSource.Items.Cast<CrossplaneDiffRow>().ToArray();

    private static Task WaitForAsync(Func<bool> condition, int timeoutMilliseconds = 5000)
        => TestWait.UntilAsync(
            condition,
            timeoutMilliseconds,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

    private static async Task WaitForProviderReconciliationAsync()
    {
        await Observable.Timer(TimeSpan.FromMilliseconds(100)).FirstAsync();
        await Dispatcher.UIThread.InvokeAsync(() => Dispatcher.UIThread.RunJobs());
    }

    private sealed class FixedPodResolver(V1Pod pod) : IPodLogSessionResolver
    {
        public PodLogSessionState CreateState(
            IKubernetesObject<V1ObjectMeta> resource,
            string containerName,
            bool previous,
            bool timestamps,
            int tailLines = 500)
            => new("crossplane-system", "provider-demo", null, "Provider", null, null, null, containerName, previous, timestamps, tailLines);

        public PodLogMultiSessionState CreateMultiState(
            IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
            string containerName,
            bool previous,
            bool timestamps,
            int tailLines = 500)
            => throw new NotSupportedException();

        public PodLogSessionResolution? TryResolve(IClusterRuntime cluster, PodLogSessionState state)
            => new(pod, state.ContainerName, [pod], false, true, null);

        public PodLogMultiSessionResolution TryResolve(IClusterRuntime cluster, PodLogMultiSessionState state)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingPodResolver : IPodLogSessionResolver
    {
        public PodLogSessionState CreateState(
            IKubernetesObject<V1ObjectMeta> resource,
            string containerName,
            bool previous,
            bool timestamps,
            int tailLines = 500)
            => new("crossplane-system", "provider-demo-abc", null, "Provider", null, null, null, containerName, previous, timestamps, tailLines);

        public PodLogMultiSessionState CreateMultiState(
            IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
            string containerName,
            bool previous,
            bool timestamps,
            int tailLines = 500)
            => throw new NotSupportedException();

        public PodLogSessionResolution? TryResolve(IClusterRuntime cluster, PodLogSessionState state)
            => throw new InvalidOperationException("provider pod resolution failed");

        public PodLogMultiSessionResolution TryResolve(IClusterRuntime cluster, PodLogMultiSessionState state)
            => throw new NotSupportedException();
    }

    private sealed class StaticPodLogStreamClient(string line) : IPodLogStreamClient
    {
        private int _openCount;
        private int _contentServed;

        public int OpenCount => Volatile.Read(ref _openCount);

        public void ResetOpenCount() => Interlocked.Exchange(ref _openCount, 0);

        public Task<Stream> OpenAsync(
            IClusterRuntime cluster,
            PodLogReadOptions options,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _openCount);
            var content = Interlocked.Exchange(ref _contentServed, 1) == 0 ? line + "\n" : string.Empty;
            return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }
    }
}
