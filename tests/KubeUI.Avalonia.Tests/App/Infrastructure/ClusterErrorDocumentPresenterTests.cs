using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.Features.Clusters.Error;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.App.Infrastructure;

public sealed class ClusterErrorDocumentPresenterTests
{
    [Fact]
    public void Show_updates_and_activates_existing_cluster_error_document()
    {
        var existing = new ClusterErrorViewModel { Id = "cluster-error", Error = "old error" };
        var documents = new DocumentDock { VisibleDockables = [existing] };
        var factory = new Mock<IFactory>();
        factory.Setup(item => item.GetDockable<IDocumentDock>("Documents")).Returns(documents);
        var createViewModelCalled = false;
        var presenter = new ClusterErrorDocumentPresenter(() => factory.Object, () =>
        {
            createViewModelCalled = true;
            return new ClusterErrorViewModel();
        });

        presenter.Show("new error");

        existing.Error.ShouldBe("new error");
        createViewModelCalled.ShouldBeFalse();
        factory.Verify(item => item.SetActiveDockable(existing), Times.Once);
        factory.Verify(item => item.SetFocusedDockable(documents, existing), Times.Once);
        factory.Verify(item => item.AddDockable(It.IsAny<IDock>(), It.IsAny<IDockable>()), Times.Never);
    }
}
