using System.Reflection;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Views;

public partial class ResourceListView : UserControl
{
    private readonly DataGridColumnFilterFlyoutFactory _filterFlyoutFactory;
    private readonly ILogger<ResourceListView> _logger;

    public ResourceListView()
    {
        InitializeComponent();

        _filterFlyoutFactory = Application.Current.GetRequiredService<DataGridColumnFilterFlyoutFactory>();
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListView>>();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect();
                await cluster.SeedResource<V1Pod>();

                var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>() as IDockable;

                if (vm is IInitializeCluster init)
                {
                    init.Initialize(cluster);
                }

                DataContext = vm;
            });
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IResourceListViewModel vm)
        {
            GetGenericMethod(nameof(GenerateGrid))?.Invoke(this, null);

            PART_Grid.ReferenceIndexResolver = vm.ReferenceIndexResolver;
            PART_Grid.SortingAdapterFactory = vm.SortingAdapterFactory;
            PART_Grid.FilteringAdapterFactory = vm.FilteringAdapterFactory;
            PART_Grid.SearchAdapterFactory = vm.SearchAdapterFactory;
            PART_Grid.SortingModel = vm.SortingModel;
            PART_Grid.FilteringModel = vm.FilteringModel;
            PART_Grid.SearchModel = vm.SearchModel;
            PART_Grid.Selection = vm.SelectionModel;

            AttachFilterFlyouts(vm);
        }
    }

    private static IResourceListColumn? GetResourceListColumn(DataGridColumnDefinition columnDefinition)
    {
        return columnDefinition.Tag as IResourceListColumn;
    }

    private void AttachFilterFlyouts(IResourceListViewModel vm)
    {
        foreach (var column in vm.ColumnDefinitions)
        {
            if (GetResourceListColumn(column) is not IResourceListColumn resourceColumn)
            {
                continue;
            }

            column.FilterFlyout = _filterFlyoutFactory.Create(resourceColumn, column, vm.FilteringModel);
        }
    }

    private MethodInfo? GetGenericMethod(string name)
    {
        var dcType = DataContext?.GetType();

        if (dcType?.IsGenericType == true)
        {
            var genericArgs = dcType.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                var t = genericArgs[0];

                if (typeof(IKubernetesObject<V1ObjectMeta>).IsAssignableFrom(t))
                {
                    var method = GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (method?.IsGenericMethodDefinition == true)
                    {
                        var genericMethod = method.MakeGenericMethod(t);
                        return genericMethod;
                    }
                }
            }
        }

        return null;
    }

    private void GenerateGrid<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var viewModel = (ResourceListViewModel<T>)DataContext!;
        if (viewModel.ResourceConfig.ListStyle() != null)
        {
            PART_Grid.Styles.Add(viewModel.ResourceConfig.ListStyle());
        }
    }
}



