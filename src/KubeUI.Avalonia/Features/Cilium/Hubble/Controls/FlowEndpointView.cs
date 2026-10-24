using Avalonia;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using KubeUI.Cilium.Hubble;

namespace KubeUI.Avalonia.Features.Cilium.Hubble.Controls;

public sealed class FlowEndpointView : StackPanel
{
    public static readonly StyledProperty<FlowEndpoint?> EndpointProperty =
        AvaloniaProperty.Register<FlowEndpointView, FlowEndpoint?>(nameof(Endpoint));

    public static readonly StyledProperty<bool> IsSourceProperty =
        AvaloniaProperty.Register<FlowEndpointView, bool>(nameof(IsSource));

    public FlowEndpoint? Endpoint
    {
        get => GetValue(EndpointProperty);
        set => SetValue(EndpointProperty, value);
    }

    public bool IsSource
    {
        get => GetValue(IsSourceProperty);
        set => SetValue(IsSourceProperty, value);
    }

    public FlowEndpointView()
    {
        this.Orientation(Orientation.Vertical).Spacing(2);

        this.GetObservable(EndpointProperty).Subscribe(e =>
        {
            Children.Clear();
            if (e == null)
                return;

            var secondaryBrush = Application.Current?.TryGetResource("SystemBaseMediumBrush", Application.Current.ActualThemeVariant, out var r) == true && r is IBrush b ? b : Brushes.Gray;

            Children.Add(new TextBlock
            {
                Text = $"{e.Namespace}/{e.Pod}",
                FontWeight = FontWeight.SemiBold,
                FontSize = 12
            });

            if (!string.IsNullOrEmpty(e.Workload))
            {
                Children.Add(new TextBlock
                {
                    Text = e.Workload,
                    FontSize = 10,
                    Foreground = secondaryBrush
                });
            }

            if (e.Identity.HasValue)
            {
                var identityStr = e.Identity.Value.ToString();
                Children.Add(new TextBlock
                {
                    Text = $"id: {identityStr[..Math.Min(8, identityStr.Length)]}",
                    FontSize = 9,
                    Foreground = secondaryBrush
                });
            }
        });
    }
}
