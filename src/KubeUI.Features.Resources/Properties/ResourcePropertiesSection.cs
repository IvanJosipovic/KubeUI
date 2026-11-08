using Avalonia;
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
    /// <param name="header">Fixed section-header text.</param>
    /// <param name="items">The resource-specific controls displayed in the section.</param>
    /// <returns>The composed expandable section.</returns>
    public static ExpandableSection Create(string header, params Control[] items)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(items);

        var section = CreateCore(items);
        section.Header = header;
        return section;
    }

    /// <summary>Creates an initially expanded section with a reactive localized header.</summary>
    /// <param name="header">Observable text bound to the section header.</param>
    /// <param name="items">The resource-specific controls displayed in the section.</param>
    /// <returns>The composed expandable section.</returns>
    public static ExpandableSection Create(IObservable<string?> header, params Control[] items)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(items);

        return CreateCore(items).Header(header.ToBinding());
    }

    private static ExpandableSection CreateCore(Control[] items) =>
        new()
        {
            Content = new StackPanel().Children(items)
        };
}
