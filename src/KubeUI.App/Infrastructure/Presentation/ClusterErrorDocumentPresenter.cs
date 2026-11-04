using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.App.Infrastructure.Docking;
using KubeUI.Features.Clusters.Error;

namespace KubeUI.App.Infrastructure.Presentation;

/// <summary>Updates or opens the shared cluster-error document.</summary>
internal interface IClusterErrorDocumentPresenter
{
    void Show(string error);
}

/// <summary>Updates or opens the shared cluster-error document through the docking factory.</summary>
internal sealed class ClusterErrorDocumentPresenter(
    Func<IFactory> getFactory,
    Func<ClusterErrorViewModel> createViewModel) : IClusterErrorDocumentPresenter
{
    private const string DocumentId = "cluster-error";
    private const string DocumentDockId = "Documents";

    public void Show(string error)
    {
        var factory = getFactory();
        var documents = factory.GetDockable<IDocumentDock>(DocumentDockId);
        if (documents is null)
        {
            return;
        }

        var existing = documents.VisibleDockables?
            .OfType<ClusterErrorViewModel>()
            .FirstOrDefault(viewModel => viewModel.Id == DocumentId);
        if (existing is not null)
        {
            existing.Error = error;
            Activate(documents, existing);
            return;
        }

        var viewModel = createViewModel();
        viewModel.Id = DocumentId;
        viewModel.Error = error;
        factory.AddDockable(documents, viewModel);
        Activate(documents, viewModel);
    }

    private void Activate(IDocumentDock documents, IDockable document)
    {
        var factory = getFactory();
        factory.SetActiveDockable(document);
        factory.SetFocusedDockable(documents, document);
    }
}
