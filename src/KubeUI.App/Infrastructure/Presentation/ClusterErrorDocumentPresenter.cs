using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Controls.Docking;
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
        if (factory.GetDockable<IDocumentDock>(DocumentDockId) is null)
        {
            return;
        }

        var viewModel = createViewModel();
        viewModel.Id = DocumentId;
        factory.AddOrUpdateToDocuments(viewModel, document => document.Error = error);
    }
}
