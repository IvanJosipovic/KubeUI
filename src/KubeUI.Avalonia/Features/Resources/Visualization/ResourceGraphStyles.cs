using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Behaviours;
using QuikGraph;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls.Controls.EdgeLabels;
using Westermo.GraphX.Controls.Controls.EdgePointers;
using Path = Avalonia.Controls.Shapes.Path;
using Shape = Avalonia.Controls.Shapes.Shape;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

internal static class ResourceGraphStyles
{
    public static void Apply(GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> area)
    {
        area.Styles.Add(new Style(s => s.OfType<EdgeControl>())
        {
            Setters =
            {
                new Setter(EdgeControl.StrokeThicknessProperty, 1d),
                new Setter(Visual.OpacityProperty, 0.5d),
                new Setter(
                    TemplatedControl.ForegroundProperty,
                    CompiledBinding.Create<ResourceGraphEdge, IBrush>(edge => edge.Brush)),
                new Setter(
                    ToolTip.TipProperty,
                    CompiledBinding.Create<ResourceGraphEdge, string>(edge => edge.RelationshipName)),
                // GraphX exposes only a single ShowArrows switch. Its default template therefore renders
                // arrows at both ends; use a target-only template for directed Kubernetes relationships.
                new Setter(TemplatedControl.TemplateProperty, CreateDirectedEdgeTemplate()),
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
                new Setter(EdgeLabelControl.AlignToEdgeProperty, true),
                new Setter(TemplatedControl.TemplateProperty,
                    new FuncControlTemplate<AttachableEdgeLabelControl>((label, _) => new Border
                    {
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(0),
                        Child = new ContentPresenter
                        {
                            Content = label.AttachNode?.Edge,
                            //Margin = new Thickness(3),
                        },
                    })),
            },
        });

    }

    private static FuncControlTemplate<EdgeControl> CreateDirectedEdgeTemplate()
    {
        return new FuncControlTemplate<EdgeControl>((_, nameScope) =>
        {
            Path edgePath = new() { Name = "PART_edgePath" };
            edgePath.Bind(Shape.StrokeProperty, new TemplateBinding { Property = TemplatedControl.ForegroundProperty });
            edgePath.Bind(Shape.StrokeThicknessProperty, new TemplateBinding { Property = EdgeControl.StrokeThicknessProperty });

            Path targetArrow = new()
            {
                Data = Geometry.Parse("M0,0.5 L1,1 1,0"),
                Stretch = Stretch.Uniform,
                Width = 10,
                Height = 10,
            };
            targetArrow.Bind(Shape.FillProperty, new TemplateBinding { Property = TemplatedControl.ForegroundProperty });

            Path selfLoop = new()
            {
                Name = "PART_SelfLoopedEdge",
                Data = Geometry.Parse("F1 M 17.4167,32.25L 32.9107,32.25L 38,18L 43.0893,32.25L 58.5833,32.25L 45.6798,41.4944L 51.4583,56L 38,48.0833L 26.125,56L 30.5979,41.7104L 17.4167,32.25 Z"),
                Stretch = Stretch.Uniform,
                Width = 10,
                Height = 10,
            };
            selfLoop.Bind(Shape.FillProperty, new TemplateBinding { Property = TemplatedControl.ForegroundProperty });

            DefaultEdgePointer targetPointer = new()
            {
                Name = "PART_EdgePointerForTarget",
                NeedRotation = true,
                Content = targetArrow,
                Width = 10,
                Height = 10,
            };
            nameScope.Register("PART_edgePath", edgePath);
            nameScope.Register("PART_EdgePointerForTarget", targetPointer);
            nameScope.Register("PART_SelfLoopedEdge", selfLoop);

            return new Canvas
            {
                Children =
                {
                    edgePath,
                    targetPointer,
                    selfLoop,
                },
            };
        });
    }
}
