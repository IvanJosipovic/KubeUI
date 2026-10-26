using Avalonia.Headless.XUnit;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Avalonia.Features.Clusters.Catalog;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Avalonia.Tests.Features.Clusters;

public sealed class ClusterListViewModelTests
{
    [AvaloniaFact]
    public void Name_and_kubeconfig_columns_share_available_width()
    {
        var runtimeCatalog = new Mock<IClusterRuntimeCatalog>();
        runtimeCatalog.SetupGet(catalog => catalog.Clusters).Returns([]);
        using var workspaceCatalog = new ClusterWorkspaceCatalog(runtimeCatalog.Object, Mock.Of<IServiceProvider>());
        using var viewModel = new ClusterListViewModel(workspaceCatalog, runtimeCatalog.Object, Mock.Of<IDialogService>());

        var columns = viewModel.TableSource.Columns;

        var nameColumn = columns.Single(column => column.Key == "name");
        nameColumn.WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
        nameColumn.Width.ShouldBe(1);

        var kubeconfigColumn = columns.Single(column => column.Key == "kubeconfig");
        kubeconfigColumn.WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
        kubeconfigColumn.Width.ShouldBe(2);
    }
}
