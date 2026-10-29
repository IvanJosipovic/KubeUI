using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Presentation;

public sealed class ThemedSvgImageTests
{
    [AvaloniaFact]
    public void themed_svg_image_swaps_helm_icon_with_theme_variant()
    {
        var originalTheme = Application.Current!.RequestedThemeVariant;
        var window = new Window();
        try
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            ThemedSvgImage image = new()
            {
                SourcePath = HelmNavigationSectionProvider.IconAssetPath,
                DarkSourcePath = HelmNavigationSectionProvider.IconDarkAssetPath,
            };
            window.Content = image;
            window.Show();

            var lightSource = LoadedSource(image);

            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            var darkSource = LoadedSource(image);
            darkSource.ShouldNotBeSameAs(lightSource);

            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            LoadedSource(image).ShouldBeSameAs(lightSource);
        }
        finally
        {
            window.Close();
            Application.Current.RequestedThemeVariant = originalTheme;
        }
    }

    [AvaloniaFact]
    public void themed_svg_image_keeps_light_asset_when_dark_variant_is_missing()
    {
        var originalTheme = Application.Current!.RequestedThemeVariant;
        var window = new Window();
        try
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            ThemedSvgImage image = new() { SourcePath = HelmNavigationSectionProvider.IconAssetPath };
            window.Content = image;
            window.Show();

            LoadedSource(image).Picture.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Application.Current.RequestedThemeVariant = originalTheme;
        }
    }

    [AvaloniaFact]
    public void themed_svg_image_clears_source_when_paths_are_empty()
    {
        var window = new Window();
        ThemedSvgImage image = new();
        window.Content = image;
        window.Show();

        image.Source.ShouldBeNull();

        image.SourcePath = HelmNavigationSectionProvider.IconAssetPath;
        LoadedSource(image).Picture.ShouldNotBeNull();

        image.SourcePath = null;
        image.Source.ShouldBeNull();
        window.Close();
    }

    private static SvgSource LoadedSource(ThemedSvgImage image)
        => image.Source.ShouldBeOfType<SvgImage>().Source.ShouldNotBeNull();
}
