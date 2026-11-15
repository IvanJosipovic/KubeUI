using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using KubeUI.Features.Clusters.Catalog;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Catalog;

public sealed class ClusterListColumnsTests
{
    [AvaloniaFact]
    public void Name_and_alias_use_default_width_while_kubeconfig_uses_double_share()
    {
        var columns = ClusterListViewModel.CreateColumns();

        var nameColumn = columns.Single(column => column.Key == "name");
        nameColumn.Width.ShouldBe(GridLength.Auto);

        var aliasColumn = columns.Single(column => column.Key == "alias");
        aliasColumn.Width.ShouldBe(GridLength.Auto);

        var kubeconfigColumn = columns.Single(column => column.Key == "kubeconfig");
        kubeconfigColumn.Width.ShouldBe(new GridLength(2, GridUnitType.Star));
    }
}
