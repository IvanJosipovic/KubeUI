using Avalonia.Svg.Skia;

namespace KubeUI.Controls.Presentation;

/// <summary>
/// Renders an SVG asset from an <c>avares://KubeUI.Foundation</c> path and swaps to a second asset when the effective
/// theme variant is dark. Use for monochrome brand icons that need an official dark-background variant.
/// </summary>
public sealed partial class ThemedSvgImage : Image
{
    /// <summary>Gets or sets the SVG asset path rendered in the light theme; also used as the dark-theme fallback.</summary>
    [GeneratedStyledProperty]
    public partial string? SourcePath { get; set; }

    /// <summary>Gets or sets the SVG asset path rendered when the effective theme variant is dark.</summary>
    [GeneratedStyledProperty]
    public partial string? DarkSourcePath { get; set; }

    private static readonly Uri AppUri = new("avares://KubeUI.Foundation");

    private (string Path, SvgImage Image)? _light;
    private (string Path, SvgImage Image)? _dark;

    static ThemedSvgImage()
    {
        SourcePathProperty.Changed.AddClassHandler<ThemedSvgImage>(static (image, _) => image.UpdateSource());
        DarkSourcePathProperty.Changed.AddClassHandler<ThemedSvgImage>(static (image, _) => image.UpdateSource());
    }

    /// <summary>Initializes the control and watches effective theme changes.</summary>
    public ThemedSvgImage()
    {
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == nameof(SourcePath)
            || change.Property.Name == nameof(DarkSourcePath)
            || change.Property.Name == "ActualThemeVariant")
        {
            UpdateSource();
        }
    }

    private void UpdateSource()
    {
        var useDark = ActualThemeVariant == ThemeVariant.Dark && !string.IsNullOrWhiteSpace(DarkSourcePath);
        var path = useDark ? DarkSourcePath : SourcePath;

        if (string.IsNullOrWhiteSpace(path))
        {
            Source = null;
            return;
        }

        if (useDark)
        {
            if (_dark is not { } cached || cached.Path != path)
            {
                cached = (path, Load(path));
                _dark = cached;
            }

            Source = cached.Image;
        }
        else
        {
            if (_light is not { } cached || cached.Path != path)
            {
                cached = (path, Load(path));
                _light = cached;
            }

            Source = cached.Image;
        }
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => UpdateSource();

    private static SvgImage Load(string path) => new() { Source = SvgSource.Load(path, AppUri) };
}
