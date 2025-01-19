using Avalonia.Media.Fonts;

public sealed class CascadiaMonoFontCollection : EmbeddedFontCollection
{
    public CascadiaMonoFontCollection() : base(
        new Uri("fonts:Cascadia Mono", UriKind.Absolute),
        new Uri("avares://KubeUI/Assets", UriKind.Absolute))
    {
    }
}
