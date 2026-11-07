using Avalonia;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using KubeUI.Controls.Styles;
using KubeUI.Features.Cilium.Hubble;

namespace KubeUI.Features.Cilium.Hubble.Controls;

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
                FlowVerdict.Forwarded => ApplicationBrushResources.GetBrush("SemanticSuccessBrush"),
                FlowVerdict.Dropped or FlowVerdict.Error => ApplicationBrushResources.GetBrush("SemanticErrorBrush"),
                _ => ApplicationBrushResources.GetBrush("SemanticMutedBrush")
            };
            Child = new TextBlock
            {
                Text = v switch
                {
                    FlowVerdict.Forwarded => CiliumLanguageManager.Instance.FlowVerdict_Allowed.CurrentValue()!,
                    FlowVerdict.Dropped => CiliumLanguageManager.Instance.FlowVerdict_Dropped.CurrentValue()!,
                    FlowVerdict.Error => CiliumLanguageManager.Instance.FlowVerdict_Error.CurrentValue()!,
                    _ => CiliumLanguageManager.Instance.FlowVerdict_Unknown.CurrentValue()!
                },
                Foreground = ApplicationBrushResources.GetBrush("SystemBaseHighBrush"),
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        });
    }
}
