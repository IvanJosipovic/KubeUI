using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Overview.Views;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Views;
using PodPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views.PropertiesView;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using k8s.Models;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;
using System.Text.RegularExpressions;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void namespaced_resource_shows_namespace_property_item()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();
        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };
        var window = new Window { Content = view };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();
            items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void cluster_scoped_resource_hides_namespace_property_item()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();
        var viewModel = new ResourcePropertiesViewModel<V1Node>();
        viewModel.Initialize(workspace, new V1Node
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };
        var window = new Window { Content = view };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();
            items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void embedded_metrics_control_is_initialized_for_pod_properties()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();
        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };
        var window = new Window { Content = view };

        try
        {
            window.Show();
            view.Measure(Size.Infinity);
            view.Arrange(new Rect(view.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var scrollViewer = view.FindControl<ScrollViewer>("PART_ScrollViewer");
            scrollViewer.ShouldNotBeNull();
            scrollViewer.Offset.Y.ShouldBe(0);

            var podProperties = view.FindControl<StackPanel>("PART_Items")!
                .Children
                .OfType<PropertiesView>()
                .Single();
            var metrics = podProperties.Content
                .ShouldBeOfType<StackPanel>()
                .Children
                .OfType<MetricsControl>()
                .Single();

            WaitForCondition(() => metrics.ShowTabs);
            metrics.IsVisible.ShouldBeTrue();
            metrics.ShowTabs.ShouldBeTrue();
            metrics.ShowStatus.ShouldBeFalse();
            metrics.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory"]);
            metrics.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(1);
            metrics.Tabs.Single(x => x.Title == "Memory").Panels.Single().Series.Count.ShouldBe(1);
            foreach (var series in metrics.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.OfType<LineSeries<DateTimePoint>>())
            {
                (series.Values?.OfType<DateTimePoint>().Count() ?? 0).ShouldBe(2);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void embedded_metrics_control_is_initialized_for_node_properties()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.NodeMetrics.Add(new NodeMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
            },
            Usage = new Dictionary<string, ResourceQuantity>
            {
                ["cpu"] = new("750m"),
                ["memory"] = new("2Gi"),
            },
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();
        var viewModel = new ResourcePropertiesViewModel<V1Node>();
        viewModel.Initialize(workspace, new V1Node
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };
        var window = new Window { Content = view };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var nodeProperties = view.FindControl<StackPanel>("PART_Items")!
                .Children
                .OfType<KubeUI.Avalonia.Resources.Core.v1.Node.Views.PropertiesView>()
                .Single();
            var metrics = nodeProperties.Content
                .ShouldBeOfType<StackPanel>()
                .Children
                .OfType<MetricsControl>()
                .Single();

            WaitForCondition(() => metrics.ShowTabs);
            metrics.IsVisible.ShouldBeTrue();
            metrics.ShowTabs.ShouldBeTrue();
            metrics.ShowStatus.ShouldBeFalse();
            metrics.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory"]);
            metrics.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(1);
            metrics.Tabs.Single(x => x.Title == "Memory").Panels.Single().Series.Count.ShouldBe(1);
            foreach (var series in metrics.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.OfType<LineSeries<DateTimePoint>>())
            {
                (series.Values?.OfType<DateTimePoint>().Count() ?? 0).ShouldBe(2);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void metrics_control_uses_selected_time_range_for_prometheus_queries()
    {
        var observedRanges = new List<int?>();
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
            request =>
            {
                observedRanges.Add(request.RangeSeconds);
                return Task.FromResult(CreateMetricResult(request));
            });

        var workspace = CreateWorkspace(runtime);
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
        };

        try
        {
            window.Show();
            window.Measure(Size.Infinity);
            window.Arrange(new Rect(window.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var podProperties = view.FindControl<StackPanel>("PART_Items")!
                .Children
                .OfType<PodPropertiesView>()
                .Single();
            var metrics = podProperties.Content
                .ShouldBeOfType<StackPanel>()
                .Children
                .OfType<MetricsControl>()
                .Single();

            WaitForCondition(() => metrics.ShowTimeRangeSelector);
            metrics.ShowTimeRangeSelector.ShouldBeTrue();
            metrics.TimeRangeOptions.Select(x => x.Label).ShouldBe(["1h", "2h", "4h", "24h", "48h", "1 week", "1 month", "2 months"]);
            observedRanges.ShouldNotBeEmpty();
            observedRanges.ShouldAllBe(x => x == 3600);

            metrics.SelectedTimeRange = metrics.TimeRangeOptions.Single(x => x.RangeSeconds == 7200);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            observedRanges.Count(x => x == 7200).ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task pod_properties_metrics_show_loading_while_prometheus_request_is_pending()
    {
        var requestStarted = new TaskCompletionSource();
        var releaseRequest = new TaskCompletionSource();
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
            async request =>
            {
                requestStarted.TrySetResult();
                await releaseRequest.Task;
                return CreateMetricResult(request);
            });

        var workspace = CreateWorkspace(runtime);
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
            Width = 420,
            Height = 320,
        };

        try
        {
            window.Show();
            window.Measure(new Size(420, 320));
            window.Arrange(new Rect(0, 0, 420, 320));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            await requestStarted.Task;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metrics = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .Single(x => x.Container == null);

            WaitForCondition(() => metrics.ShowTabs);
            metrics.ShowStatus.ShouldBeTrue();
            metrics.StatusText.ShouldBe("Loading metrics...");
            metrics.ShowTabs.ShouldBeTrue();

            releaseRequest.TrySetResult();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            WaitForCondition(() => metrics.StatusText != "Loading metrics...");
            metrics.ShowTabs.ShouldBeTrue();
            metrics.StatusText.ShouldNotBe("Loading metrics...");
        }
        finally
        {
            releaseRequest.TrySetResult();
            window.Close();
        }
    }

    [AvaloniaFact]
    public void embedded_metrics_control_refreshes_when_active_metrics_backend_changes()
    {
        var cluster = new TestCluster();
        var workspace = cluster.CreateWorkspace();
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };
        var window = new Window { Content = view };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var podProperties = view.FindControl<StackPanel>("PART_Items")!
                .Children
                .OfType<PropertiesView>()
                .Single();
            var metrics = podProperties.Content
                .ShouldBeOfType<StackPanel>()
                .Children
                .OfType<MetricsControl>()
                .Single();

            WaitForCondition(() => metrics.ShowStatus);
            metrics.ShowStatus.ShouldBeTrue();
            metrics.StatusText.ShouldContain("Kubernetes Metrics Server");

            cluster.MetricsServiceType = KubeUI.Kubernetes.MetricsServiceType.Prometheus;

            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            WaitForCondition(() => metrics.ShowStatus && metrics.StatusText is not null && !metrics.StatusText.Contains("Kubernetes Metrics Server", StringComparison.Ordinal));
            metrics.IsVisible.ShouldBeTrue();
            metrics.ShowStatus.ShouldBeTrue();
            metrics.StatusText.ShouldNotContain("Kubernetes Metrics Server");
            metrics.StatusText.ShouldContain("No Prometheus metrics");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void metrics_control_keeps_last_successful_prometheus_data_when_refresh_fails()
    {
        var requestCount = 0;
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
            request =>
            {
                requestCount++;
                if (requestCount > 6)
                {
                    throw new InvalidOperationException("Transient Prometheus failure.");
                }

                return Task.FromResult(CreateMetricResult(request));
            });

        var workspace = CreateWorkspace(runtime);
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
        };

        try
        {
            window.Show();
            window.Measure(Size.Infinity);
            window.Arrange(new Rect(window.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var podProperties = view.FindControl<StackPanel>("PART_Items")!
                .Children
                .OfType<PodPropertiesView>()
                .Single();
            var metrics = podProperties.Content
                .ShouldBeOfType<StackPanel>()
                .Children
                .OfType<MetricsControl>()
                .Single();

            WaitForCondition(() => metrics.ShowTabs);
            metrics.ShowTabs.ShouldBeTrue();
            metrics.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory", "Network", "Filesystem"]);
            metrics.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(3);
            metrics.Tabs.Single(x => x.Title == "Memory").Panels.ShouldBeEmpty();
            metrics.Tabs.Single(x => x.Title == "Network").Panels.ShouldBeEmpty();
            metrics.Tabs.Single(x => x.Title == "Filesystem").Panels.ShouldBeEmpty();

            runtime.PodMetrics.Add(new PodMetrics
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "pod-2",
                    NamespaceProperty = "default",
                },
            });

            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            metrics.ShowTabs.ShouldBeTrue();
            metrics.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory", "Network", "Filesystem"]);
            metrics.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(3);
            metrics.Tabs.Single(x => x.Title == "Memory").Panels.ShouldBeEmpty();
            metrics.Tabs.Single(x => x.Title == "Network").Panels.ShouldBeEmpty();
            metrics.Tabs.Single(x => x.Title == "Filesystem").Panels.ShouldBeEmpty();
            metrics.StatusText.ShouldNotBe("Unable to load metrics.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task cluster_overview_uses_prometheus_usage_when_node_metrics_are_empty()
    {
        const double cpuUsage = 2.5;
        const long memoryUsageBytes = 3L * 1024 * 1024 * 1024;
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
            request =>
            {
                var nodesOption = request.Queries
                    .SelectMany(static query => query.Options)
                    .FirstOrDefault(static option => string.Equals(option.Key, "nodes", StringComparison.Ordinal))
                    .Value;

                nodesOption.ShouldBe(Regex.Escape("node-1"));

                return Task.FromResult(CreateMetricResult(request, new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["cpuUsage"] = cpuUsage,
                    ["workloadMemoryUsage"] = memoryUsageBytes,
                }));
            });

        await runtime.AddOrUpdateResource(new V1Node
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
            },
            Status = new V1NodeStatus
            {
                Capacity = new Dictionary<string, ResourceQuantity>
                {
                    ["cpu"] = new("8"),
                    ["memory"] = new("16Gi"),
                    ["pods"] = new("100"),
                },
                Allocatable = new Dictionary<string, ResourceQuantity>
                {
                    ["cpu"] = new("7500m"),
                    ["memory"] = new("15Gi"),
                    ["pods"] = new("90"),
                },
            },
        });

        var workspace = CreateWorkspace(runtime);
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var view = new MetricsControl
        {
            ClusterOverviewMode = true,
        };
        var window = new Window
        {
            Content = view,
            Width = 640,
            Height = 480,
        };

        try
        {
            view.Initialize(workspace);
            window.Show();
            window.Measure(new Size(640, 480));
            window.Arrange(new Rect(0, 0, 640, 480));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            view.ClusterCpuChart.HasData.ShouldBeTrue();
            view.ClusterMemoryChart.HasData.ShouldBeTrue();
            view.ClusterCpuChart.LegendItems.Single(x => x.Name == "Usage").Value.ShouldBe($"{cpuUsage:F2}c");
            view.ClusterMemoryChart.LegendItems.Single(x => x.Name == "Usage").Value.ShouldNotBe("n/a");
            view.ClusterMemoryChart.Series.Count.ShouldBeGreaterThan(1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void pod_container_properties_show_resources_section()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "main",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "main",
                        Image = "main:latest",
                    },
                ],
            },
        };

        var view = new PodPropertiesView
        {
            DataContext = pod,
        };
        view.Initialize(workspace);
        var window = new Window
        {
            Content = view,
        };

        try
        {
            window.Show();
            window.Width = 420;
            window.Height = 360;
            window.Measure(new Size(420, 360));
            window.Arrange(new Rect(0, 0, 420, 360));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metrics = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .Single(x => x.Container != null);

            metrics.IsVisible.ShouldBeTrue();
            metrics.ShowTabs.ShouldBeTrue();
            metrics.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory"]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void cluster_view_initializes_metrics_control_in_cluster_overview_mode()
    {
        const double cpuUsage = 2.5;
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
            request => Task.FromResult(CreateMetricResult(
                request,
                new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["cpuUsage"] = cpuUsage,
                    ["workloadMemoryUsage"] = 512 * 1024 * 1024,
                    ["podUsage"] = 12,
                })));

        runtime.AddOrUpdateResource(new V1Node
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
            },
            Status = new V1NodeStatus
            {
                Capacity = new Dictionary<string, ResourceQuantity>
                {
                    ["cpu"] = new("8"),
                    ["memory"] = new("16Gi"),
                    ["pods"] = new("100"),
                },
                Allocatable = new Dictionary<string, ResourceQuantity>
                {
                    ["cpu"] = new("7500m"),
                    ["memory"] = new("15Gi"),
                    ["pods"] = new("90"),
                },
            },
        }).GetAwaiter().GetResult();

        var workspace = CreateWorkspace(runtime);
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ClusterViewModel();
        viewModel.Initialize(workspace);

        var view = new ClusterView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
            Width = 900,
            Height = 700,
        };

        try
        {
            window.Show();
            window.Measure(new Size(900, 700));
            window.Arrange(new Rect(0, 0, 900, 700));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metrics = WaitForValue(() => view.FindControl<MetricsControl>("ClusterMetricsControl"), 3000);
            metrics.ClusterOverviewMode.ShouldBeTrue();
            WaitForCondition(() => metrics.ClusterCpuChart.HasData, 3000);
            metrics.IsVisible.ShouldBeTrue();
            metrics.ClusterCpuChart.HasData.ShouldBeTrue();
            metrics.ClusterMemoryChart.HasData.ShouldBeTrue();
            metrics.ClusterCpuChart.LegendItems.Single(x => x.Name == "Usage").Value.ShouldBe($"{cpuUsage:F2}c");
        }
        finally
        {
            window.Close();
            viewModel.Dispose();
        }
    }

    private static MetricResultSet CreateMetricResult(MetricRequest request)
    {
        var metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal);

        foreach (var query in request.Queries)
        {
            metrics[query.Name] =
            [
                new MetricSeries
                {
                    Name = query.Name,
                    Labels = new Dictionary<string, string>(StringComparer.Ordinal),
                    Points =
                    [
                        new MetricPoint(DateTimeOffset.UtcNow, 1),
                    ],
                },
            ];
        }

        return new MetricResultSet
        {
            Metrics = metrics,
        };
    }

    private static MetricResultSet CreateMetricResult(MetricRequest request, IReadOnlyDictionary<string, double> values)
    {
        var metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal);

        foreach (var query in request.Queries)
        {
            metrics[query.Name] =
            [
                new MetricSeries
                {
                    Name = query.Name,
                    Labels = new Dictionary<string, string>(StringComparer.Ordinal),
                    Points =
                    [
                        new MetricPoint(DateTimeOffset.UtcNow, values.TryGetValue(query.Name, out var value) ? value : 1),
                    ],
                },
            ];
        }

        return new MetricResultSet
        {
            Metrics = metrics,
        };
    }

    private static ClusterWorkspaceViewModel CreateWorkspace(IClusterRuntime runtime)
    {
        return ActivatorUtilities.CreateInstance<ClusterWorkspaceViewModel>(
            Application.Current?.GetRequiredService<IServiceProvider>() ?? throw new InvalidOperationException("Avalonia Application.Current is not initialized."),
            runtime);
    }

    [AvaloniaFact]
    public void pod_container_metrics_control_shows_metrics_for_regular_container()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "main",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container { Name = "main", Image = "main:latest" },
                ],
            },
        };

        var view = new MetricsControl
        {
            Pod = pod,
            Container = pod.Spec.Containers.Single(),
        };
        view.Initialize(workspace);

        view.Measure(Size.Infinity);
        view.Arrange(new Rect(view.DesiredSize));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        view.IsVisible.ShouldBeTrue();
        view.ShowTabs.ShouldBeTrue();
        view.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory"]);
        view.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(1);
        view.Tabs.Single(x => x.Title == "Memory").Panels.Single().Series.Count.ShouldBe(1);

        view.SelectedTab = view.Tabs.Single(x => x.Title == "Memory");
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        view.SelectedTab!.Title.ShouldBe("Memory");
        view.Tabs.Single(x => x.Title == "Memory").Panels.Single().Series.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public void pod_container_metrics_control_keeps_rendering_when_one_prometheus_panel_fails()
    {
        var requestCount = 0;
        var failAfter = int.MaxValue;
        var workspace = CreateWorkspace(
            new PrometheusMetricsClusterRuntime(
                new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
                request =>
                {
                    requestCount++;
                    if (requestCount > failAfter)
                    {
                        throw new InvalidOperationException("Transient Prometheus failure.");
                    }

                    return Task.FromResult(CreateMetricResult(request));
                }));
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "main",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container { Name = "main", Image = "main:latest" },
                ],
            },
        };

        var view = new MetricsControl
        {
            Pod = pod,
            Container = pod.Spec.Containers.Single(),
        };
        view.Initialize(workspace);

        view.Measure(Size.Infinity);
        view.Arrange(new Rect(view.DesiredSize));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        view.IsVisible.ShouldBeTrue();
        view.ShowTabs.ShouldBeTrue();
        view.ShowStatus.ShouldBeFalse();
        view.StatusText.ShouldBeNull();
        view.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory", "Filesystem"]);
        view.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(3);
        view.Tabs.Single(x => x.Title == "Memory").Panels.ShouldBeEmpty();
        view.Tabs.Single(x => x.Title == "Filesystem").Panels.ShouldBeEmpty();

        failAfter = requestCount + 1;
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-2",
                NamespaceProperty = "default",
            },
        });

        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        view.IsVisible.ShouldBeTrue();
        view.ShowTabs.ShouldBeTrue();
        view.ShowStatus.ShouldBeFalse();
        view.StatusText.ShouldBeNull();
        view.Tabs.Select(x => x.Title).ShouldBe(["CPU", "Memory", "Filesystem"]);
        view.Tabs.Single(x => x.Title == "CPU").Panels.Single().Series.Count.ShouldBe(3);
        view.Tabs.Single(x => x.Title == "Memory").Panels.ShouldBeEmpty();
        view.Tabs.Single(x => x.Title == "Filesystem").Panels.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void pod_container_metrics_control_uses_selected_time_range_for_prometheus_queries()
    {
        var observedRanges = new List<int?>();
        var runtime = new PrometheusMetricsClusterRuntime(
            new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
            request =>
            {
                observedRanges.Add(request.RangeSeconds);
                return Task.FromResult(CreateMetricResult(request));
            });

        var workspace = CreateWorkspace(runtime);
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container { Name = "main", Image = "main:latest" },
                ],
            },
        };

        var view = new MetricsControl
        {
            Pod = pod,
            Container = pod.Spec.Containers.Single(),
        };
        view.Initialize(workspace);

        view.Measure(Size.Infinity);
        view.Arrange(new Rect(view.DesiredSize));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        view.ShowTimeRangeSelector.ShouldBeTrue();
        view.TimeRangeOptions.Select(x => x.Label).ShouldBe(["1h", "2h", "4h", "24h", "48h", "1 week", "1 month", "2 months"]);
        observedRanges.ShouldNotBeEmpty();
        observedRanges.ShouldAllBe(x => x == 3600);

        view.SelectedTimeRange = view.TimeRangeOptions.Single(x => x.RangeSeconds == 7200);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        observedRanges.Count(x => x == 7200).ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void pod_metrics_chart_keeps_width_after_expanding_container_sections()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "main",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "main",
                        Image = "main:latest",
                    },
                ],
            },
        };

        var view = new PodPropertiesView
        {
            DataContext = pod,
        };
        view.Initialize(workspace);
        var window = new Window
        {
            Content = view,
            Width = 420,
            Height = 360,
        };

        try
        {
            window.Show();
            window.Measure(new Size(420, 360));
            window.Arrange(new Rect(0, 0, 420, 360));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metrics = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .Single(x => x.Container == null);
            var chart = WaitForRealizedChart(metrics);
            var initialWidth = chart.Bounds.Width;
            initialWidth.ShouldBeGreaterThan(1);

            var containersSection = view.GetVisualDescendants()
                .OfType<ExpandableSection>()
                .Single(x => string.Equals(x.Header?.ToString(), "Containers", StringComparison.Ordinal));
            containersSection.IsExpanded = false;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            containersSection.IsExpanded = true;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            WaitForCondition(() => chart.Bounds.Width > 1);
            chart.Bounds.Width.ShouldBeGreaterThanOrEqualTo(initialWidth - 10);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void pod_metrics_chart_reflows_when_window_resizes_while_offscreen()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "main",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "main",
                        Image = "main:latest",
                    },
                ],
            },
        };

        var view = new PodPropertiesView
        {
            DataContext = pod,
        };
        view.Initialize(workspace);
        var window = new Window
        {
            Content = view,
            Width = 420,
            Height = 280,
        };

        try
        {
            window.Show();
            window.Measure(new Size(420, 280));
            window.Arrange(new Rect(0, 0, 420, 280));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metrics = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .Single(x => x.Container == null);
            var chart = WaitForRealizedChart(metrics);
            var initialWidth = chart.Bounds.Width;
            initialWidth.ShouldBeGreaterThan(1);

            var scrollViewer = view.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();
            scrollViewer.ShouldNotBeNull();
            scrollViewer.Offset = new Vector(0, 800);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            window.Width = 620;
            window.Measure(new Size(620, 280));
            window.Arrange(new Rect(0, 0, 620, 280));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            scrollViewer.Offset = default;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            WaitForCondition(() => chart.Bounds.Width > initialWidth + 1);
            chart.Bounds.Width.ShouldBeGreaterThan(initialWidth + 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void pod_container_metrics_chart_uses_expanded_width_when_first_scrolled_into_view()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.PodMetrics.Add(new PodMetrics
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "main",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("125m"),
                        ["memory"] = new("96Mi"),
                    },
                },
            ],
        });
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "main",
                        Image = "main:latest",
                    },
                ],
            },
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
            Width = 420,
            Height = 280,
        };

        try
        {
            window.Show();
            window.Measure(new Size(420, 280));
            window.Arrange(new Rect(0, 0, 420, 280));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metricsControls = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .ToList();
            metricsControls.Count.ShouldBeGreaterThanOrEqualTo(2);

            var podMetrics = metricsControls.Single(x => x.Container == null);
            var podChart = WaitForRealizedChart(podMetrics);
            var initialPodChartWidth = podChart.Bounds.Width;
            initialPodChartWidth.ShouldBeGreaterThan(1);

            var containerMetrics = metricsControls.Single(x => x.Container?.Name == "main");
            window.Width = 620;
            window.Measure(new Size(620, 280));
            window.Arrange(new Rect(0, 0, 620, 280));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var resizedPodChartWidth = podChart.Bounds.Width;
            WaitForCondition(() => podChart.Bounds.Width > initialPodChartWidth + 1);
            resizedPodChartWidth.ShouldBeGreaterThan(initialPodChartWidth + 1);

            var scrollViewer = view.FindControl<ScrollViewer>("PART_ScrollViewer");
            scrollViewer.ShouldNotBeNull();
            scrollViewer.Offset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var containerChart = WaitForRealizedChart(containerMetrics);
            WaitForCondition(() => containerChart.Bounds.Width > 1);
            containerChart.Bounds.Width.ShouldBeGreaterThan(initialPodChartWidth + 1);
            containerChart.Bounds.Width.ShouldBeGreaterThanOrEqualTo(resizedPodChartWidth - 20);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void prometheus_pod_container_metrics_chart_uses_expanded_width_when_first_scrolled_into_view()
    {
        var workspace = CreateWorkspace(
            new PrometheusMetricsClusterRuntime(
                new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
                request => Task.FromResult(CreateMetricResult(request))));
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "main",
                        Image = "main:latest",
                    },
                ],
            },
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
            Width = 420,
            Height = 280,
        };

        try
        {
            window.Show();
            window.Measure(new Size(420, 280));
            window.Arrange(new Rect(0, 0, 420, 280));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var metricsControls = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .ToList();
            metricsControls.Count.ShouldBeGreaterThanOrEqualTo(2);

            var podMetrics = metricsControls.Single(x => x.Container == null);
            var podChart = WaitForRealizedChart(podMetrics);
            var initialPodChartWidth = podChart.Bounds.Width;
            initialPodChartWidth.ShouldBeGreaterThan(1);

            window.Width = 620;
            window.Measure(new Size(620, 280));
            window.Arrange(new Rect(0, 0, 620, 280));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var resizedPodChartWidth = podChart.Bounds.Width;
            WaitForCondition(() => podChart.Bounds.Width > initialPodChartWidth + 1);
            resizedPodChartWidth.ShouldBeGreaterThan(initialPodChartWidth + 1);

            var scrollViewer = view.FindControl<ScrollViewer>("PART_ScrollViewer");
            scrollViewer.ShouldNotBeNull();
            scrollViewer.Offset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var containerMetrics = metricsControls.Single(x => x.Container?.Name == "main");
            containerMetrics.SelectedTab = containerMetrics.Tabs.FirstOrDefault();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var containerChart = WaitForRealizedChart(containerMetrics);
            WaitForCondition(() => containerChart.Bounds.Width > 1);
            containerChart.Bounds.Width.ShouldBeGreaterThan(initialPodChartWidth + 1);
            containerChart.Bounds.Width.ShouldBeGreaterThanOrEqualTo(resizedPodChartWidth - 20);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void docked_pod_container_metrics_chart_uses_updated_right_pane_width_when_scrolled_into_view()
    {
        var workspace = CreateWorkspace(
            new PrometheusMetricsClusterRuntime(
                new TestClusterRuntime { Name = $"cluster-{Guid.NewGuid():N}", MetricsServiceType = MetricsServiceType.Prometheus },
                request => Task.FromResult(CreateMetricResult(request))));
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();

        var factory = Application.Current?.GetRequiredService<IFactory>()
            ?? throw new InvalidOperationException("Avalonia Application.Current is not initialized.");
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "main",
                        Image = "main:latest",
                    },
                ],
            },
        });

        var window = new Window
        {
            Content = dockControl,
            Width = 1200,
            Height = 700,
        };

        try
        {
            window.Show();
            factory.AddToRight(viewModel);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            window.Measure(new Size(1200, 700));
            window.Arrange(new Rect(0, 0, 1200, 700));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var view = WaitForValue(
                () => window.GetVisualDescendants()
                    .OfType<ResourcePropertiesView>()
                    .FirstOrDefault(x => x.IsVisible && ReferenceEquals(x.DataContext, viewModel)),
                3000);
            view.ShouldNotBeNull();

            var metricsControls = view.GetVisualDescendants()
                .OfType<MetricsControl>()
                .ToList();
            metricsControls.Count.ShouldBeGreaterThanOrEqualTo(2);

            var podMetrics = metricsControls.Single(x => x.Container == null);
            var podChart = WaitForRealizedChart(podMetrics);
            var initialPodChartWidth = podChart.Bounds.Width;
            initialPodChartWidth.ShouldBeGreaterThan(1);

            var rightDock = factory.GetDockable<IToolDock>("RightDock");
            rightDock.ShouldNotBeNull();
            rightDock.Proportion = 0.4;
            dockControl.InvalidateMeasure();
            dockControl.InvalidateArrange();
            dockControl.Measure(new Size(1200, 700));
            dockControl.Arrange(new Rect(0, 0, 1200, 700));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var resizedPodChartWidth = podChart.Bounds.Width;
            WaitForCondition(() => podChart.Bounds.Width > initialPodChartWidth + 1);
            resizedPodChartWidth.ShouldBeGreaterThan(initialPodChartWidth + 1);

            var scrollViewer = view.FindControl<ScrollViewer>("PART_ScrollViewer");
            scrollViewer.ShouldNotBeNull();
            scrollViewer.Offset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var containerMetrics = metricsControls.Single(x => x.Container?.Name == "main");
            var containerChart = WaitForRealizedChart(containerMetrics);
            WaitForCondition(() => containerChart.Bounds.Width > 1);
            containerChart.Bounds.Width.ShouldBeGreaterThan(initialPodChartWidth + 1);
            containerChart.Bounds.Width.ShouldBeGreaterThanOrEqualTo(resizedPodChartWidth - 20);
        }
        finally
        {
            window.Close();
        }
    }

    private static T WaitForValue<T>(Func<T?> getter, int timeoutMs = 1000) where T : class
    {
        T? value = null;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            Dispatcher.UIThread.RunJobs();
            value = getter();
            if (value != null)
            {
                return value;
            }

            System.Threading.Thread.Sleep(10);
        }

        value.ShouldNotBeNull();
        return value;
    }

    private static LiveChartsCore.SkiaSharpView.Avalonia.CartesianChart WaitForRealizedChart(MetricsControl metricsControl, int timeoutMs = 3000)
    {
        LiveChartsCore.SkiaSharpView.Avalonia.CartesianChart? chart = null;
        WaitForCondition(() =>
        {
            if (!metricsControl.ShowTabs || metricsControl.SelectedTab?.Panels.Count <= 0)
            {
                return false;
            }

            chart = metricsControl.GetVisualDescendants()
                .OfType<LiveChartsCore.SkiaSharpView.Avalonia.CartesianChart>()
                .FirstOrDefault(x => x.IsVisible && x.Bounds.Width > 1);

            return chart != null;
        }, timeoutMs);

        return chart!;
    }

    private static void WaitForCondition(Func<bool> predicate, int timeoutMs = 3000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            System.Threading.Thread.Sleep(10);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }
}
