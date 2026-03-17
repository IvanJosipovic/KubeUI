using Avalonia.Media.Fonts;

namespace KubeUI.Assets;

public sealed class CascadiaMonoFontCollection : EmbeddedFontCollection
{
    public CascadiaMonoFontCollection() : base(
        new Uri("fonts:Cascadia Mono", UriKind.Absolute),
        new Uri("avares://KubeUI.Avalonia/Assets", UriKind.Absolute))
    {
    }
}

