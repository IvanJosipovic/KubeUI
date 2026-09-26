using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Svg.Skia;

namespace KubeUI.Documentation.Tests.Infra;

internal sealed class KubeUIWalkthroughIntroView : UserControl
{
    public const int VideoWidth = 1440;
    public const int VideoHeight = 900;

    public KubeUIWalkthroughIntroView(string title, SvgImage icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(icon);

        this.Width(VideoWidth)
            .Height(VideoHeight)
            .Content(
                new Grid()
                    .Width(VideoWidth)
                    .Height(VideoHeight)
                    .Background(new DynamicResourceExtension("SystemAltHighColor"))
                    .Children(
                        new StackPanel()
                            .HorizontalAlignment(global::Avalonia.Layout.HorizontalAlignment.Center)
                            .VerticalAlignment(global::Avalonia.Layout.VerticalAlignment.Center)
                            .Spacing(18)
                            .Children(
                                new Image()
                                    .Source(icon)
                                    .Width(250)
                                    .Height(250)
                                    .Stretch(Stretch.Uniform)
                                    .HorizontalAlignment(global::Avalonia.Layout.HorizontalAlignment.Center),
                                new TextBlock()
                                    .Text("KubeUI")
                                    .FontSize(56)
                                    .FontWeight(FontWeight.SemiBold)
                                    .Foreground(new DynamicResourceExtension("SystemBaseHighBrush"))
                                    .TextAlignment(TextAlignment.Center)
                                    .HorizontalAlignment(global::Avalonia.Layout.HorizontalAlignment.Center),
                                new TextBlock()
                                    .Text(title)
                                    .FontSize(36)
                                    .Foreground(new DynamicResourceExtension("SystemBaseHighBrush"))
                                    .TextAlignment(TextAlignment.Center)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .MaxWidth(1100)
                                    .HorizontalAlignment(global::Avalonia.Layout.HorizontalAlignment.Center))));
    }
}
