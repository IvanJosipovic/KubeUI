using System.Globalization;
using Avalonia;
using SVCSystems.Avalonia.CSharpUI;
using Avalonia.Media;
using KubeUI.Controls.Styles;
using KubeUI.Features.Cilium.Hubble;

namespace KubeUI.Features.Cilium.Hubble.Controls;

public sealed partial class FlowEndpointView : StackPanel
{
    [GeneratedStyledProperty]
    public partial FlowEndpoint? Endpoint { get; set; }

    [GeneratedStyledProperty]
    public partial bool IsSource { get; set; }

    public FlowEndpointView()
    {
        this.Orientation(Orientation.Vertical).Spacing(2);

        this.GetObservable(EndpointProperty).Subscribe(e =>
        {
            Children.Clear();
            if (e == null)
                return;

            var secondaryBrush = ApplicationBrushResources.GetBrush("SemanticMutedBrush");

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
                var identityStr = e.Identity.Value.ToString(CultureInfo.InvariantCulture);
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
