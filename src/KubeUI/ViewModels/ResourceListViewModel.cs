using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Styling;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using Yarp.Kubernetes.Controller;
using Yarp.Kubernetes.Controller.Client;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial GroupApiVersionKind Kind { get; set; }

    [ObservableProperty]
    public partial AvaloniaDictionary<NamespacedName, T> Objects { get; set; }

    [ObservableProperty]
    public partial KeyValuePair<NamespacedName, T> SelectedItem { get; set; }

    [ObservableProperty]
    public partial ReadOnlyObservableCollection<KeyValuePair<NamespacedName, T>>? DataGridObjects { get; private set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    [ObservableProperty]
    public partial string SortColumnName { get; set; }

    [ObservableProperty]
    public partial Resources.SortDirection SortDirection { get; set; }

    private IDisposable? _filter;

    public ResourceListViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListViewModel<T>>>();
        SettingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Kind = GroupApiVersionKind.From<T>();
        Title = Kind.Kind.Humanize(LetterCasing.Title).Pluralize();
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
            .ToObservableChangeSet<AvaloniaDictionary<NamespacedName, T>, KeyValuePair<NamespacedName, T>>()
            .Filter(GenerateFilter())
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out var filteredObjects)
            .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Setting Resource List Filter: {ns} ", typeof(T)));

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

            if (Cluster.SelectedNamespaces != null && ResourceConfig.IsNamespaced)
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

                        var columnDisplay = (Func<T, string>)colType.GetProperty(nameof(ResourceListColumn<,>.Display)).GetValue(column);

                        columnDisplay ??= (Func<T, string>)colType.GetProperty(nameof(ResourceListColumn<,>.Field)).GetValue(column);

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

    public void Dispose()
    {
        _filter?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }
}

/// <summary>
/// A reactive scheduler that uses Avalonia's <see cref="Dispatcher"/>.
/// </summary>
public sealed class AvaloniaScheduler : LocalScheduler
{
    /// <summary>
    /// The instance of the <see cref="AvaloniaScheduler"/>.
    /// </summary>
    public static readonly AvaloniaScheduler Instance = new();

    /// <summary>
    /// Users can schedule actions on the dispatcher thread while being on the correct thread already.
    /// We are optimizing this case by invoking user callback immediately which can lead to stack overflows in certain cases.
    /// To prevent this we are limiting amount of reentrant calls to <see cref="Schedule{TState}"/> before we will
    /// schedule on a dispatcher anyway.
    /// </summary>
    private const int MaxReentrantSchedules = 32;

    private int _reentrancyGuard;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaScheduler"/> class.
    /// </summary>
    private AvaloniaScheduler()
    {
    }

    /// <inheritdoc/>
    public override IDisposable Schedule<TState>(
        TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        IDisposable PostOnDispatcher()
        {
            var composite = new CompositeDisposable(2);

            var cancellation = new CancellationDisposable();

            Dispatcher.UIThread.Post(
                                     () =>
                                     {
                                         if (!cancellation.Token.IsCancellationRequested)
                                         {
                                             composite.Add(action(this, state));
                                         }
                                     },
                                     DispatcherPriority.Background);

            composite.Add(cancellation);

            return composite;
        }

        if (dueTime == TimeSpan.Zero)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                return PostOnDispatcher();
            }

            if (_reentrancyGuard >= MaxReentrantSchedules)
            {
                return PostOnDispatcher();
            }

            try
            {
                _reentrancyGuard++;

                return action(this, state);
            }
            finally
            {
                _reentrancyGuard--;
            }
        }

        {
            var composite = new CompositeDisposable(2);

            composite.Add(DispatcherTimer.RunOnce(() => composite.Add(action(this, state)), dueTime));

            return composite;
        }
    }
}
