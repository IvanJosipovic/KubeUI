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
using KubeUI.Resources;
using OpenTelemetry.Trace;
using Swordfish.NET.Collections;
using static KubeUI.Client.Cluster;
using SortDirection = KubeUI.Resources.SortDirection;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial GroupApiVersionKind Kind { get; set; }

    [ObservableProperty]
    public partial ConcurrentObservableDictionary<NamespacedName, T> Objects { get; set; }

    [ObservableProperty]
    public partial KeyValuePair<NamespacedName, T> SelectedItem { get; set; }

    [ObservableProperty]
    public partial ReadOnlyObservableCollection<KeyValuePair<NamespacedName, T>> DataGridObjects { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

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
        ResourceConfig = (ResourceConfigBase<T>)Cluster.GetResourceConfig(Kind);

        Objects = Cluster.GetObjectDictionary<T>();

        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

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

            if (Cluster.SelectedNamespaces != null && ResourceConfig.ShowNamespaces)
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

                    foreach (var column in ResourceConfig.Columns())
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

    //private ResourceListViewDefinition<T> GetViewDefinition()
    //{
    //    var resourceType = typeof(T);

    //    ResourceListViewDefinition<T> definition = new();

    //    else if (resourceType == typeof(V1ConfigMap))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1ConfigMap, string>()
    //            {
    //                Name = "Keys",
    //                Display = x => x.Data != null && x.Data.Keys.Count > 0 ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
    //                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1Secret))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1Secret, string>()
    //            {
    //                Name = "Labels",
    //                Display = x => x.Metadata?.Labels != null ? x.Metadata.Labels.Keys.Aggregate((a,b) => a + ", " + b) : "",
    //                Field = x => x.Metadata?.Labels?.Keys.FirstOrDefault() ?? "",
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V1Secret, string>()
    //            {
    //                Name = "Keys",
    //                Display = x => x.Data != null ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
    //                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V1Secret, string>()
    //            {
    //                Name = "Type",
    //                Field = x => x.Type,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V2HorizontalPodAutoscaler))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, int>()
    //            {
    //                Name = "Min Pods",
    //                Display = x => (x.Spec.MinReplicas ?? 0).ToString(),
    //                Field = x => x.Spec.MinReplicas ?? 0,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, int>()
    //            {
    //                Name = "Max Pods",
    //                Display = x => x.Spec.MaxReplicas.ToString(),
    //                Field = x => x.Spec.MaxReplicas,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, int>()
    //            {
    //                Name = "Replica",
    //                Display = x => (x.Status.CurrentReplicas ?? 0).ToString(),
    //                Field = x => x.Status.CurrentReplicas ?? 0,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //            new ResourceListViewDefinitionColumn<V2HorizontalPodAutoscaler, string>()
    //            {
    //                Name = "Conditions",
    //                Display = x => x.Status.Conditions.FirstOrDefault(y => y.Status == "True").Type,
    //                Field = x => x.Status.Conditions.First(y => y.Status == "True").Type,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //        ];
    //    }
    //    else if (resourceType == typeof(V1PodDisruptionBudget))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, IntstrIntOrString>()
    //            {
    //                Name = "Min Avalilable",
    //                Display = x => x.Spec.MinAvailable != null ? x.Spec.MinAvailable.Value : "",
    //                Field = x => x.Spec.MinAvailable,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, IntstrIntOrString>()
    //            {
    //                Name = "Max Unavailable",
    //                Display = x => x.Spec.MaxUnavailable != null ? x.Spec.MaxUnavailable.Value : "",
    //                Field = x => x.Spec.MaxUnavailable,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, int>()
    //            {
    //                Name = "Current Healthy",
    //                Display = x => x.Status.CurrentHealthy.ToString(),
    //                Field = x => x.Status.CurrentHealthy,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, int>()
    //            {
    //                Name = "Desired Healthy",
    //                Display = x => x.Status.DesiredHealthy.ToString(),
    //                Field = x => x.Status.DesiredHealthy,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1PriorityClass))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1PriorityClass, int>()
    //            {
    //                Name = "Value",
    //                Display = x => x.Value.ToString(),
    //                Field = x => x.Value,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            new ResourceListViewDefinitionColumn<V1PriorityClass, bool?>()
    //            {
    //                Name = "Global Default",
    //                Display = x => (x.GlobalDefault ?? false).ToString(),
    //                Field = x => x.GlobalDefault ?? false,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1RuntimeClass))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1RuntimeClass, string>()
    //            {
    //                Name = "Handler",
    //                Display = x => x.Handler,
    //                Field = x => x.Handler,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1Lease))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1Lease, string>()
    //            {
    //                Name = "Holder",
    //                Display = x => x.Spec.HolderIdentity ?? "",
    //                Field = x => x.Spec.HolderIdentity ?? "",
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1MutatingWebhookConfiguration))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1MutatingWebhookConfiguration, int>()
    //            {
    //                Name = "Webhooks",
    //                Display = x => x.Webhooks?.Count.ToString() ?? "",
    //                Field = x => x.Webhooks?.Count ?? 0,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1ValidatingWebhookConfiguration))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1ValidatingWebhookConfiguration, int>()
    //            {
    //                Name = "Webhooks",
    //                Display = x => x.Webhooks.Count.ToString(),
    //                Field = x => x.Webhooks.Count,
    //                Width = nameof(DataGridLengthUnitType.SizeToHeader)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1Service))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1Service, string>()
    //            {
    //                Name = "Type",
    //                Display = x => x.Spec.Type,
    //                Field = x => x.Spec.Type,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            new ResourceListViewDefinitionColumn<V1Service, string>()
    //            {
    //                Name = "Cluster IP",
    //                Display = x => x.Spec.ClusterIP,
    //                Field = x => x.Spec.ClusterIP,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            new ResourceListViewDefinitionColumn<V1Service, int>()
    //            {
    //                Name = "Ports",
    //                Display = x => x.Spec.Ports.Select((a) => $"{a.Port}{(string.IsNullOrEmpty(a.Name) ? "" : ":" + a.Name)}/{a.Protocol}").Aggregate((a,b) => a + ", " + b),
    //                Field = x => x.Spec.Ports.FirstOrDefault().Port,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];

    //        definition.MenuItems =
    //        [
    //            new()
    //            {
    //                Header = "Port Forwarding",
    //                ItemSourcePath = "SelectedItem.Value.Spec.Ports",
    //                IconResource = "ic_fluent_cloud_flow_filled",
    //                ItemTemplate = new()
    //                {
    //                    HeaderBinding = new MultiBinding()
    //                    {
    //                        Bindings =
    //                        [
    //                            new Binding(nameof(V1ServicePort.Name)),
    //                            new Binding(nameof(V1ServicePort.Port))
    //                        ],
    //                        StringFormat = "{0} - {1}"
    //                    },
    //                    //CommandPath = nameof(ResourceListViewModel<V1Pod>.PortForwardServiceCommand), //todo fix
    //                    CommandParameterPath = ".",
    //                }
    //            }
    //        ];
    //    }
    //    else if (resourceType == typeof(V1Endpoints))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1Endpoints, int>()
    //            {
    //                Name = "Endpoints",
    //                Display = x => x.Subsets != null ? x.Subsets.SelectMany(y => y.Ports.Select(z => y.Addresses[0].Ip + ":" + z.Port)).Aggregate((a,b) => a + ", " + b) : "",
    //                Field = x => x.Subsets != null ? x.Subsets[0].Ports[0].Port : 0,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1EndpointSlice))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1Ingress))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1Ingress, string>()
    //            {
    //                Name = "Load Balancers",
    //                Display = x => x.Status.LoadBalancer.Ingress.Select(x => x.Ip).Aggregate((a,b) => a + ", " + b),
    //                Field = x => x.Status.LoadBalancer.Ingress.Count > 0 ? x.Status.LoadBalancer.Ingress[0].Ip : "",
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1Ingress, string>()
    //            {
    //                Name = "Rules",
    //                Display = x => x.Spec.Rules.Select(z => $"http://{z.Host}{z.Http.Paths[0].Path}").Aggregate((a,b) => a + ", " + b),
    //                Field = x => x.Spec.Rules.Count > 0 ? x.Spec.Rules[0].Host : "",
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1IngressClass))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
    //            {
    //                Name = "Controller",
    //                Field = x => x.Spec.Controller,
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
    //            {
    //                Name = "API Group",
    //                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.ApiGroup : "",
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
    //            {
    //                Name = "Scope",
    //                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Scope : "",
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
    //            {
    //                Name = "Kind",
    //                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Kind : "",
    //                Width = "*",
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1NetworkPolicy))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1NetworkPolicy, string>()
    //            {
    //                Name = "Policy Types",
    //                Display = x => x.Spec.PolicyTypes.Aggregate((a,b) => a + ", " + b),
    //                Field = x => x.Spec.PolicyTypes.Count > 0 ? x.Spec.PolicyTypes[0] : "",
    //                Width = "*",
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1PersistentVolumeClaim))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
    //            {
    //                Name = "Storage Class",
    //                Field = x => x.Spec.StorageClassName,
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
    //            {
    //                Name = "Size",
    //                Field = x => x.Spec.Resources.Requests["storage"].Value,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //            new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
    //            {
    //                Name = "Status",
    //                Field = x => x.Status.Phase,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //        ];
    //    }
    //    else if (resourceType == typeof(V1PersistentVolume))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
    //            {
    //                Name = "Storage Class",
    //                Field = x => x.Spec.StorageClassName,
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
    //            {
    //                Name = "Size",
    //                Field = x => x.Spec.Capacity["storage"].Value,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
    //            {
    //                Name = "Claim",
    //                Field = x => x.Spec.ClaimRef.Name,
    //                Width = "*",
    //            },
    //            AgeColumn(),
    //            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
    //            {
    //                Name = "Status",
    //                Field = x => x.Status.Phase,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //        ];
    //    }
    //    else if (resourceType == typeof(V1StorageClass))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1StorageClass, string>()
    //            {
    //                Name = "Provisioner",
    //                Field = x => x.Provisioner,
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1StorageClass, string>()
    //            {
    //                Name = "Reclaim Policy",
    //                Field = x => x.ReclaimPolicy,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            new ResourceListViewDefinitionColumn<V1StorageClass, string>()
    //            {
    //                Name = "Default", // "storageclass.kubernetes.io/is-default-class":"true"
    //                Field = x => x.Metadata.Annotations?.ContainsKey("storageclass.kubernetes.io/is-default-class") == true ?
    //                                x.Metadata.Annotations["storageclass.kubernetes.io/is-default-class"] : "false",
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1ServiceAccount))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1ClusterRole))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1Role))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            AgeColumn(),
    //        ];
    //    }
    //    else if (resourceType == typeof(V1ClusterRoleBinding))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            new ResourceListViewDefinitionColumn<V1ClusterRoleBinding, string>()
    //            {
    //                Name = "Bindings",
    //                Field = x => x.Subjects == null || x.Subjects.Count == 0 ? "" : x.Subjects.Select(y => y.Name).Aggregate((a,b) => a + ", " + b),
    //                Width = "*",
    //            },
    //            AgeColumn()
    //        ];
    //    }
    //    else if (resourceType == typeof(V1RoleBinding))
    //    {
    //        definition.Columns =
    //        [
    //            NameColumn(SortDirection.Ascending),
    //            NamespaceColumn(),
    //            new ResourceListViewDefinitionColumn<V1RoleBinding, string>()
    //            {
    //                Name = "Bindings",
    //                Field = x => x.Subjects.Select(y => y.Name).Aggregate((a,b) => a + ", " + b),
    //                Width = "*",
    //            },
    //            AgeColumn()
    //        ];
    //    }
    //    else if (resourceType == typeof(V1CustomResourceDefinition))
    //    {
    //        definition.ShowNamespaces = false;

    //        definition.Columns =
    //        [
    //            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
    //            {
    //                Name = "Name",
    //                Display = x => x.Spec.Names.Kind.Humanize(LetterCasing.Title),
    //                Field = x => x.Spec.Names.Kind,
    //                Sort = SortDirection.Ascending,
    //                Width = "2*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
    //            {
    //                Name = "Group",
    //                Field = x => x.Spec.Group,
    //                Width = "*",
    //            },
    //            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
    //            {
    //                Name = "Version",
    //                Field = x => x.Spec.Versions.First(x => x.Storage).Name,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
    //            {
    //                Name = "Scope",
    //                Field = x => x.Spec.Scope,
    //                Width = nameof(DataGridLengthUnitType.SizeToCells)
    //            },
    //            AgeColumn(),
    //        ];

    //        definition.MenuItems =
    //        [
    //            new()
    //            {
    //                Header = "View Items",
    //                CommandParameterPath = "SelectedItem.Value",
    //                CommandPath = nameof(ResourceListViewModel<V1Pod>.ListCRDCommand)
    //            },
    //        ];

    //    }
    //    else
    //    {
    //        definition.Columns = [];

    //        // Add Name Column
    //        var nameColumn = NameColumn(SortDirection.Ascending);

    //        definition.Columns.Add(nameColumn);

    //        // Check if resource type is CRD
    //        // TODO: Find a better way to identify CRDs
    //        if (resourceType.Namespace.StartsWith("KubeUI.Models"))
    //        {
    //            // Generate CRD Columns
    //            var crd = Cluster.GetObject<V1CustomResourceDefinition>(null, Kind.PluralNameGroup);

    //            var version = crd.Spec.Versions.First(x => x.Storage);

    //            //Check if its a namespaced crd
    //            if (crd.Spec.Scope == "Namespaced")
    //            {
    //                // Add Namespace Column
    //                var nsColumn = NamespaceColumn();

    //                definition.Columns.Add(nsColumn);
    //            }
    //            else
    //            {
    //                definition.ShowNamespaces = false;
    //            }

    //            if (version.AdditionalPrinterColumns != null)
    //            {
    //                foreach (var item in version.AdditionalPrinterColumns)
    //                {
    //                start:

    //                    try
    //                    {
    //                        if (item.JsonPath == ".metadata.creationTimestamp")
    //                        {
    //                            continue;
    //                        }

    //                        if (item.Type == "string")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, string>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, string>()
    //                            {
    //                                Name = item.Name,
    //                                Field = exp.Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else if (item.Type == "number")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, double>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, double>()
    //                            {
    //                                Name = item.Name,
    //                                Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
    //                                Field = exp.Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else if (item.Type == "integer" && item.Format == "int64")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, long>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, long>()
    //                            {
    //                                Name = item.Name,
    //                                Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
    //                                Field = exp.Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else if (item.Type == "integer")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, int>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, int>()
    //                            {
    //                                Name = item.Name,
    //                                Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
    //                                Field = exp.Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else if (item.Type == "date")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, DateTime>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, DateTime>()
    //                            {
    //                                Name = item.Name,
    //                                Field = exp.Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else if (item.Type == "boolean")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, bool>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, bool>()
    //                            {
    //                                Name = item.Name,
    //                                Display = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
    //                                Field = exp.Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else if (item.Type == "enum")
    //                        {
    //                            var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, Enum>(item.JsonPath, true);

    //                            var colDef = new ResourceListViewDefinitionColumn<T, string>()
    //                            {
    //                                Name = item.Name,
    //                                Field = TransformToFuncOfString(exp.Body, exp.Parameters).Compile(),
    //                                //Width = "*"
    //                            };

    //                            definition.Columns.Add(colDef);
    //                        }
    //                        else
    //                        {
    //                            _logger.LogWarning("CRD Column Type not supported: {type}", item.Type);
    //                        }
    //                    }
    //                    catch (InvalidOperationException ex) when (ex.Message.StartsWith("No coercion operator is defined between types", StringComparison.Ordinal))
    //                    {
    //                        // The type defined in the AdditionalPrinterColumn is not correct
    //                        var match = TypeErrorRegex().Match(ex.Message);
    //                        if (match.Success)
    //                        {
    //                            var typeString = match.Groups[1].Value;

    //                            if (typeString.StartsWith("System.Nullable`1[", StringComparison.Ordinal))
    //                            {
    //                                typeString = typeString["System.Nullable`1[".Length..].TrimEnd(']');
    //                            }

    //                            var type = Type.GetType(typeString);

    //                            if (type == null)
    //                            {
    //                                type = resourceType.Assembly.GetType(typeString);

    //                                if (type == null)
    //                                {
    //                                    _logger.LogError(ex, "Unable to load type for column: {Name} in type {type}", typeString, crd.Name());
    //                                    continue;
    //                                }
    //                            }

    //                            if (type.IsGenericType)
    //                            {
    //                                type = type.GenericTypeArguments[0];
    //                            }

    //                            if (type == typeof(string))
    //                            {
    //                                item.Type = "string";
    //                            }
    //                            else if (type == typeof(double))
    //                            {
    //                                item.Type = "number";
    //                            }
    //                            else if (type == typeof(int))
    //                            {
    //                                item.Type = "integer";
    //                            }
    //                            else if (type == typeof(long))
    //                            {
    //                                item.Type = "integer";
    //                                item.Format = "int64";
    //                            }
    //                            else if (type == typeof(DateTime))
    //                            {
    //                                item.Type = "date";
    //                            }
    //                            else if (type == typeof(bool))
    //                            {
    //                                item.Type = "boolean";
    //                            }
    //                            else if (type.IsEnum)
    //                            {
    //                                item.Type = "enum";
    //                            }
    //                            else
    //                            {
    //                                _logger.LogError(ex, "Unable to generate CRD Column: {Name} with type {Type}", item.Name, type);
    //                                continue;
    //                            }

    //                            goto start;
    //                        }
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        _logger.LogCritical(ex, "Unable to generate CRD Column: {Name} in {crd}", item.Name, crd.Name());
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            _logger.LogError("Unexpected CRD namespace {res}", resourceType.Namespace);
    //        }

    //        // Add Age Column
    //        var ageColumn = new ResourceListViewDefinitionColumn<T, DateTime?>()
    //        {
    //            Name = "Age",
    //            CustomControl = typeof(AgeCell),
    //            Field = x => x.Metadata.CreationTimestamp,
    //            Display = x => x.Metadata.CreationTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
    //            Width = "80"
    //        };

    //        definition.Columns.Add(ageColumn);
    //    }

    //    return definition;
    //}

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
        resource.ApiVersion = Kind.GroupApiVersion;
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
            Title = Assets.Resources.ResourceListViewModel_Delete_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_Delete_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_Delete_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_Delete_Secondary,
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
        //if (items.Count == 0)
        //{
        //    return false;
        //}

        //foreach (var item in items)
        //{
        //    var ns = (NamespacedName)item.GetType().GetProperty("Key")!.GetValue(item);

        //    if (!Cluster.CanI<T>(Verb.Delete, ns.Namespace))
        //    {
        //        return false;
        //    }
        //}

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    private void View(IList parameters)
    {
        var item = parameters[1];

        var instance = Application.Current.GetRequiredService<ResourcePropertiesViewModel<T>>();
        instance.Initialize(Cluster, ((KeyValuePair<NamespacedName, T>)(item)).Value);
        instance.CanFloat = false;

        Factory?.AddToRight(instance);
    }

    private bool CanView(IList parameters)
    {
        return parameters != null;
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
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, deployment.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
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
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, replicaSet.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
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
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, statefulSet.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
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
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, daemonSet.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
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
            Title = Assets.Resources.ResourceListViewModel_CordonNode_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_CordonNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_CordonNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_CordonNode_Secondary,
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
            Title = Assets.Resources.ResourceListViewModel_UnCordonNode_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_UnCordonNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_UnCordonNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_UnCordonNode_Secondary,
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
            Title = Assets.Resources.ResourceListViewModel_DrainNode_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_DrainNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_DrainNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_DrainNode_Secondary,
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
