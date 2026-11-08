using Avalonia.Controls;
using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Resources.Properties;

/// <summary>Builds the shared section layout used by resource-kind property views.</summary>
/// <remarks>
/// This stays in Resources because it composes resource-property presentation, while individual
/// resource kinds retain ownership of their labels, values, and section contents.
/// </remarks>
public static class ResourcePropertiesSection
{
    /// <summary>Creates an initially expanded property section with a vertical content stack.</summary>
    /// <param name="header">Localized text or a binding used as the section header.</param>
    /// <param name="items">The resource-specific controls displayed in the section.</param>
    /// <returns>The composed expandable section.</returns>
    public static ExpandableSection Create(object? header, params Control[] items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new ExpandableSection
        {
            Header = header,
            Content = new StackPanel().Children(items)
        };
    }
}
