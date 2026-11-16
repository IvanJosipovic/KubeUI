using Irihi.Lingua;

#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<HelmLanguageManager>))]
#endif

namespace KubeUI.Features.Helm.Assets;

/// <summary>
/// The Lingua manager for the Helm feature's resource files.
/// </summary>
[LinguaManager("./Resources.json")]
internal sealed partial class HelmLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
