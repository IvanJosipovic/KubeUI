using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace KubeUI.Features.Resources.Styles;

/// <summary>Provides Kubernetes status and relationship brushes used by Resources presentation.</summary>
public sealed class ResourceFeatureStyles : global::Avalonia.Styling.Styles
{
    /// <summary>Creates dynamic Resource-owned brushes for light and dark themes.</summary>
    public ResourceFeatureStyles()
    {
        Resources = new ResourceDictionary
        {
            ThemeDictionaries =
            {
                [ThemeVariant.Light] = CreateLightResources(),
                [ThemeVariant.Dark] = CreateDarkResources()
            }
        };
    }

    private static ResourceDictionary CreateLightResources() => new()
    {
        ["PodStatusReadyBrush"] = Brush("#2E7D32"),
        ["PodStatusWarningBrush"] = Brush("#B35A00"),
        ["ContainerStatusReadyBrush"] = Brush("#2E7D32"),
        ["ContainerStatusInitReadyBrush"] = Brush("#7B3FB5"),
        ["ContainerStatusEphemeralReadyBrush"] = Brush("#0067B8"),
        ["ContainerStatusRunningBrush"] = Brush("#B35A00"),
        ["ContainerStatusInitRunningBrush"] = Brush("#9C4DCC"),
        ["ContainerStatusEphemeralRunningBrush"] = Brush("#2F80C0"),
        ["ContainerStatusWaitingBrush"] = Brush("#C62828"),
        ["ContainerStatusInitWaitingBrush"] = Brush("#AD1457"),
        ["ContainerStatusEphemeralWaitingBrush"] = Brush("#D84315"),
        ["ContainerStatusCompletedBrush"] = Brush("#5C6B73"),
        ["ContainerStatusErrorBrush"] = Brush("#C62828"),
        ["SubtleOutlineBrush"] = Brush("#757575"),
        ["VisualizationRelationshipOwnerBrush"] = Brush("#0067B8"),
        ["VisualizationRelationshipReferenceBrush"] = Brush("#5C6B73"),
        ["VisualizationRelationshipSelectorBrush"] = Brush("#7B3FB5"),
        ["VisualizationRelationshipLabelBrush"] = Brush("#B35A00"),
        ["VisualizationRelationshipStorageBrush"] = Brush("#008577"),
        ["VisualizationRelationshipIdentityBrush"] = Brush("#2E7D32"),
        ["VisualizationRelationshipRbacBrush"] = Brush("#B0005A"),
        ["VisualizationRelationshipEventBrush"] = Brush("#A15C00"),
        ["VisualizationRelationshipGitOpsBrush"] = Brush("#C2185B"),
        ["VisualizationRelationshipDefaultBrush"] = Brush("#5C6B73")
    };

    private static ResourceDictionary CreateDarkResources() => new()
    {
        ["PodStatusReadyBrush"] = Brush("#81C784"),
        ["PodStatusWarningBrush"] = Brush("#FFB454"),
        ["ContainerStatusReadyBrush"] = Brush("#81C784"),
        ["ContainerStatusInitReadyBrush"] = Brush("#C084FC"),
        ["ContainerStatusEphemeralReadyBrush"] = Brush("#4FA3FF"),
        ["ContainerStatusRunningBrush"] = Brush("#FFB454"),
        ["ContainerStatusInitRunningBrush"] = Brush("#D18BFF"),
        ["ContainerStatusEphemeralRunningBrush"] = Brush("#78B8FF"),
        ["ContainerStatusWaitingBrush"] = Brush("#FF6B6B"),
        ["ContainerStatusInitWaitingBrush"] = Brush("#FF80AB"),
        ["ContainerStatusEphemeralWaitingBrush"] = Brush("#FF8A65"),
        ["ContainerStatusCompletedBrush"] = Brush("#B8C4CC"),
        ["ContainerStatusErrorBrush"] = Brush("#FF6B6B"),
        ["SubtleOutlineBrush"] = Brush("#9E9E9E"),
        ["VisualizationRelationshipOwnerBrush"] = Brush("#4FA3FF"),
        ["VisualizationRelationshipReferenceBrush"] = Brush("#B8C4CC"),
        ["VisualizationRelationshipSelectorBrush"] = Brush("#C084FC"),
        ["VisualizationRelationshipLabelBrush"] = Brush("#FFB454"),
        ["VisualizationRelationshipStorageBrush"] = Brush("#40D9C0"),
        ["VisualizationRelationshipIdentityBrush"] = Brush("#81C784"),
        ["VisualizationRelationshipRbacBrush"] = Brush("#FF6FB5"),
        ["VisualizationRelationshipEventBrush"] = Brush("#FFD166"),
        ["VisualizationRelationshipGitOpsBrush"] = Brush("#FF80AB"),
        ["VisualizationRelationshipDefaultBrush"] = Brush("#B8C4CC")
    };

    private static SolidColorBrush Brush(string value) => new(Color.Parse(value));
}
