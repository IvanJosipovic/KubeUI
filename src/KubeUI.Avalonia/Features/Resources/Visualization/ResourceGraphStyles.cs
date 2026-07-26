using Avalonia.Controls.Primitives;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Behaviours;
using QuikGraph;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

internal static class ResourceGraphStyles
{
    public static void Apply(GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> area)
    {
        area.Styles.Add(new Style(selector => selector.OfType<EdgeControl>())
        {
            Setters =
            {
                new Setter(EdgeControl.StrokeThicknessProperty, 1d),
                new Setter(
                    TemplatedControl.ForegroundProperty,
                    CompiledBinding.Create<ResourceGraphEdge, IBrush>(edge => edge.Brush)),
                new Setter(
                    ToolTip.TipProperty,
                    CompiledBinding.Create<ResourceGraphEdge, string>(edge => edge.RelationshipName)),
            },
        });
        area.Styles.Add(new Style(selector => selector.OfType<VertexControl>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
                new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Top),
                new Setter(DragBehaviour.IsDragEnabledProperty, true),
                new Setter(DragBehaviour.UpdateEdgesOnMoveProperty, true),
            },
        });
    }

}
