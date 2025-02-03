using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Avalonia.Controls.Notifications;
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
using KubeUI.Resources;
using Swordfish.NET.Collections;
using static KubeUI.Client.Cluster;

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
