using KubeUI.Avalonia.Features.Resources.Visualization;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Opens a Helm release preview as a visualization document.</summary>
public interface IHelmVisualizationDocumentOpener
{
    /// <summary>Opens the supplied visualization document.</summary>
    /// <param name="viewModel">Initialized preview view model.</param>
    void Open(VisualizationViewModel viewModel);
}
