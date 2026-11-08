using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using BenchmarkDotNet.Attributes;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Yaml;
using KubeUI.Kubernetes;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Kubernetes.Scenarios;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Benchmarks;

/// <summary>
/// Measures fake-watch resource updates through the production YAML view and editor layout.
/// Frame capture is kept in a separate benchmark so it does not distort refresh allocations.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(YamlViewerResourceRefreshBenchmarkConfig))]
[BenchmarkCategory("YAML", "ResourceRefresh", "Headless")]
public class YamlViewerResourceRefreshBenchmarks : IDisposable
{
    private const string RevisionLabel = "benchmark.kubeui.com/revision";
    private const int RefreshCount = 100;

    private YamlBenchmarkUiThread _uiThread = null!;
    private TestCluster _testCluster = null!;
    private string _podName = null!;
    private bool _useConfigMap;
    private int _containerCount;
    private ClusterWorkspace _workspace = null!;
    private ResourceYamlViewModel _viewModel = null!;
    private ResourceYamlView _view = null!;
    private Window _window = null!;
    private TaskCompletionSource<int>? _pendingObjectUpdate;
    private int _expectedRevision;
    private IKubernetesObject<V1ObjectMeta> _currentResource = null!;
    private int _additionalAnnotationCount;
    private int _additionalConfigMapEntryCount;
    private bool _isCalibrating;
    private int _lastFoldingCount;
    private int _lastVisualLineCount;
    private int _lastYamlLineCount;
    private int _cleanupStarted;

