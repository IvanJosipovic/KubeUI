using Avalonia.Media.Fonts;

public sealed class CascadiaMonoFontCollection : EmbeddedFontCollection
{
    public CascadiaMonoFontCollection() : base(
        new Uri("fonts:CascadiaMono-Regular", UriKind.Absolute),
        new Uri("avares://KubeUI/Assets", UriKind.Absolute))
    {
    }
}
