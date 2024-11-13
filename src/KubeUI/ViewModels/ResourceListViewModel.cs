using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Dock.Model.Core;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Controls;
using Swordfish.NET.Collections;
using static KubeUI.Client.Cluster;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;

    [ObservableProperty]
    private ICluster _cluster;

    [ObservableProperty]
    private GroupApiVersionKind _kind;

    [ObservableProperty]
    private ConcurrentObservableDictionary<NamespacedName, T> _objects;

    [ObservableProperty]
    private object _selectedItem;

    [ObservableProperty]
    private ReadOnlyObservableCollection<KeyValuePair<NamespacedName, T>> _dataGridObjects;

    [ObservableProperty]
    private string _searchQuery;

    [ObservableProperty]
    private ResourceListViewDefinition<T> _viewDefinition;

    private IDisposable? _filter;

    [GeneratedRegex("types '(.+)' and '(.+)'", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeErrorRegex();

    public ResourceListViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListViewModel<T>>>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();
        _notificationManager = Application.Current.GetRequiredService<INotificationManager>();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Kind = GroupApiVersionKind.From<T>();
        Title = Kind.Kind.Humanize(LetterCasing.Title);
        Id = Cluster.Name + "-" + Kind;

        Objects = Cluster.GetObjectDictionary<T>();

        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        ViewDefinition = GetViewDefinition();

        SetFilter();
    }

    private void SetFilter()
    {
        _filter?.Dispose();

        _filter = Objects
            .ToObservableChangeSet<ConcurrentObservableDictionary<NamespacedName, T>, KeyValuePair<NamespacedName, T>>()
            .Filter(GenerateFilter())
            .Bind(out var filteredObjects, BindingOptions.NeverFireReset(false))
            .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Set Namespace Filter: {ns}", typeof(T)));

        DataGridObjects = filteredObjects;
    }

    private Func<KeyValuePair<NamespacedName, T>, bool> GenerateFilter()
    {
        try
        {
            var param = Expression.Parameter(typeof(KeyValuePair<NamespacedName, T>), "p");

            var key = Expression.PropertyOrField(param, "Key");

            var value = Expression.PropertyOrField(param, "Value");

            BinaryExpression? body = null;

            BinaryExpression? namespaceFilter = null;

            BinaryExpression? searchFilter = null;

            if (Cluster.SelectedNamespaces != null && ViewDefinition.ShowNamespaces)
            {
                foreach (var item in Cluster.SelectedNamespaces)
                {
                    var expression = Expression.Equal(
                            Expression.PropertyOrField(key, "Namespace"),
                            Expression.Constant(item.Name())
                       );

                    namespaceFilter = namespaceFilter == null ? expression : Expression.OrElse(namespaceFilter, expression);
                }
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var method = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(string), typeof(StringComparison)]);

                foreach (var query in SearchQuery.Split(' '))
                {
                    if (string.IsNullOrEmpty(query))
                    {
                        continue;
                    }

                    BinaryExpression? wordFilter = null;

                    foreach (var column in ViewDefinition.Columns)
                    {
                        var someValue = Expression.Constant(query, typeof(string));

                        var colType = column.GetType();

                        var columnDisplay = (Func<T, string>)colType.GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Display)).GetValue(column);

                        columnDisplay ??= (Func<T, string>)colType.GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Field)).GetValue(column);

                        var funcCall = Expression.Call(Expression.Constant(columnDisplay), columnDisplay.GetType().GetMethod("Invoke"), value);

                        var expression = Expression.GreaterThanOrEqual(Expression.Call(funcCall, method, someValue, Expression.Constant(StringComparison.OrdinalIgnoreCase)), Expression.Constant(0));

                        wordFilter = wordFilter == null ? expression : Expression.OrElse(wordFilter, expression);
                    }

                    searchFilter = searchFilter == null ? wordFilter : Expression.AndAlso(searchFilter, wordFilter);
                }
            }

            if (namespaceFilter != null && searchFilter == null)
            {
                body = namespaceFilter;
            }
            else if (namespaceFilter == null && searchFilter != null)
            {
                body = searchFilter;
            }
            else if (namespaceFilter != null && searchFilter != null)
            {
                body = Expression.AndAlso(namespaceFilter, searchFilter);
            }
            else
            {
                body = Expression.Equal(Expression.Constant(true), Expression.Constant(true));
            }

            return Expression.Lambda<Func<KeyValuePair<NamespacedName, T>, bool>>(body, param).Compile();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Generating Namespace Filter Expression");

            throw new Exception("Error Generating Namespace Filter Expression", ex);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchQuery))
        {
            SetFilter();
        }
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetFilter();
    }

    private ResourceListViewDefinition<T> GetViewDefinition()
    {
        var resourceType = typeof(T);

        ResourceListViewDefinition<T> definition = new();

        if (resourceType == typeof(V1Node))
        {
            definition.ShowNamespaces = false;
            definition.ShowNewResource = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Instance Type",
                    Field = x => x.Metadata.Labels.TryGetValue("node.kubernetes.io/instance-type", out var value) ? value : "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Node, decimal>()
                {
                    Name = "CPU",
                    Field = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal() : 0,
                    Display = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal().ToString("0.##") + "c" : "0c",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, decimal>()
                {
                    Name = "Memory",
                    Field = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDecimal() : 0,
                    Display = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0Gi",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, decimal>()
                {
                    Name = "Disk",
                    Field = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? value.ToDecimal() : 0,
                    Display = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Taints",
                    Field = x => x?.Spec?.Taints?.Select(x => $"{x.Key}={x.Effect}").Aggregate((x,y) => $"{x}, {y}") ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Version",
                    Field = x => x.Status.NodeInfo.KubeletVersion,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Conditions.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "Cordon",
                    IconResource = "stop_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.CordonNodeCommand),
                    CommandParameterPath = "SelectedItems",
                },
                new()
                {
                    Header = "UnCordon",
                    IconResource = "play_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.UnCordonNodeCommand),
                    CommandParameterPath = "SelectedItems",
                },
                new()
                {
                    Header = "Drain",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.DrainNodeCommand),
                    CommandParameterPath = "SelectedItems",
                },
            ];
        }
        else if (resourceType == typeof(V1Namespace))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Labels",
                    Field = x => x.Metadata.Labels.Select(x => x.Key + "=" + x.Value).Aggregate((x,y) => x + ", " + y),
                    Width = "2*"
                },
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(Corev1Event))
        {
            definition.Columns =
            [
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Type",
                    Field = x => x?.Type ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Message",
                    Field = x => x?.Message ?? "",
                    Width = "4*"
                },
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Involved Object",
                    Field = x => x?.InvolvedObject?.Name ?? "",
                    Width = "*"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Source",
                    Field = x => x?.Source?.Component ?? "",
                    Width = "*"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, int>()
                {
                    Name = "Count",
                    Display = x => (x.Count ?? 0).ToString(),
                    Field = x => x.Count ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<Corev1Event, DateTime?>()
                {
                    Name = "Last Seen",
                    CustomControl = typeof(LastSeenCell),
                    Field = x => x.LastTimestamp,
                    Display = x => x.LastTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    Sort = SortDirection.Descending,
                    Width = "80"
                },
                AgeColumn(),
            ];

            definition.SetStyle = () => [
                new Style<DataGridRow>()
                    .Setter(DataGridRow.ForegroundProperty, new Binding("Value.Type")
                    {
                        Converter = new FuncValueConverter<string, IBrush>(x =>
                        {
                            if (string.Equals(x, "Warning", StringComparison.Ordinal))
                            {
                                return Brushes.Red;
                            }

                            if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
                            {
                                return Brushes.Black; //todo reference style
                            }

                            return Brushes.White; //todo reference style
                        }),
                    }),
                ];
        }
        else if (resourceType == typeof(V1Pod))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1Pod, int>()
                {
                    Name = "Containers",
                    CustomControl = typeof(PodContainerCell),
                    Field = x => x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0),
                    Display = x => (x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0)).ToString(),
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Pod, int>()
                {
                    Name = "Restarts",
                    Field = x => x.Status.ContainerStatuses.Sum(x => x.RestartCount),
                    Display = x => x.Status.ContainerStatuses?.Sum(x => x.RestartCount).ToString() ?? "0",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Controlled By",
                    Field = x => x.Metadata.OwnerReferences?.FirstOrDefault()?.Name ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Node",
                    Field = x => x.Spec.NodeName ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "QoS",
                    Field = x => x.Status.QosClass ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase ?? "",
                    CustomControl = typeof(PodStatusCell),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];

            if (Cluster.IsMetricsAvailable)
            {
                definition.Columns.Insert(3, new ResourceListViewDefinitionColumn<V1Pod, decimal>()
                {
                    Name = "CPU",
                    CustomControl = typeof(PodMetricCPUCell),
                    Field = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]) ?? 0,
                    Display = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]).ToString() ?? "",
                    Width = "80"
                });
                definition.Columns.Insert(4, new ResourceListViewDefinitionColumn<V1Pod, decimal>()
                {
                    Name = "Memory",
                    CustomControl = typeof(PodMetricMemoryCell),
                    Field = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]) ?? 0,
                    Display = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]).ToString() ?? "",
                    Width = "80"
                });
            }

            definition.MenuItems =
            [
                new()
                {
                    Header = "View Console",
                    IconResource = "desktop_regular",
                    MenuItems =
                    [
                        new()
                        {
                            Header = "Init",
                            ItemSourcePath = "SelectedItem.Value.Spec.InitContainers",
                            ItemTemplate = new()
                            {
                                HeaderBinding = new Binding(nameof(V1Container.Name)),
                                CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewConsoleCommand),
                                CommandParameterPath = ".",
                            }
                        },
                        new()
                        {
                            Header = "Normal",
                            ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                            ItemTemplate = new()
                            {
                                HeaderBinding = new Binding(nameof(V1Container.Name)),
                                CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewConsoleCommand),
                                CommandParameterPath = ".",
                            }
                        },
                    ]
                },
                new()
                {
                    Header = "View Logs",
                    IconResource = "text_description_regular",
                    MenuItems = [
                        new()
                        {
                            Header = "Init",
                            ItemSourcePath = "SelectedItem.Value.Spec.InitContainers",
                            ItemTemplate = new()
                            {
                                HeaderBinding = new Binding(nameof(V1Container.Name)),
                                CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewLogsCommand),
                                CommandParameterPath = ".",
                            }
                        },
                        new()
                        {
                            Header = "Normal",
                            ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                            ItemTemplate = new()
                            {
                                HeaderBinding = new Binding(nameof(V1Container.Name)),
                                CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewLogsCommand),
                                CommandParameterPath = ".",
                            }
                        },
                    ],
                },
                new()
                {
                    Header = "Port Forwarding",
                    ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                    IconResource = "ic_fluent_cloud_flow_filled",
                    ItemTemplate = new()
                    {
                        HeaderBinding = new Binding(nameof(V1Container.Name)),
                        ItemSourcePath = nameof(V1Container.Ports),
                        ItemTemplate = new()
                        {
                            HeaderBinding = new MultiBinding()
                            {
                                Bindings =
                                [
                                    new Binding(nameof(V1ContainerPort.Name)),
                                    new Binding(nameof(V1ContainerPort.ContainerPort))
                                ],
                                StringFormat = "{0} - {1}"
                            },
                            CommandPath = nameof(ResourceListViewModel<V1Pod>.PortForwardCommand),
                            CommandParameterPath = ".",
                        }
                    }
                }
            ];
        }
        else if (resourceType == typeof(V1Deployment))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Pods",
                    Display = x => $"{x.Status?.AvailableReplicas.GetValueOrDefault()}/{x.Spec?.Replicas}",
                    Field = x => x.Status.AvailableReplicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Replicas",
                    Display = x => x.Spec.Replicas.GetValueOrDefault().ToString(),
                    Field = x => x.Spec.Replicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, string>()
                {
                    Name = "Available",
                    Field = x => x.Status.Conditions == null ? "" : x.Status.Conditions.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "Restart",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartDeploymentCommand),
                    CommandParameterPath = "SelectedItem.Value"
                },
            ];
        }
        else if (resourceType == typeof(V1DaemonSet))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1DaemonSet, int>()
                {
                    Name = "Pods",
                    Display = x => x.Status.NumberReady.ToString(),
                    Field = x => x.Status.NumberReady,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1DaemonSet, string>()
                {
                    Name = "Node Selector",
                    Display = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
                    Field = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "Restart",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartDaemonSetCommand),
                    CommandParameterPath = "SelectedItem.Value"
                },
            ];
        }
        else if (resourceType == typeof(V1StatefulSet))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1StatefulSet, int>()
                {
                    Name = "Replicas",
                    Display = x => x.Status.Replicas.ToString(),
                    Field = x => x.Status.Replicas,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "Restart",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartStatefulSetCommand),
                    CommandParameterPath = "SelectedItem.Value"
                },
            ];
        }
        else if (resourceType == typeof(V1ReplicaSet))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1ReplicaSet, int>()
                {
                    Name = "Desired",
                    Display = x => (x.Spec.Replicas ?? 0).ToString(),
                    Field = x => x.Spec.Replicas ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1ReplicaSet, int>()
                {
                    Name = "Current",
                    Display = x => x.Status.Replicas.ToString(),
                    Field = x => x.Status.Replicas,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1ReplicaSet, int>()
                {
                    Name = "Ready",
                    Display = x => x.Status.Replicas.ToString(),
                    Field = x => x.Status.ReadyReplicas ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "Restart",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartReplicaSetCommand),
                    CommandParameterPath = "SelectedItem.Value"
                },
            ];
        }
        else if (resourceType == typeof(V1Job))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Job, int>()
                {
                    Name = "Completions",
                    Display = x => $"{x.Status.Succeeded ?? 0}/{x.Spec.Completions ?? 0}",
                    Field = x => x.Spec.Completions ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
                new ResourceListViewDefinitionColumn<V1Job, string>()
                {
                    Name = "Conditions",
                    Field = x => x.Status?.Conditions?.FirstOrDefault(y => y.Status == "True")?.Type ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];
        }
        else if (resourceType == typeof(V1CronJob))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1CronJob, string>()
                {
                    Name = "Schedule",
                    Display = x => x.Spec.Schedule,
                    Field = x => x.Spec.Schedule,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1CronJob, bool>()
                {
                    Name = "Suspend",
                    Display = x => (x.Spec.Suspend ?? false).ToString(),
                    Field = x => x.Spec.Suspend ?? false,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1CronJob, int>()
                {
                    Name = "Active",
                    Display = x => x.Status.Active?.Count.ToString() ?? "",
                    Field = x => x.Status.Active?.Count ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1CronJob, DateTime?>()
                {
                    Name = "Last Schedule",
                    Display = x => x.Status.LastScheduleTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    Field = x => x.Status.LastScheduleTime,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1ConfigMap))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1ConfigMap, string>()
                {
                    Name = "Keys",
                    Display = x => x.Data != null && x.Data.Keys.Count > 0 ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                    Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1Secret))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Secret, string>()
                {
                    Name = "Labels",
                    Display = x => x.Metadata?.Labels != null ? x.Metadata.Labels.Keys.Aggregate((a,b) => a + ", " + b) : "",
                    Field = x => x.Metadata?.Labels?.Keys.FirstOrDefault() ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Secret, string>()
                {
                    Name = "Keys",
                    Display = x => x.Data != null ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                    Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Secret, string>()
                {
                    Name = "Type",
                    Field = x => x.Type,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V2HorizontalPodAutoscaler))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, int>()
                {
                    Name = "Min Pods",
                    Display = x => (x.Spec.MinReplicas ?? 0).ToString(),
                    Field = x => x.Spec.MinReplicas ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, int>()
                {
                    Name = "Max Pods",
                    Display = x => x.Spec.MaxReplicas.ToString(),
                    Field = x => x.Spec.MaxReplicas,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, int>()
                {
                    Name = "Replica",
                    Display = x => (x.Status.CurrentReplicas ?? 0).ToString(),
                    Field = x => x.Status.CurrentReplicas ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
                new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, string>()
                {
                    Name = "Conditions",
                    Display = x => x.Status.Conditions.FirstOrDefault(y => y.Status == "True").Type,
                    Field = x => x.Status.Conditions.First(y => y.Status == "True").Type,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];
        }
        else if (resourceType == typeof(V1PodDisruptionBudget))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, IntstrIntOrString>()
                {
                    Name = "Min Avalilable",
                    Display = x => x.Spec.MinAvailable != null ? x.Spec.MinAvailable.Value : "",
                    Field = x => x.Spec.MinAvailable,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, IntstrIntOrString>()
                {
                    Name = "Max Unavailable",
                    Display = x => x.Spec.MaxUnavailable != null ? x.Spec.MaxUnavailable.Value : "",
                    Field = x => x.Spec.MaxUnavailable,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, int>()
                {
                    Name = "Current Healthy",
                    Display = x => x.Status.CurrentHealthy.ToString(),
                    Field = x => x.Status.CurrentHealthy,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, int>()
                {
                    Name = "Desired Healthy",
                    Display = x => x.Status.DesiredHealthy.ToString(),
                    Field = x => x.Status.DesiredHealthy,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1PriorityClass))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1PriorityClass, int>()
                {
                    Name = "Value",
                    Display = x => x.Value.ToString(),
                    Field = x => x.Value,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1PriorityClass, bool?>()
                {
                    Name = "Global Default",
                    Display = x => (x.GlobalDefault ?? false).ToString(),
                    Field = x => x.GlobalDefault ?? false,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1RuntimeClass))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1RuntimeClass, string>()
                {
                    Name = "Handler",
                    Display = x => x.Handler,
                    Field = x => x.Handler,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1Lease))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Lease, string>()
                {
                    Name = "Holder",
                    Display = x => x.Spec.HolderIdentity ?? "",
                    Field = x => x.Spec.HolderIdentity ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1MutatingWebhookConfiguration))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1MutatingWebhookConfiguration, int>()
                {
                    Name = "Webhooks",
                    Display = x => x.Webhooks?.Count.ToString() ?? "",
                    Field = x => x.Webhooks?.Count ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1ValidatingWebhookConfiguration))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1ValidatingWebhookConfiguration, int>()
                {
                    Name = "Webhooks",
                    Display = x => x.Webhooks.Count.ToString(),
                    Field = x => x.Webhooks.Count,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1Service))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Service, string>()
                {
                    Name = "Type",
                    Display = x => x.Spec.Type,
                    Field = x => x.Spec.Type,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Service, string>()
                {
                    Name = "Cluster IP",
                    Display = x => x.Spec.ClusterIP,
                    Field = x => x.Spec.ClusterIP,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Service, int>()
                {
                    Name = "Ports",
                    Display = x => x.Spec.Ports.Select((a) => $"{a.Port}{(string.IsNullOrEmpty(a.Name) ? "" : ":" + a.Name)}/{a.Protocol}").Aggregate((a,b) => a + ", " + b),
                    Field = x => x.Spec.Ports.FirstOrDefault().Port,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "Port Forwarding",
                    ItemSourcePath = "SelectedItem.Value.Spec.Ports",
                    IconResource = "ic_fluent_cloud_flow_filled",
                    ItemTemplate = new()
                    {
                        HeaderBinding = new MultiBinding()
                        {
                            Bindings =
                            [
                                new Binding(nameof(V1ServicePort.Name)),
                                new Binding(nameof(V1ServicePort.Port))
                            ],
                            StringFormat = "{0} - {1}"
                        },                        CommandPath = nameof(ResourceListViewModel<V1Pod>.PortForwardServiceCommand),
                        CommandParameterPath = ".",
                    }
                }
            ];
        }
        else if (resourceType == typeof(V1Endpoints))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Endpoints, int>()
                {
                    Name = "Endpoints",
                    Display = x => x.Subsets != null ? x.Subsets.SelectMany(y => y.Ports.Select(z => y.Addresses[0].Ip + ":" + z.Port)).Aggregate((a,b) => a + ", " + b) : "",
                    Field = x => x.Subsets != null ? x.Subsets[0].Ports[0].Port : 0,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1EndpointSlice))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1Ingress))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Ingress, string>()
                {
                    Name = "Load Balancers",
                    Display = x => x.Status.LoadBalancer.Ingress.Select(x => x.Ip).Aggregate((a,b) => a + ", " + b),
                    Field = x => x.Status.LoadBalancer.Ingress.Count > 0 ? x.Status.LoadBalancer.Ingress[0].Ip : "",
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1Ingress, string>()
                {
                    Name = "Rules",
                    Display = x => x.Spec.Rules.Select(z => $"http://{z.Host}{z.Http.Paths[0].Path}").Aggregate((a,b) => a + ", " + b),
                    Field = x => x.Spec.Rules.Count > 0 ? x.Spec.Rules[0].Host : "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1IngressClass))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1IngressClass, string>()
                {
                    Name = "Controller",
                    Field = x => x.Spec.Controller,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1IngressClass, string>()
                {
                    Name = "API Group",
                    Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.ApiGroup : "",
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1IngressClass, string>()
                {
                    Name = "Scope",
                    Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Scope : "",
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1IngressClass, string>()
                {
                    Name = "Kind",
                    Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Kind : "",
                    Width = "*",
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1NetworkPolicy))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1NetworkPolicy, string>()
                {
                    Name = "Policy Types",
                    Display = x => x.Spec.PolicyTypes.Aggregate((a,b) => a + ", " + b),
                    Field = x => x.Spec.PolicyTypes.Count > 0 ? x.Spec.PolicyTypes[0] : "",
                    Width = "*",
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1PersistentVolumeClaim))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
                {
                    Name = "Storage Class",
                    Field = x => x.Spec.StorageClassName,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
                {
                    Name = "Size",
                    Field = x => x.Spec.Resources.Requests["storage"].Value,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
                new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
            ];
        }
        else if (resourceType == typeof(V1PersistentVolume))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
                {
                    Name = "Storage Class",
                    Field = x => x.Spec.StorageClassName,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
                {
                    Name = "Size",
                    Field = x => x.Spec.Capacity["storage"].Value,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
                {
                    Name = "Claim",
                    Field = x => x.Spec.ClaimRef.Name,
                    Width = "*",
                },
                AgeColumn(),
                new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
            ];
        }
        else if (resourceType == typeof(V1StorageClass))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1StorageClass, string>()
                {
                    Name = "Provisioner",
                    Field = x => x.Provisioner,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1StorageClass, string>()
                {
                    Name = "Reclaim Policy",
                    Field = x => x.ReclaimPolicy,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1StorageClass, string>()
                {
                    Name = "Default", // "storageclass.kubernetes.io/is-default-class":"true"
                    Field = x => x.Metadata.Annotations?.ContainsKey("storageclass.kubernetes.io/is-default-class") == true ?
                                    x.Metadata.Annotations["storageclass.kubernetes.io/is-default-class"] : "false",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1ServiceAccount))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1ClusterRole))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1Role))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                AgeColumn(),
            ];
        }
        else if (resourceType == typeof(V1ClusterRoleBinding))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1ClusterRoleBinding, string>()
                {
                    Name = "Bindings",
                    Field = x => x.Subjects == null || x.Subjects.Count == 0 ? "" : x.Subjects.Select(y => y.Name).Aggregate((a,b) => a + ", " + b),
                    Width = "*",
                },
                AgeColumn()
            ];
        }
        else if (resourceType == typeof(V1RoleBinding))
        {
            definition.Columns =
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1RoleBinding, string>()
                {
                    Name = "Bindings",
                    Field = x => x.Subjects.Select(y => y.Name).Aggregate((a,b) => a + ", " + b),
                    Width = "*",
                },
                AgeColumn()
            ];
        }
        else if (resourceType == typeof(V1CustomResourceDefinition))
        {
            definition.ShowNamespaces = false;

            definition.Columns =
            [
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Name",
                    Display = x => x.Spec.Names.Kind.Humanize(LetterCasing.Title),
                    Field = x => x.Spec.Names.Kind,
                    Sort = SortDirection.Ascending,
                    Width = "2*",
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Group",
                    Field = x => x.Spec.Group,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Version",
                    Field = x => x.Spec.Versions.First(x => x.Storage).Name,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Scope",
                    Field = x => x.Spec.Scope,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];

            definition.MenuItems =
            [
                new()
                {
                    Header = "View Items",
                    CommandParameterPath = "SelectedItem.Value",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.ListCRDCommand)
                },
            ];

        }
        else
        {
            definition.Columns = [];

            // Add Name Column
            var nameColumn = NameColumn(SortDirection.Ascending);

            definition.Columns.Add(nameColumn);

            // Check if resource type is CRD
            // TODO: Find a better way to identify CRDs
            if (resourceType.Namespace.StartsWith("KubeUI.Models"))
            {
                // Generate CRD Columns
                var crd = Cluster.GetObject<V1CustomResourceDefinition>(null, Kind.PluralNameGroup);

                var version = crd.Spec.Versions.First(x => x.Storage);

                //Check if its a namespaced crd
                if (crd.Spec.Scope == "Namespaced")
                {
                    // Add Namespace Column
                    var nsColumn = NamespaceColumn();

                    definition.Columns.Add(nsColumn);
                }
                else
                {
                    definition.ShowNamespaces = false;
                }

                if (version.AdditionalPrinterColumns != null)
                {
                    foreach (var item in version.AdditionalPrinterColumns)
                    {
                        start:

                        try
                        {
                            if (item.JsonPath == ".metadata.creationTimestamp")
                            {
                                continue;
                            }

                            if (item.Type == "string")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, string>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, string>()
                                {
                                    Name = item.Name,
                                    Field = exp.Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "number")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, double>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, double>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "integer" && item.Format == "int64")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, long>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, long>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "integer")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, int>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, int>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "date")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, DateTime>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, DateTime>()
                                {
                                    Name = item.Name,
                                    Field = exp.Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "boolean")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, bool>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, bool>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if(item.Type == "enum")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, Enum>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, string>()
                                {
                                    Name = item.Name,
                                    Field = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
                                    //Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else
                            {
                                _logger.LogWarning("CRD Column Type not supported: {type}", item.Type);
                            }
                        }
                        catch (InvalidOperationException ex) when (ex.Message.StartsWith("No coercion operator is defined between types", StringComparison.Ordinal))
                        {
                            // The type defined in the AdditionalPrinterColumn is not correct
                            var match = TypeErrorRegex().Match(ex.Message);
                            if (match.Success)
                            {
                                var typeString = match.Groups[1].Value;

                                if (typeString.StartsWith("System.Nullable`1[", StringComparison.Ordinal))
                                {
                                    typeString = typeString["System.Nullable`1[".Length..].TrimEnd(']');
                                }

                                var type = Type.GetType(typeString);

                                if (type == null)
                                {
                                    type = resourceType.Assembly.GetType(typeString);

                                    if (type == null)
                                    {
                                        _logger.LogError(ex, "Unable to load type for column: {Name} in type {type}", typeString, crd.Name());
                                        continue;
                                    }
                                }

                                if (type.IsGenericType)
                                {
                                    type = type.GenericTypeArguments[0];
                                }

                                if (type == typeof(string))
                                {
                                    item.Type = "string";
                                }
                                else if (type == typeof(double))
                                {
                                    item.Type = "number";
                                }
                                else if (type == typeof(int))
                                {
                                    item.Type = "integer";
                                }
                                else if (type == typeof(long))
                                {
                                    item.Type = "integer";
                                    item.Format = "int64";
                                }
                                else if (type == typeof(DateTime))
                                {
                                    item.Type = "date";
                                }
                                else if (type == typeof(bool))
                                {
                                    item.Type = "boolean";
                                }
                                else if (type.IsEnum)
                                {
                                    item.Type = "enum";
                                }
                                else
                                {
                                    _logger.LogError(ex, "Unable to generate CRD Column: {Name} with type {Type}", item.Name, type);
                                    continue;
                                }

                                goto start;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical(ex, "Unable to generate CRD Column: {Name} in {crd}", item.Name, crd.Name());
                        }
                    }
                }
            }
            else
            {
                _logger.LogError("Unexpected CRD namespace {res}", resourceType.Namespace);
            }

            // Add Age Column
            var ageColumn = new ResourceListViewDefinitionColumn<T, DateTime?>()
            {
                Name = "Age",
                CustomControl = typeof(AgeCell),
                Field = x => x.Metadata.CreationTimestamp,
                Display = x => x.Metadata.CreationTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                Width = "80"
            };

            definition.Columns.Add(ageColumn);
        }

        return definition;
    }

    private static Expression<Func<T, string>> TransformToFuncOfString(Expression expression, ReadOnlyCollection<ParameterExpression> parameters)
    {
        // Check if the expression type is an enum
        if (expression.Type == typeof(Enum))
        {
            // Create a method to get the enum member name from the JsonStringEnumMemberNameAttribute
            var getEnumMemberNameMethod = typeof(ResourceListViewModel<V1Pod>).GetMethod(nameof(GetEnumMemberName), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(expression.Type);

            // Call the method to get the enum member name
            var bodyAsString = Expression.Call(getEnumMemberNameMethod, expression);

            // Create a new lambda expression
            return Expression.Lambda<Func<T, string>>(bodyAsString, parameters);
        }
        else
        {
            Expression bodyAsString;

            // Convert the body of the original expression to return a string
            if (Nullable.GetUnderlyingType(expression.Type) != null)
            {
                bodyAsString = Expression.Condition(
                    Expression.Equal(expression, Expression.Constant(null, expression.Type)),
                    Expression.Constant(string.Empty),
                    Expression.Call(expression, nameof(object.ToString), Type.EmptyTypes)
                );
            }
            else
            {
                bodyAsString = Expression.Call(expression, nameof(object.ToString), Type.EmptyTypes);
            }

            // Create a new lambda expression
            return Expression.Lambda<Func<T, string>>(bodyAsString, parameters);
        }
    }

    private static string GetEnumMemberName<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attribute = memberInfo.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attribute != null)
            {
                return attribute.Name;
            }
        }
        return enumValue.ToString();
    }

    #region Actions

    [RelayCommand(CanExecute = nameof(CanNewResource))]
    private void NewResource()
    {
        var resource = Activator.CreateInstance<T>();
        resource.Kind = Kind.Kind;
        resource.ApiVersion = Kind.ApiVersion;
        resource.Metadata = new()
        {
            Name = "temp"
        };

        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        vm.Cluster = Cluster;
        vm.Object = resource;
        vm.Id = $"{nameof(ViewYaml)}-{Cluster.Name}-new";
        vm.EditMode = true;

        Factory.AddToBottom(vm);
    }

    private bool CanNewResource()
    {
        return Cluster.CanIAnyNamespace(typeof(T), Verb.Create);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Resources.ResourceListViewModel_Delete_Title,
            Content = string.Format(Resources.ResourceListViewModel_Delete_Content, items.Count),
            PrimaryButtonText = Resources.ResourceListViewModel_Delete_Primary,
            SecondaryButtonText = Resources.ResourceListViewModel_Delete_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            var exceptions = new List<Exception>();

            foreach (var item in items.Cast<KeyValuePair<NamespacedName, T>>().ToList())
            {
                try
                {
                    await Cluster.Delete<T>(item.Value);

                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Utilities.HandleException(_logger, _notificationManager, ex, $"Error Deleting {item.Key.Namespace}/{item.Key.Name}", sendNotification: true);
                }
            }
        }
    }

    private bool CanDelete(IList items)
    {
        if (items.Count == 0)
        {
            return false;
        }

        foreach (var item in items)
        {
            var ns = (NamespacedName)item.GetType().GetProperty("Key")!.GetValue(item);

            if (!Cluster.CanI<T>(Verb.Delete, ns.Namespace))
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    private void View(object item)
    {
        var instance = Application.Current.GetRequiredService<ResourcePropertiesViewModel<T>>();
        instance.Initialize(Cluster, ((KeyValuePair<NamespacedName, T>)(item)).Value);
        instance.CanFloat = false;

        Factory?.AddToRight(instance);
    }

    private bool CanView(object item)
    {
        return item != null;
    }

    [RelayCommand(CanExecute = nameof(CanViewYaml))]
    private void ViewYaml(object item)
    {
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

        vm.Initialize(Cluster, ((KeyValuePair<NamespacedName, T>)item).Value);

        Factory.AddToBottom(vm);
    }

    private bool CanViewYaml(object item)
    {
        return item != null;
    }

    [RelayCommand(CanExecute = nameof(CanViewLogs))]
    private async Task ViewLogs(V1Container container)
    {
        var vm = Application.Current.GetRequiredService<PodLogsViewModel>();
        vm.Cluster = Cluster;
        vm.Object = ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Value;
        vm.ContainerName = container.Name;
        vm.Id = $"{nameof(ViewLogs)}-{Cluster.Name}-{((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key}-{container.Name}";

        if (Factory.AddToBottom(vm))
        {
        try
        {
            await vm.Connect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing logs");
            return;
        }
        }
    }

    private bool CanViewLogs(V1Container? container)
    {
        return container != null && Cluster.CanI<V1Pod>(Verb.Get, ((KeyValuePair<NamespacedName,V1Pod>)SelectedItem).Key.Namespace, "log");
    }

    [RelayCommand(CanExecute = nameof(CanViewConsole))]
    private async Task ViewConsole(V1Container container)
    {
        var vm = Application.Current.GetRequiredService<PodConsoleViewModel>();
        vm.Cluster = Cluster;
        vm.Object = ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Value;
        vm.ContainerName = container.Name;
        vm.Id = $"{nameof(ViewConsole)}-{Cluster.Name}-{((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key}-{container.Name}";

        if (Factory.AddToBottom(vm))
        {
            try
            {
                await vm.Connect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to console");
                return;
            }
        }
    }

    private bool CanViewConsole(V1Container? container)
    {
        return container != null && Cluster.CanI<V1Pod>(Verb.Create, ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Namespace, "exec");
    }

    [RelayCommand(CanExecute = nameof(CanPortForward))]
    private async Task PortForward(V1ContainerPort containerPort)
    {
        var pf = Cluster.AddPodPortForward(((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Namespace, ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Name, containerPort.ContainerPort);

        ContentDialogSettings settings = new()
        {
            Title = Resources.ResourceListViewModel_PortForward_Title,
            Content = string.Format(Resources.ResourceListViewModel_PortForward_Content, containerPort.ContainerPort, pf.LocalPort),
            PrimaryButtonText = Resources.ResourceListViewModel_PortForward_Primary,
            SecondaryButtonText = Resources.ResourceListViewModel_PortForward_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
            await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
        }
    }

    private bool CanPortForward(V1ContainerPort? containerPort)
    {
        return containerPort?.ContainerPort > 0 &&
               containerPort.Protocol == "TCP" &&
               Cluster.CanI<V1Pod>(Verb.Create, ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Namespace, "portforward");
    }

    [RelayCommand(CanExecute = nameof(CanPortForwardService))]
    private async Task PortForwardService(V1ServicePort containerPort)
    {
        var pf = Cluster.AddServicePortForward(((KeyValuePair<NamespacedName, V1Service>)SelectedItem).Key.Namespace, ((KeyValuePair<NamespacedName, V1Service>)SelectedItem).Key.Name, containerPort.Port);

        ContentDialogSettings settings = new()
        {
            Title = Resources.ResourceListViewModel_PortForward_Title,
            Content = string.Format(Resources.ResourceListViewModel_PortForward_Content, containerPort.Port, pf.LocalPort),
            PrimaryButtonText = Resources.ResourceListViewModel_PortForward_Primary,
            SecondaryButtonText = Resources.ResourceListViewModel_PortForward_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
            await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
        }
    }

    private bool CanPortForwardService(V1ServicePort? servicePort)
    {
        var @namespace = ((KeyValuePair<NamespacedName, V1Service>)SelectedItem).Key.Namespace;

        return servicePort?.Port > 0 &&
               servicePort.Protocol == "TCP" &&
               Cluster.CanI<V1Pod>(Verb.Create, @namespace, "portforward") &&
               Cluster.CanI<V1Endpoints>(Verb.List, @namespace) &&
               Cluster.CanI<V1Endpoints>(Verb.Watch, @namespace);
    }

    [RelayCommand(CanExecute = nameof(CanListCRD))]
    private void ListCRD(V1CustomResourceDefinition item)
    {
        var version = item.Spec.Versions.First(x => x.Served && x.Storage);

        var type = Cluster.ModelCache.GetResourceType(item.Spec.Group, version.Name, item.Spec.Names.Kind);
        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(type);

        var vm = Application.Current.GetRequiredService(resourceListType) as IDockable;

        if (vm is IInitializeCluster init)
        {
            init.Initialize(Cluster);
        }

        Factory.AddToDocuments(vm);
    }

    private bool CanListCRD(V1CustomResourceDefinition? item)
    {
        if (item == null)
        {
            return false;
        }

        var version = item.Spec.Versions.First(x => x.Served && x.Storage);
        var type = Cluster.ModelCache.GetResourceType(item.Spec.Group, version.Name, item.Spec.Names.Kind);

        return Cluster.CanIAnyNamespace(type, Verb.List) && Cluster.CanIAnyNamespace(type, Verb.Watch);
    }

    private static readonly string s_restartControllerPatch = $$"""
    {
        "spec": {
            "template": {
                "metadata": {
                    "annotations": {
                        "kubectl.kubernetes.io/restartedAt": "{{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}}"
                    }
                }
            }
        }
    }
    """;

    [RelayCommand(CanExecute = nameof(CanRestartDeployment))]
    private async Task RestartDeployment(V1Deployment deployment)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Resources.ResourceListViewModel_Restart_Content, deployment.Name()),
                PrimaryButtonText = Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedDeploymentAsync(new V1Patch(s_restartControllerPatch, V1Patch.PatchType.MergePatch), deployment.Metadata.Name, deployment.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting Deployment", sendNotification: true);
        }
    }

    private bool CanRestartDeployment(V1Deployment deployment)
    {
        return deployment != null && Cluster.CanI<V1Deployment>(Verb.Patch, deployment.Namespace());
    }

    [RelayCommand(CanExecute = nameof(CanRestartReplicaSet))]
    private async Task RestartReplicaSet(V1ReplicaSet replicaSet)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Resources.ResourceListViewModel_Restart_Content, replicaSet.Name()),
                PrimaryButtonText = Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedReplicaSetAsync(new V1Patch(s_restartControllerPatch, V1Patch.PatchType.MergePatch), replicaSet.Metadata.Name, replicaSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting ReplicaSet", sendNotification: true);
        }
    }

    private bool CanRestartReplicaSet(V1ReplicaSet replicaSet)
    {
        return replicaSet != null && Cluster.CanI<V1ReplicaSet>(Verb.Patch, replicaSet.Namespace());
    }

    [RelayCommand(CanExecute = nameof(CanRestartStatefulSet))]
    private async Task RestartStatefulSet(V1StatefulSet statefulSet)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Resources.ResourceListViewModel_Restart_Content, statefulSet.Name()),
                PrimaryButtonText = Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedStatefulSetAsync(new V1Patch(s_restartControllerPatch, V1Patch.PatchType.MergePatch), statefulSet.Metadata.Name, statefulSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting StatefulSet", sendNotification: true);
        }
    }

    private bool CanRestartStatefulSet(V1StatefulSet statefulSet)
    {
        return statefulSet != null && Cluster.CanI<V1StatefulSet>(Verb.Patch, statefulSet.Namespace());
    }

    [RelayCommand(CanExecute = nameof(CanRestartDaemonSet))]
    private async Task RestartDaemonSet(V1DaemonSet daemonSet)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Resources.ResourceListViewModel_Restart_Content, daemonSet.Name()),
                PrimaryButtonText = Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedDaemonSetAsync(new V1Patch(s_restartControllerPatch, V1Patch.PatchType.MergePatch), daemonSet.Metadata.Name, daemonSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting DaemonSet", sendNotification: true);
        }
    }

    private bool CanRestartDaemonSet(V1DaemonSet daemonSet)
    {
        return daemonSet != null && Cluster.CanI<V1DaemonSet>(Verb.Patch, daemonSet.Namespace());
    }

    [RelayCommand(CanExecute = nameof(CanCordonNode))]
    private async Task CordonNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Resources.ResourceListViewModel_CordonNode_Title,
            Content = string.Format(Resources.ResourceListViewModel_CordonNode_Content, items.Count),
            PrimaryButtonText = Resources.ResourceListViewModel_CordonNode_Primary,
            SecondaryButtonText = Resources.ResourceListViewModel_CordonNode_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var patch = $$"""
        {
            "spec": {
                "unschedulable": true
            }
        }
        """;

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, V1Node>>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Key.Name, item.Key.Namespace);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error Cordoning Node", sendNotification: true);
                }
            }
        }
    }

    private bool CanCordonNode(IList items)
    {
        return Cluster.CanI<V1Node>(Verb.Patch);
    }

    [RelayCommand(CanExecute = nameof(CanUnCordonNode))]
    private async Task UnCordonNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Resources.ResourceListViewModel_UnCordonNode_Title,
            Content = string.Format(Resources.ResourceListViewModel_UnCordonNode_Content, items.Count),
            PrimaryButtonText = Resources.ResourceListViewModel_UnCordonNode_Primary,
            SecondaryButtonText = Resources.ResourceListViewModel_UnCordonNode_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var patch = $$"""
        {
            "spec": {
                "unschedulable": false
            }
        }
        """;

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, V1Node>>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Key.Name, item.Key.Namespace);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error UnCordoning Node", sendNotification: true);
                }
            }
        }
    }

    private bool CanUnCordonNode(IList items)
    {
        return Cluster.CanI<V1Node>(Verb.Patch);
    }

    [RelayCommand(CanExecute = nameof(CanCordonNode))]
    private async Task DrainNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Resources.ResourceListViewModel_DrainNode_Title,
            Content = string.Format(Resources.ResourceListViewModel_DrainNode_Content, items.Count),
            PrimaryButtonText = Resources.ResourceListViewModel_DrainNode_Primary,
            SecondaryButtonText = Resources.ResourceListViewModel_DrainNode_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var patch = $$"""
        {
            "spec": {
                "unschedulable": true
            }
        }
        """;

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, V1Node>>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Key.Name, item.Key.Namespace);

                    var pods = await Cluster.GetObjectDictionaryAsync<V1Pod>();

                    foreach (var pod in pods)
                    {
                        if (pod.Value.Spec.NodeName == item.Value.Metadata.Name)
                        {
                            if (pod.Value.Metadata.OwnerReferences.Any(x => x.ApiVersion == V1DaemonSet.KubeGroup + "/" + V1DaemonSet.KubeApiVersion &&
                                                                            x.Kind == V1DaemonSet.KubeKind &&
                                                                            x.Controller == true &&
                                                                            x.BlockOwnerDeletion == true))
                            {
                                continue;
                            }

                            V1Eviction evict = new()
                            {
                                ApiVersion = V1Eviction.KubeGroup + "/" + V1Eviction.KubeApiVersion,
                                Kind = V1Eviction.KubeKind,
                                Metadata = new()
                                {
                                    Name = pod.Value.Metadata.Name,
                                    NamespaceProperty = pod.Value.Metadata.NamespaceProperty
                                }
                            };

                            try
                            {
                                await Cluster.Client.CoreV1.CreateNamespacedPodEvictionAsync(evict, pod.Value.Metadata.Name, pod.Value.Metadata.NamespaceProperty);
                            }
                            catch (Exception ex)
                            {
                                Utilities.HandleException(_logger, _notificationManager, ex, "Error Evicting Pod", sendNotification: true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error Draining Node", sendNotification: true);
                }
            }
        }
    }

    private bool CanDrainNode(IList items)
    {
        return Cluster.CanI<V1Node>(Verb.Patch);
    }

    #endregion

    public void Dispose()
    {
        _filter?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    private ResourceListViewDefinitionColumn<T, string> NameColumn(SortDirection sort = SortDirection.None)
    {
        return new ResourceListViewDefinitionColumn<T, string>()
        {
            Name = "Name",
            Field = x => x.Metadata.Name,
            Width = "2*",
            Sort = sort,
        };
    }

    private ResourceListViewDefinitionColumn<T, string> NamespaceColumn()
    {
        return new ResourceListViewDefinitionColumn<T, string>()
        {
            Name = "Namespace",
            Field = x => x.Metadata.NamespaceProperty,
            Width = "*",
        };
    }

    private ResourceListViewDefinitionColumn<T, DateTime?> AgeColumn()
    {
        return new ResourceListViewDefinitionColumn<T, DateTime?>()
        {
            Name = "Age",
            CustomControl = typeof(AgeCell),
            Field = x => x.Metadata.CreationTimestamp,
            Display = x => x.Metadata.CreationTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            Width = "80"
        };
    }
}