    [Params(10, 100, 1000)]
    public int YamlLineCount { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _isCalibrating = true;
        StartHeadlessUiThread();
        var services = InvokeOnUi(() => _uiThread.App.Services);

        _useConfigMap = YamlLineCount == 10;
        _containerCount = Math.Max(1, ((YamlLineCount - 80) / 18) + 1);
        _currentResource = _useConfigMap
            ? CreateConfigMap(0, 0)
            : CreatePod(0, 0, _containerCount);
        _podName = $"yaml-refresh-benchmark-{YamlLineCount.ToString(CultureInfo.InvariantCulture)}";
        _currentResource.Metadata!.Name = _podName;
        var generator = services.GetRequiredService<TestClusterGenerator>();
        _testCluster = await generator.CreateAsync(
            new TestClusterConfig
            {
                Type = KubernetesBackend.Fake,
                Name = _podName,
                InitialResources = [_currentResource],
            },
            CancellationToken.None).ConfigureAwait(false);

        _workspace = InvokeOnUi(() =>
        {
            services.GetRequiredService<ClusterManager>().AddCluster(_testCluster.Cluster);
            return services.GetRequiredService<ClusterWorkspaceCatalog>()
                .GetCluster(_testCluster.Cluster.Name)
                ?? throw new InvalidOperationException("Fake cluster workspace was not registered.");
        });

        await _workspace.Connect().ConfigureAwait(false);
        if (_useConfigMap)
        {
            await _workspace.Runtime.SeedResource<V1ConfigMap>(true, CancellationToken.None).ConfigureAwait(false);
        }
        else
        {
            await _workspace.Runtime.SeedResource<V1Pod>(true, CancellationToken.None).ConfigureAwait(false);
        }

        await TestWait.UntilAsync(
            () => GetCurrentResource() is not null,
            TimeSpan.FromSeconds(15),
            cancellationToken: CancellationToken.None).ConfigureAwait(false);
        _currentResource = GetCurrentResource()
            ?? throw new InvalidOperationException("Seeded benchmark resource was not observed by the fake cluster.");

        InvokeOnUi(() =>
        {
            _viewModel = services.GetRequiredService<ResourceYamlViewModel>();
            _viewModel.Initialize(_workspace, _currentResource);
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            _view = new ResourceYamlView { DataContext = _viewModel };
            _window = new Window
            {
                Content = _view,
                Width = 1280,
                Height = 900,
            };
            _window.Show();
            RenderCurrentFrame();
            CollapseAllFoldings();
            RenderCurrentFrame();
            return 0;
        });

        await CalibrateYamlLineCountAsync().ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<int> UpdatePodAndLayoutYamlView()
    {
        for (var revision = 1; revision <= RefreshCount; revision++)
        {
            await UpdatePodAndLayoutAsync(revision).ConfigureAwait(false);
        }

        return _lastFoldingCount;
    }

    [IterationCleanup]
    public void ReportRefreshedYamlShape()
    {
        Console.WriteLine(
            $"YAML viewer after {RefreshCount} updates: foldings={_lastFoldingCount}, " +
            $"visualLines={_lastVisualLineCount}, yamlLines={_lastYamlLineCount}");
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        if (Interlocked.Exchange(ref _cleanupStarted, 1) != 0)
        {
            return;
        }

        if (_testCluster is not null)
        {
            await _testCluster.DisposeAsync().ConfigureAwait(false);
        }

        if (_uiThread is not null)
        {
            InvokeOnUi(() =>
            {
                _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                _window.Close();
                _viewModel.Dispose();
                return 0;
            });
        }

    }

    public void Dispose()
    {
        Cleanup().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    private void StartHeadlessUiThread()
    {
        _uiThread = YamlBenchmarkUiThread.Instance;
    }

    private async Task CalibrateYamlLineCountAsync()
    {
        try
        {
            var revision = RefreshCount + 1;
            for (var attempt = 0; attempt < 4; attempt++)
            {
                await UpdatePodAndLayoutAsync(revision++).ConfigureAwait(false);
                if (_lastYamlLineCount == YamlLineCount)
                {
                    return;
                }

                if (_lastYamlLineCount > YamlLineCount)
                {
                    throw new InvalidOperationException(
                        $"Refreshed YAML exceeded target {YamlLineCount} lines: {_lastYamlLineCount}.");
                }

                var missingLines = YamlLineCount - _lastYamlLineCount;
                if (_useConfigMap)
                {
                    _additionalConfigMapEntryCount += missingLines;
                }
                else
                {
                    _additionalAnnotationCount += missingLines;
                }
            }

            throw new InvalidOperationException(
                $"Could not size refreshed YAML to exactly {YamlLineCount} lines; got {_lastYamlLineCount}.");
        }
        finally
        {
            _isCalibrating = false;
        }
    }

    private async Task UpdatePodAndLayoutAsync(int revision)
    {
        var update = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Volatile.Write(ref _expectedRevision, revision);
        Interlocked.Exchange(ref _pendingObjectUpdate, update);

        var updatedResource = _useConfigMap
            ? (IKubernetesObject<V1ObjectMeta>)CreateConfigMap(revision, _additionalConfigMapEntryCount)
            : CreatePod(revision, _additionalAnnotationCount, _containerCount);
        updatedResource.Metadata!.Name = _podName;
        updatedResource.Metadata.Uid = _currentResource.Metadata!.Uid;
        updatedResource.Metadata.ResourceVersion = _currentResource.Metadata.ResourceVersion;

        // Exercise production API write and informer/watch path, not direct editor mutation.
        if (updatedResource is V1ConfigMap configMap)
        {
            await _workspace.Runtime.AddOrUpdateResource(configMap).ConfigureAwait(false);
        }
        else if (updatedResource is V1Pod pod)
        {
            await _workspace.Runtime.AddOrUpdateResource(pod).ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException("Unsupported benchmark resource type.");
        }

        _currentResource = updatedResource;
        await update.Task.WaitAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
        await InvokeOnUiAsync(() =>
        {
            LayoutCurrentYamlView();
            return 0;
        }).ConfigureAwait(false);
    }

    private T InvokeOnUi<T>(Func<T> action)
    {
        return _uiThread.Invoke(action);
    }

    private Task<T> InvokeOnUiAsync<T>(Func<T> action)
    {
        return _uiThread.InvokeAsync(action);
    }

    private void RenderCurrentFrame()
    {
        LayoutCurrentYamlView();
        using var frame = _window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("Headless YAML view rendered no frame.");
    }

    private void LayoutCurrentYamlView()
    {
        _window.Measure(new Size(_window.Width, _window.Height));
        _window.Arrange(new Rect(0, 0, _window.Width, _window.Height));
        Dispatcher.UIThread.RunJobs();

        var editor = _view.FindControl<TextEditor>("Editor")
            ?? throw new InvalidOperationException("Resource YAML editor was not created.");
        _lastYamlLineCount = editor.Document!.LineCount;
        if (!_isCalibrating && _lastYamlLineCount != YamlLineCount)
        {
            throw new InvalidOperationException(
                $"Expected exactly {YamlLineCount} YAML lines; got {_lastYamlLineCount}.");
        }

        var expectedYamlValue = _expectedRevision.ToString(CultureInfo.InvariantCulture);
        if (!editor.Text.Contains(expectedYamlValue, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Rendered editor has not received resource revision {_expectedRevision}.");
        }

        var margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        _lastFoldingCount = margin.FoldingManager.AllFoldings.Count();
        _lastVisualLineCount = editor.TextArea.TextView.VisualLines.Count;
        if (_lastFoldingCount == 0 || _lastVisualLineCount == 0)
        {
            throw new InvalidOperationException(
                $"Expected rendered YAML foldings and lines; got {_lastFoldingCount} foldings and {_lastVisualLineCount} visual lines.");
        }
    }

    private void CollapseAllFoldings()
    {
        var editor = _view.FindControl<TextEditor>("Editor")
            ?? throw new InvalidOperationException("Resource YAML editor was not created.");
        var margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        foreach (var folding in margin.FoldingManager.AllFoldings)
        {
            folding.IsFolded = true;
        }

        Dispatcher.UIThread.RunJobs();
    }

    private void ViewModelOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName != nameof(ResourceYamlViewModel.Object)
            || !TryGetRevision(_viewModel.Object, out var revision)
            || revision != Volatile.Read(ref _expectedRevision))
        {
            return;
        }

        Interlocked.Exchange(ref _pendingObjectUpdate, null)?.TrySetResult(revision);
    }

    private IKubernetesObject<V1ObjectMeta>? GetCurrentResource()
    {
        return _useConfigMap
            ? _workspace.Runtime.GetResource<V1ConfigMap>("default", _podName)
            : _workspace.Runtime.GetResource<V1Pod>("default", _podName);
    }

    private static bool TryGetRevision(IKubernetesObject<V1ObjectMeta>? resource, out int revision)
    {
        var revisionText = resource switch
        {
            V1ConfigMap configMap when configMap.Data?.TryGetValue("benchmark-revision", out var configMapRevision) == true => configMapRevision,
            V1Pod pod when pod.Metadata?.Labels?.TryGetValue(RevisionLabel, out var podRevision) == true => podRevision,
            _ => null,
        };

        return int.TryParse(revisionText, NumberStyles.None, CultureInfo.InvariantCulture, out revision);
    }

    private static V1ConfigMap CreateConfigMap(int revision, int additionalEntryCount)
    {
        var data = new Dictionary<string, string>
        {
            ["benchmark-revision"] = revision.ToString(CultureInfo.InvariantCulture),
        };
        for (var index = 0; index < additionalEntryCount; index++)
        {
            data[$"value-{index.ToString(CultureInfo.InvariantCulture)}"] = "nested-map-value";
        }

        return new V1ConfigMap
        {
            ApiVersion = V1ConfigMap.KubeApiVersion,
            Kind = V1ConfigMap.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "yaml-refresh-benchmark",
                NamespaceProperty = "default",
            },
            Data = data,
        };
    }

