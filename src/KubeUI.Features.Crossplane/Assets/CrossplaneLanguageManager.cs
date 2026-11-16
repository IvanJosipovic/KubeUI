using Irihi.Lingua;

#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<CrossplaneLanguageManager>))]
#endif

namespace KubeUI.Features.Crossplane.Assets;

/// <summary>
/// The Lingua manager for the Crossplane feature's resource files.
/// </summary>
[LinguaManager("./Resources.json")]
internal sealed partial class CrossplaneLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
