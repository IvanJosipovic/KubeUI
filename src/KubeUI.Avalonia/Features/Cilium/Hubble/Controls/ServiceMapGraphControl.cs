using System.Globalization;
using KubeUI.Cilium.Hubble;

namespace KubeUI.Avalonia.Features.Cilium.Hubble.Controls;

/// <summary>Draws a compact, scrollable node and flow graph.</summary>
public sealed class ServiceMapGraphControl : Control
{
    public static readonly DirectProperty<ServiceMapGraphControl, ServiceMapGraph?> GraphProperty =
        AvaloniaProperty.RegisterDirect<ServiceMapGraphControl, ServiceMapGraph?>(
            nameof(Graph),
            control => control.Graph,
            (control, value) => control.Graph = value);

    private const double NodeWidth = 180;
    private const double NodeHeight = 76;
    private const double ColumnGap = 44;
    private const double RowGap = 48;
    private ServiceMapGraph? _graph;

    /// <summary>Gets or sets the graph rendered by this control.</summary>
    public ServiceMapGraph? Graph
    {
        get => _graph;
        set
        {
            if (ReferenceEquals(_graph, value))
            {
                return;
            }

            var oldValue = _graph;
            _graph = value;
            RaisePropertyChanged(GraphProperty, oldValue, value);
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var count = _graph?.Nodes.Count ?? 0;
        var columns = count == 0 ? 1 : (int)Math.Ceiling(Math.Sqrt(count));
        var rows = count == 0 ? 1 : (int)Math.Ceiling(count / (double)columns);
        return new Size(
            Math.Max(560, columns * (NodeWidth + ColumnGap) + ColumnGap),
            Math.Max(340, rows * (NodeHeight + RowGap) + RowGap));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_graph is not { Nodes.Count: > 0 } graph)
        {
            return;
        }

        var columns = (int)Math.Ceiling(Math.Sqrt(graph.Nodes.Count));
        var centers = new Dictionary<string, Point>(StringComparer.Ordinal);
        for (var index = 0; index < graph.Nodes.Count; index++)
        {
            var row = index / columns;
            var column = index % columns;
            var node = graph.Nodes[index];
            centers[node.Id] = new Point(
                ColumnGap + column * (NodeWidth + ColumnGap) + NodeWidth / 2,
                RowGap + row * (NodeHeight + RowGap) + NodeHeight / 2);
        }

        var edgePen = new Pen(Brushes.SlateGray, 2);
        foreach (var edge in graph.Edges)
        {
            if (!centers.TryGetValue(edge.SourceId, out var source)
                || !centers.TryGetValue(edge.TargetId, out var target))
            {
                continue;
            }

            context.DrawLine(edgePen, source, target);
            var edgeLabel = CreateText(edge.FlowCount.ToString(CultureInfo.CurrentCulture), 11, Brushes.DimGray);
            context.DrawText(edgeLabel, new Point((source.X + target.X) / 2, (source.Y + target.Y) / 2));
        }

        for (var index = 0; index < graph.Nodes.Count; index++)
        {
            var node = graph.Nodes[index];
            var center = centers[node.Id];
            var rectangle = new Rect(center.X - NodeWidth / 2, center.Y - NodeHeight / 2, NodeWidth, NodeHeight);
            var background = node.DeniedCount > 0 ? Brushes.DarkRed : Brushes.DarkSlateBlue;
            context.DrawRectangle(background, new Pen(Brushes.White, 1), rectangle, 8, 8);

            var namespaceLabel = string.IsNullOrEmpty(node.Namespace)
                ? Assets.Resources.ServiceMapGraphControl_External
                : node.Namespace;
            var counts = string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.ServiceMapGraphControl_FlowCountsFormat,
                node.IngressFlowCount,
                node.EgressFlowCount,
                node.DeniedCount);
            context.DrawText(CreateText(Shorten(node.Name), 14, Brushes.White), new Point(rectangle.X + 10, rectangle.Y + 8));
            context.DrawText(CreateText(Shorten(namespaceLabel), 11, Brushes.WhiteSmoke), new Point(rectangle.X + 10, rectangle.Y + 30));
            context.DrawText(CreateText(counts, 10, Brushes.WhiteSmoke), new Point(rectangle.X + 10, rectangle.Y + 52));
        }
    }

    private static FormattedText CreateText(string text, double size, IBrush brush)
        => new(
            text,
            CultureInfo.CurrentCulture,
            global::Avalonia.Media.FlowDirection.LeftToRight,
            Typeface.Default,
            size,
            brush);

    private static string Shorten(string text)
        => text.Length <= 24 ? text : string.Concat(text.AsSpan(0, 21), "…");
}
