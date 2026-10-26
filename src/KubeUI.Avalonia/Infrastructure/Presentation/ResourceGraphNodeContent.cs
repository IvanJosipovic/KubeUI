namespace KubeUI.Avalonia.Infrastructure.Presentation;

/// <summary>Builds shared identity content for resource and Hubble graph nodes.</summary>
internal static class ResourceGraphNodeContent
{
    /// <summary>Creates the centered icon, kind, and name stack shared by graph visualizations.</summary>
    public static StackPanel Create(Control? icon, TextBlock kind, TextBlock name, bool fixedHeight = true)
    {
        var content = new StackPanel
        {
            Width = 128,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (fixedHeight)
        {
            content.Height = 128;
        }

        if (icon is not null)
        {
            icon.Width = 64;
            icon.Height = 64;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            content.Children.Add(icon);
        }

        kind.TextAlignment = TextAlignment.Center;
        kind.TextWrapping = TextWrapping.Wrap;
        content.Children.Add(kind);

        name.TextAlignment = TextAlignment.Center;
        name.TextWrapping = TextWrapping.Wrap;
        content.Children.Add(name);
        return content;
    }
}
