using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using KubeUI.Cilium.Hubble;
using KubeUIFlowDirection = KubeUI.Cilium.Hubble.FlowDirection;

namespace KubeUI.Avalonia.Features.Cilium.Hubble.Controls;

public sealed class DirectionIcon : Border
{
    public static readonly StyledProperty<KubeUIFlowDirection> DirectionProperty =
        AvaloniaProperty.Register<DirectionIcon, KubeUIFlowDirection>(nameof(Direction));

    public KubeUIFlowDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public DirectionIcon()
    {
        this.Width(24).Height(24).HorizontalAlignment(HorizontalAlignment.Center);

        this.GetObservable(DirectionProperty).Subscribe(d =>
        {
            var ingressBrush = Application.Current?.TryGetResource("PodStatusReadyBrush", Application.Current.ActualThemeVariant, out var r1) == true && r1 is IBrush b1 ? b1 : Brushes.Green;
            var egressBrush = Application.Current?.TryGetResource("VisualizationRelationshipStorageBrush", Application.Current.ActualThemeVariant, out var r2) == true && r2 is IBrush b2 ? b2 : Brushes.Blue;

            var icon = d switch
            {
                KubeUIFlowDirection.Ingress => new PathIcon
                {
                    Data = StreamGeometry.Parse("M12,2 L22,12 L12,22 Z"),
                    Foreground = ingressBrush,
                    Width = 16,
                    Height = 16,
                },
                KubeUIFlowDirection.Egress => new PathIcon
                {
                    Data = StreamGeometry.Parse("M12,22 L2,12 L12,2 Z"),
                    Foreground = egressBrush,
                    Width = 16,
                    Height = 16,
                },
                _ => null
            };

            if (icon != null)
            {
                var tooltip = d switch
                {
                    KubeUIFlowDirection.Ingress => Assets.Resources.FlowDirection_Ingress,
                    KubeUIFlowDirection.Egress => Assets.Resources.FlowDirection_Egress,
                    _ => null
                };

                if (tooltip != null)
                {
                    ToolTip.SetTip(icon, tooltip);
                }
            }

            Child = icon;
        });
    }
}
