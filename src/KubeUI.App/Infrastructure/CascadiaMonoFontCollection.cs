using Avalonia.Media.Fonts;

namespace KubeUI.App.Infrastructure;

public sealed class CascadiaMonoFontCollection : EmbeddedFontCollection
{
    public CascadiaMonoFontCollection() : base(
        new Uri("fonts:Cascadia Mono", UriKind.Absolute),
        new Uri("avares://KubeUI.Foundation/Assets", UriKind.Absolute))
    {
    }
}
