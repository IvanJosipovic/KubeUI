using Irihi.Lingua;

#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<ResourcesLanguageManager>))]
#endif

namespace KubeUI.Features.Resources.Assets;

/// <summary>
/// The Lingua manager for the Resources feature's resource files.
/// </summary>
[LinguaManager("./Resources.json")]
internal sealed partial class ResourcesLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
