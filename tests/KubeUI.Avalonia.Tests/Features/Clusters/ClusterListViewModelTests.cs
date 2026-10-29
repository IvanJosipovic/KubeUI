using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Features.Clusters.Catalog;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Avalonia.Tests.Features.Clusters;

public sealed class ClusterListViewModelTests
{
    [AvaloniaFact]
    public void Name_and_kubeconfig_columns_share_available_width()
    {
        var columns = ClusterListViewModel.CreateColumns();

        var nameColumn = columns.Single(column => column.Key == "name");
        nameColumn.WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
        nameColumn.Width.ShouldBe(1);

        var kubeconfigColumn = columns.Single(column => column.Key == "kubeconfig");
        kubeconfigColumn.WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
        kubeconfigColumn.Width.ShouldBe(2);
    }
}