public interface IResourceListViewDefinitionColumn
{
    string Name { get; set; }

    public SortDirection Sort { get; set; }

    public Type? CustomControl { get; set; }

    public string? Width { get; set; }
}

public class ResourceListViewDefinitionColumn<T, T2> : IResourceListViewDefinitionColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public required string Name { get; set; }

    public required Func<T, T2> Field { get; set; }

    public Func<T, string>? Display { get; set; }

    public SortDirection Sort { get; set; }

    public Type? CustomControl { get; set; }

    public string? Width { get; set; }
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}

public class ResourceListViewMenuItem
{
    public string? Header { get; set; }

    public IBinding? HeaderBinding { get; set; }

    public string? CommandPath { get; set; }

    public string? CommandParameterPath { get; set; }

    public string? ItemSourcePath { get; set; }

    public IBinding? ItemSourceBinding { get; set; }

    public string? IconResource { get; set; }

    public ResourceListViewMenuItem? ItemTemplate { get; set; }

    public IList<ResourceListViewMenuItem> MenuItems { get; set; }
}

public class ResourceListViewDefinition<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public IList<IResourceListViewDefinitionColumn> Columns { get; set; }

    public IList<ResourceListViewMenuItem> MenuItems { get; set; }

    public bool DefaultMenuItems { get; set; } = true;

    public bool ShowNamespaces { get; set; } = true;

    public bool ShowNewResource { get; set; } = true;

    public Func<StyleGroup>? SetStyle { get; set; } = () => [];
}
