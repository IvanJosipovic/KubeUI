using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using Humanizer;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodMetricCellTests : AvaloniaTestBase
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
    public async Task cpu_cell_updates_rendered_text_after_async_prometheus_response()
    {
        var value = 1.234;
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}" },
            async request =>
            {
                await Task.Delay(25).ConfigureAwait(false);
                request.Queries.Single().Name.ShouldBe("cpuUsage");
                return CreateMetricResult("cpuUsage", value);
            });
        var workspace = CreateWorkspace(runtime);
        _disposables.Add(workspace);
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"pod-{Guid.NewGuid():N}",
                NamespaceProperty = "default",
            }
        };
        var window = new Window();
        try
        {
            var cell = new PodMetricCPUCell { DataContext = pod };
            window.Content = cell;
            window.Show();

            cell.Initialize(workspace);

            await WaitForAsync(() => GetRenderedText(cell) == $"{value:F3}c");

            GetRenderedText(cell).ShouldBe($"{value:F3}c");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task memory_cell_updates_rendered_text_after_async_prometheus_response()
    {
        var value = 1_048_576d;
        var expected = ((long)value).Bytes().Humanize();
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}" },
            async request =>
            {
                await Task.Delay(25).ConfigureAwait(false);
                request.Queries.Single().Name.ShouldBe("memoryUsage");
                return CreateMetricResult("memoryUsage", value);
            });
        var workspace = CreateWorkspace(runtime);
        _disposables.Add(workspace);
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"pod-{Guid.NewGuid():N}",
                NamespaceProperty = "default",
            }
        };
        var window = new Window();
        try
        {
            var cell = new PodMetricMemoryCell { DataContext = pod };
            window.Content = cell;
            window.Show();

            cell.Initialize(workspace);

            await WaitForAsync(() => GetRenderedText(cell) == expected);

            GetRenderedText(cell).ShouldBe(expected);
        }
        finally
        {
            window.Close();
        }
    }

    private static ClusterWorkspaceViewModel CreateWorkspace(IClusterRuntime runtime)
    {
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspaceViewModel>(
            Application.Current?.GetRequiredService<IServiceProvider>() ?? throw new InvalidOperationException("Avalonia Application.Current is not initialized."),
            runtime);
        return workspace;
    }

    private static MetricResultSet CreateMetricResult(string queryName, double value)
    {
        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                [queryName] =
                [
                    new MetricSeries
                    {
                        Name = queryName,
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal),
                        Points =
                        [
                            new MetricPoint(DateTimeOffset.UtcNow, value),
                        ],
                    }
                ],
            },
        };
    }

    private static string? GetRenderedText(Control cell)
    {
        Dispatcher.UIThread.RunJobs();
        return cell.GetVisualDescendants().OfType<TextBlock>().Single().Text;
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 5000)
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
}

internal sealed class PrometheusMetricsClusterRuntime : IClusterRuntime, INotifyPropertyChanged
{
    private readonly TestClusterRuntime _inner;
    private readonly Func<MetricRequest, Task<MetricResultSet>> _requestMetricsAsync;

    public PrometheusMetricsClusterRuntime(TestClusterRuntime inner, Func<MetricRequest, Task<MetricResultSet>> requestMetricsAsync)
    {
        _inner = inner;
        _requestMetricsAsync = requestMetricsAsync;
        _inner.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
    }

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

    public IReadOnlyDictionary<GroupApiVersionKind, object> Objects => _inner.Objects;
    public bool Connected { get => _inner.Connected; set => _inner.Connected = value; }
    public ClusterStatus Status { get => _inner.Status; set => _inner.Status = value; }
    public string? LastError { get => _inner.LastError; set => _inner.LastError = value; }
    public bool RequiresNamespaceSelectionPrompt { get => _inner.RequiresNamespaceSelectionPrompt; set => _inner.RequiresNamespaceSelectionPrompt = value; }
    public bool IsMetricsAvailable => true;
    public ActiveMetricsBackend ActiveMetricsBackend => ActiveMetricsBackend.Prometheus(PrometheusProviderKind.External);
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

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null) => _inner.CanI(type, verb, @namespace, subresource);
    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.CanI<T>(verb, @namespace, subresource);
    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null) => _inner.CanIAnyNamespace(type, verb, subresource);
    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.CanIAnyNamespace<T>(verb, subresource);
    public bool IsResourceNamespaced(Type type) => _inner.IsResourceNamespaced(type);
    public bool IsResourceNamespaced<T>() => _inner.IsResourceNamespaced<T>();
    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort) => _inner.AddPodPortForward(@namespace, podName, containerPort);
    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort) => _inner.AddServicePortForward(@namespace, serviceName, servicePort);
    public void RemovePortForward(PortForwarder pf) => _inner.RemovePortForward(pf);
    public Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.AddOrUpdateResource(item);
    public Task Connect() => _inner.Connect();
    public Task Disconnect() => _inner.Disconnect();
    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.DeleteResource(item);
    public Task DryRunYaml(Stream stream) => _inner.DryRunYaml(stream);
    public Task ImportFolder(string path) => _inner.ImportFolder(path);
    public Task ImportYaml(Stream stream) => _inner.ImportYaml(stream);
    public Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default) => _requestMetricsAsync(request);
    public Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync() => _inner.GetAvailablePrometheusProvidersAsync();
    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.SeedResource<T>(waitForReady);
    public Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.IsResourceReady<T>(token);
    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResource<T>(@namespace, name);
    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceList<T>();
    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceSourceCache<T>();
    public IObservable<int> GetResourceCount(Type type) => _inner.GetResourceCount(type);
    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.GetResourceCount<T>();
    public Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null) => _inner.UpdatePermissionsAllNamespaceAsync(type, verb, subresource);
    public Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.UpdatePermissionsAllNamespaceAsync<T>(verb, subresource);
    public Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null) => _inner.UpdateCanI(type, verb, @namespace, subresource);
    public Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.UpdateCanI<T>(verb, @namespace, subresource);
    public Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null) => _inner.UpdateCanIAnyNamespaceAsync(type, verb, subresource);
    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new() => _inner.UpdateCanIAnyNamespaceAsync<T>(verb, subresource);
}
