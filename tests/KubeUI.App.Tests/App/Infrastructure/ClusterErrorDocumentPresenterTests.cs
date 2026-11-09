using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.Features.Clusters.Error;
using Moq;
using Shouldly;

namespace KubeUI.App.Tests.App.Infrastructure;

public sealed class ClusterErrorDocumentPresenterTests
{
    [Fact]
    public void Show_updates_and_activates_existing_cluster_error_document()
    {
        var existing = new ClusterErrorViewModel { Id = "cluster-error", Error = "old error" };
        var documents = new DocumentDock { VisibleDockables = [existing] };
        existing.Owner = documents;
        var factory = new Mock<IFactory>();
        factory.Setup(item => item.GetDockable<IDocumentDock>("Documents")).Returns(documents);
        var presenter = new ClusterErrorDocumentPresenter(() => factory.Object, () => new ClusterErrorViewModel());

        presenter.Show("new error");

        existing.Error.ShouldBe("new error");
        factory.Verify(item => item.SetActiveDockable(existing), Times.Once);
        factory.Verify(item => item.SetFocusedDockable(documents, existing), Times.Once);
        factory.Verify(item => item.AddDockable(It.IsAny<IDock>(), It.IsAny<IDockable>()), Times.Never);
        factory.Verify(item => item.InsertDockable(It.IsAny<IDock>(), It.IsAny<IDockable>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Show_updates_floated_cluster_error_document_instead_of_creating_a_duplicate()
    {
        var floated = new ClusterErrorViewModel { Id = "cluster-error", Error = "old error" };
        var windowDock = new DocumentDock();
        floated.Owner = windowDock;
        var documents = new DocumentDock();
        var factory = new Mock<IFactory>();
        factory.Setup(item => item.GetDockable<IDocumentDock>("Documents")).Returns(documents);
        factory
            .Setup(item => item.Find(It.IsAny<Func<IDockable, bool>>()))
            .Returns((Func<IDockable, bool> predicate) => predicate(floated)
                ? [floated]
                : Array.Empty<IDockable>());
        var presenter = new ClusterErrorDocumentPresenter(() => factory.Object, () => new ClusterErrorViewModel());

        presenter.Show("new error");

        floated.Error.ShouldBe("new error");
        factory.Verify(item => item.SetActiveDockable(floated), Times.Once);
        factory.Verify(item => item.SetFocusedDockable(windowDock, floated), Times.Once);
        factory.Verify(item => item.AddDockable(It.IsAny<IDock>(), It.IsAny<IDockable>()), Times.Never);
        factory.Verify(item => item.InsertDockable(It.IsAny<IDock>(), It.IsAny<IDockable>(), It.IsAny<int>()), Times.Never);
    }
}
