using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using KubeUI.Features.Clusters.Catalog;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Catalog;

public sealed class ClusterListColumnsTests
{
    [AvaloniaFact]
    public void Name_and_kubeconfig_columns_share_available_width()
    {
        var columns = ClusterListViewModel.CreateColumns();

        var nameColumn = columns.Single(column => column.Key == "name");
        nameColumn.Width.ShouldBe(new GridLength(1, GridUnitType.Star));

        var kubeconfigColumn = columns.Single(column => column.Key == "kubeconfig");
        kubeconfigColumn.Width.ShouldBe(new GridLength(2, GridUnitType.Star));
    }
}
