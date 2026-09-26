using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using KubeUI.Avalonia.Infrastructure;
using Shouldly;

namespace KubeUI.Documentation.Tests.Infra;

public sealed class KubeUIWalkthroughIntroViewTests
{
    [AvaloniaFact]
    public void intro_view_renders_icon_brand_title_and_video_dimensions()
    {
        var icon = ApplicationIcons.CreateControlPlaneImage();
        var secondIcon = ApplicationIcons.CreateControlPlaneImage();
        secondIcon.Source.ShouldBeSameAs(icon.Source);
        var view = new KubeUIWalkthroughIntroView("Walkthrough title", icon);
        var window = new Window
        {
            Width = KubeUIWalkthroughIntroView.VideoWidth,
            Height = KubeUIWalkthroughIntroView.VideoHeight,
            Content = view,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            view.Width.ShouldBe(KubeUIWalkthroughIntroView.VideoWidth);
            view.Height.ShouldBe(KubeUIWalkthroughIntroView.VideoHeight);

            var canvas = view.Content.ShouldBeOfType<Grid>();
            canvas.Width.ShouldBe(KubeUIWalkthroughIntroView.VideoWidth);
            canvas.Height.ShouldBe(KubeUIWalkthroughIntroView.VideoHeight);
            canvas.Background.ShouldNotBeNull();

            var content = canvas.Children.OfType<StackPanel>().Single();
            var iconImage = content.Children[0].ShouldBeOfType<Image>();
            iconImage.Source.ShouldBeSameAs(icon);
            iconImage.Source.ShouldBeOfType<SvgImage>().Source.ShouldNotBeNull();

            var brand = content.Children[1].ShouldBeOfType<TextBlock>();
            brand.Text.ShouldBe("KubeUI");
            brand.Foreground.ShouldNotBeNull();

            var title = content.Children[2].ShouldBeOfType<TextBlock>();
            title.Text.ShouldBe("Walkthrough title");
            title.Foreground.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void intro_view_rejects_empty_title()
    {
        var icon = ApplicationIcons.CreateControlPlaneImage();

        Should.Throw<ArgumentException>(
            () => new KubeUIWalkthroughIntroView(string.Empty, icon));
    }
}
