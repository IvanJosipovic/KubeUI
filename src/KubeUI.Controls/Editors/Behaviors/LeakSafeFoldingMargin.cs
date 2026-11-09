using Avalonia.LogicalTree;
using AvaloniaEdit.Folding;

namespace KubeUI.Controls.Editors.Behaviors;

internal sealed class LeakSafeFoldingMargin : FoldingMargin
{
    protected override void OnTextViewVisualLinesChanged()
    {
        ClearLogicalChildren(this);
        base.OnTextViewVisualLinesChanged();
    }

    /// <summary>Detaches and removes logical children retained by a folding margin.</summary>
    internal static void ClearLogicalChildren(FoldingMargin margin)
    {
        var logicalChildren = ((ILogical)margin).LogicalChildren;
        var children = logicalChildren.ToArray();
        foreach (var child in children)
        {
            ((ISetLogicalParent)child).SetParent(null);
        }
        ((IList)logicalChildren).Clear();
    }
}
