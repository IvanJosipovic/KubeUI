using Avalonia.LogicalTree;
using AvaloniaEdit.Folding;

namespace KubeUI.Features.Resources.Yaml.Behaviors;

internal sealed class FoldingMarginWithLogicalChildCleanup : FoldingMargin
{
    protected override void OnTextViewVisualLinesChanged()
    {
        ClearLogicalChildren(this);
        base.OnTextViewVisualLinesChanged();
    }

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
