using System.Collections.Specialized;
using System.Linq.Expressions;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
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
            Content = string.Format(Assets.Resources.ResourceListViewModel_Delete_Content, ((IList)items[1]).Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_Delete_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_Delete_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            var exceptions = new List<Exception>();

            foreach (var item in ((IList)items[1]).Cast<KeyValuePair<NamespacedName, T>>().ToList())
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

            if (exceptions.Count > 0)
            {
                _logger.LogError(new AggregateException(exceptions), "Error Deleting Resources");
            }
        }
    }

    private bool CanDelete(IList items)
    {
        //foreach (var item in items)
        //{
        //    var ns = (NamespacedName)item.GetType().GetProperty("Key")!.GetValue(item);

        //    if (!Cluster.CanI<T>(Verb.Delete, ns.Namespace))
        //    {
        //        return false;
        //    }
        //}

        return items?[1] is IList list && list.Count > 0;
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

    private bool CanView(IList? parameters)
    {
        return parameters?[1] != null;
    }

    [RelayCommand(CanExecute = nameof(CanViewYaml))]
    private void ViewYaml(IList parameters)
    {
        var item = parameters[1];

        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

        vm.Initialize(Cluster, ((KeyValuePair<NamespacedName, T>)item).Value);

        Factory.AddToBottom(vm);
    }

    private bool CanViewYaml(IList? parameters)
    {
        return parameters?[1] != null;
    }

    #endregion

    public void Dispose()
    {
        _filter?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }
}