    private static V1Pod CreatePod(int revision, int additionalAnnotationCount, int containerCount)
    {
        var annotations = new Dictionary<string, string>
        {
            ["example.com/config"] = "one",
            ["example.com/revision"] = revision.ToString(CultureInfo.InvariantCulture),
            ["example.com/owner"] = "benchmark",
        };
        for (var index = 0; index < additionalAnnotationCount; index++)
        {
            annotations[$"benchmark.kubeui.com/field-{index.ToString(CultureInfo.InvariantCulture)}"] = "nested-map-value";
        }

        return new V1Pod
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "yaml-refresh-benchmark",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string>
                {
                    ["app.kubernetes.io/name"] = "yaml-refresh-benchmark",
                    [RevisionLabel] = revision.ToString(CultureInfo.InvariantCulture),
                },
                Annotations = annotations,
            },
            Spec = new V1PodSpec
            {
                Containers = Enumerable.Range(0, containerCount)
                    .Select(containerIndex => new V1Container
                    {
                        Name = $"container-{containerIndex.ToString(CultureInfo.InvariantCulture)}",
                        Image = $"example.invalid/benchmark:{revision.ToString(CultureInfo.InvariantCulture)}",
                        Env = Enumerable.Range(0, 2)
                            .Select(variableIndex => new V1EnvVar
                            {
                                Name = $"SETTING_{variableIndex.ToString(CultureInfo.InvariantCulture)}",
                                Value = $"revision-{revision.ToString(CultureInfo.InvariantCulture)}-container-{containerIndex.ToString(CultureInfo.InvariantCulture)}-value-{variableIndex.ToString(CultureInfo.InvariantCulture)}",
                            })
                            .ToList(),
                        Ports = [new V1ContainerPort { ContainerPort = 8080 + containerIndex }],
                        Resources = new V1ResourceRequirements
                        {
                            Limits = new Dictionary<string, ResourceQuantity>
                            {
                                ["cpu"] = new ResourceQuantity("100m"),
                                ["memory"] = new ResourceQuantity("64Mi"),
                            },
                        },
                    })
                    .ToList(),
                InitContainers =
                [
                    new V1Container
                    {
                        Name = "init",
                        Image = "example.invalid/init:latest",
                        Args = ["prepare", "config"],
                    },
                ],
            },
        };
    }
}
