using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Behaviours;
using QuikGraph;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls.Controls.EdgeLabels;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

internal static class ResourceGraphStyles
{
    public static void Apply(GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> area)
    {
        area.Styles.Add(new Style(s => s.OfType<EdgeControl>())
        {
            Setters =
            {
                new Setter(EdgeControlBase.ShowArrowsProperty, true),
                new Setter(EdgeControl.StrokeThicknessProperty, 1d),
                new Setter(
                    TemplatedControl.ForegroundProperty,
                    CompiledBinding.Create<ResourceGraphEdge, IBrush>(edge => edge.Brush)),
                new Setter(
                    ToolTip.TipProperty,
                    CompiledBinding.Create<ResourceGraphEdge, string>(edge => edge.RelationshipName)),
            },
        });

        area.Styles.Add(new Style(s => s.OfType<VertexControl>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
                new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(VertexControlBase.ShowLabelProperty, false),
                new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(DragBehaviour.IsDragEnabledProperty, true),
                new Setter(DragBehaviour.UpdateEdgesOnMoveProperty, true),
                new Setter(VertexControlBase.VertexShapeProperty, VertexShape.Rectangle),
            },
        });

        area.Styles.Add(new Style(s => s.OfType<AttachableEdgeLabelControl>())
        {
            Setters =
            {
                new Setter(EdgeLabelControl.ForegroundProperty, Brushes.White),
                new Setter(TemplatedControl.TemplateProperty,
                    new FuncControlTemplate<AttachableEdgeLabelControl>((label, _) => new Border
                    {
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(8),
                        Child = new ContentPresenter
                        {
                            Content = label.AttachNode?.Edge,
                            //Margin = new Thickness(3),
                        },
                    })),
            },
        });
    }
}
