using Avalonia.Svg.Skia;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

/// <summary>
/// Renders an SVG asset from an <c>avares://KubeUI.Avalonia</c> path and swaps to a second asset when the effective
/// theme variant is dark. Use for monochrome brand icons that need an official dark-background variant.
/// </summary>
public sealed class ThemedSvgImage : Image
{
    /// <summary>Defines the <see cref="SourcePath"/> property.</summary>
    public static readonly StyledProperty<string?> SourcePathProperty =
        AvaloniaProperty.Register<ThemedSvgImage, string?>(nameof(SourcePath));

    /// <summary>Defines the <see cref="DarkSourcePath"/> property.</summary>
    public static readonly StyledProperty<string?> DarkSourcePathProperty =
        AvaloniaProperty.Register<ThemedSvgImage, string?>(nameof(DarkSourcePath));

    private static readonly Uri AppUri = new("avares://KubeUI.Avalonia");

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

    /// <summary>Gets or sets the SVG asset path rendered in the light theme; also used as the dark-theme fallback.</summary>
    public string? SourcePath
    {
        get => GetValue(SourcePathProperty);
        set => SetValue(SourcePathProperty, value);
    }

    /// <summary>Gets or sets the SVG asset path rendered when the effective theme variant is dark.</summary>
    public string? DarkSourcePath
    {
        get => GetValue(DarkSourcePathProperty);
        set => SetValue(DarkSourcePathProperty, value);
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
