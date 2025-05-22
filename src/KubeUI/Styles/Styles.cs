using Avalonia.Controls.Primitives;

namespace KubeUI.Styles;

public class Styles : Avalonia.Styling.Styles
{
    public Styles() => AddRange([
            new Style<TreeDataGridColumnHeader>()
              .Setter(TreeDataGridColumnHeader.BackgroundProperty, nameof(ColorPaletteResources.AltHigh).GetDynamicResource())
            ]);
}
