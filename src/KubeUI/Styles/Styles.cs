using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace KubeUI.Styles;

public class Styles : Avalonia.Styling.Styles
{
    public Styles() => AddRange([
        new Style<TreeDataGridColumnHeader>()
            .Setter(TreeDataGridColumnHeader.BackgroundProperty, Brushes.Black),

        new Style(x => x.Is<TreeDataGridCell>())
            .Setter(TreeDataGridCell.BorderBrushProperty, Brushes.DarkGray)
            .Setter(TreeDataGridCell.BorderThicknessProperty, Thickness.Parse("0.5"))
    ]);
}
