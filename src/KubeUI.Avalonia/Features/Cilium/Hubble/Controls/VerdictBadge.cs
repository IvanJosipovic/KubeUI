using Avalonia;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
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
            var allowedBrush = Application.Current?.TryGetResource("PodStatusReadyBrush", Application.Current.ActualThemeVariant, out var r1) == true && r1 is IBrush b1 ? b1 : Brushes.Green;
            var droppedBrush = Application.Current?.TryGetResource("ContainerStatusErrorBrush", Application.Current.ActualThemeVariant, out var r2) == true && r2 is IBrush b2 ? b2 : Brushes.Red;
            var errorBrush = Application.Current?.TryGetResource("ContainerStatusErrorBrush", Application.Current.ActualThemeVariant, out var r3) == true && r3 is IBrush b3 ? b3 : Brushes.DarkRed;
            var unknownBrush = Application.Current?.TryGetResource("SubtleOutlineBrush", Application.Current.ActualThemeVariant, out var r4) == true && r4 is IBrush b4 ? b4 : Brushes.Gray;
            var textBrush = Application.Current?.TryGetResource("SystemBaseHighBrush", Application.Current.ActualThemeVariant, out var r5) == true && r5 is IBrush b5 ? b5 : Brushes.White;

            Background = v switch
            {
                FlowVerdict.Forwarded => allowedBrush,
                FlowVerdict.Dropped => droppedBrush,
                FlowVerdict.Error => errorBrush,
                _ => unknownBrush
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
                Foreground = textBrush,
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        });
    }
}