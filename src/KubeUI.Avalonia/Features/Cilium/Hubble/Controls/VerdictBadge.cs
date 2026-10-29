using Avalonia;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using KubeUI.Avalonia.Styles;
using KubeUI.Cilium.Hubble;

namespace KubeUI.Avalonia.Features.Cilium.Hubble.Controls;

public sealed class VerdictBadge : Border
{
    public static readonly StyledProperty<FlowVerdict> VerdictProperty =
        AvaloniaProperty.Register<VerdictBadge, FlowVerdict>(nameof(Verdict));

    public FlowVerdict Verdict
    {
        get => GetValue(VerdictProperty);
        set => SetValue(VerdictProperty, value);
    }

    public VerdictBadge()
    {
        this.CornerRadius(4)
            .Padding(6, 2)
            .MinWidth(60)
            .HorizontalAlignment(HorizontalAlignment.Center);

        this.GetObservable(VerdictProperty).Subscribe(v =>
        {
            Background = v switch
            {
                FlowVerdict.Forwarded => ApplicationBrushResources.GetBrush("PodStatusReadyBrush"),
                FlowVerdict.Dropped or FlowVerdict.Error => ApplicationBrushResources.GetBrush("ContainerStatusErrorBrush"),
                _ => ApplicationBrushResources.GetBrush("SubtleOutlineBrush")
            };
            Child = new TextBlock
            {
                Text = v switch
                {
                    FlowVerdict.Forwarded => Assets.Resources.FlowVerdict_Allowed,
                    FlowVerdict.Dropped => Assets.Resources.FlowVerdict_Dropped,
                    FlowVerdict.Error => Assets.Resources.FlowVerdict_Error,
                    _ => Assets.Resources.FlowVerdict_Unknown
                },
                Foreground = ApplicationBrushResources.GetBrush("SystemBaseHighBrush"),
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        });
    }
}
