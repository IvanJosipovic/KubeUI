using Dock.Model.Core;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Infrastructure.Docking;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Opens Helm visualization documents through the application dock factory.</summary>
public sealed class HelmVisualizationDocumentOpener : IHelmVisualizationDocumentOpener
{
    private readonly IFactory _factory;

    /// <summary>Creates a Helm visualization document opener.</summary>
    /// <param name="factory">Application dock factory.</param>
    public HelmVisualizationDocumentOpener(IFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <inheritdoc />
    public void Open(VisualizationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _factory.AddToDocuments(viewModel);
    }
}
