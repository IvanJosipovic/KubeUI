using System.Reflection;
using Avalonia;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Features.Resources.List.Views;

public partial class ResourceListView : UserControl
{
    private DataGridColumnFilterFlyoutFactory? _filterFlyoutFactory;

    public ResourceListView()
    {
        InitializeComponent();

        DesignTimePreview.Run(InitializePreviewDataAsync);
    }

    private async Task InitializePreviewDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ResourceListViewModel<V1Pod>, V1Pod>();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IResourceListViewModel vm)
        {
            GetGenericMethod(nameof(GenerateGrid))?.Invoke(this, null);

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
        _filterFlyoutFactory ??= GetServiceProvider().GetRequiredService<DataGridColumnFilterFlyoutFactory>();

        foreach (var column in vm.ColumnDefinitions)
        {
            if (GetResourceListColumn(column) is not IResourceListColumn resourceColumn)
            {
                continue;
            }

            column.FilterFlyout = _filterFlyoutFactory.Create(resourceColumn, column, vm.FilteringModel);
        }
    }

    private static IServiceProvider GetServiceProvider()
    {
        if (Application.Current is IServiceProviderHost host)
        {
            return host.Services;
        }

        throw new InvalidOperationException("Unable to resolve services from the current application host.");
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


