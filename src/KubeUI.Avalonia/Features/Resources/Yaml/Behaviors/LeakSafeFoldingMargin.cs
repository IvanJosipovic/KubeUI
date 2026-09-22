using Avalonia.LogicalTree;
using AvaloniaEdit.Folding;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

internal sealed class LeakSafeFoldingMargin : FoldingMargin
{
    protected override void OnTextViewVisualLinesChanged()
    {
        ClearLogicalChildren(this);
        base.OnTextViewVisualLinesChanged();
    }

    internal static void ClearLogicalChildren(FoldingMargin margin)
    {
        var children = ((ILogical)margin).LogicalChildren.ToArray();
        foreach (var child in children)
        {
            ((ISetLogicalParent)child).SetParent(null);
        }
    }
}
