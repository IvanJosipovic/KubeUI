using Irihi.Lingua;

#if DEBUG
using KubeUI.Foundation.Localization;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<KubeUI.Foundation.Assets.FoundationLanguageManager>))]
#endif

namespace KubeUI.Foundation.Assets;

/// <summary>
/// The Lingua manager for the shared foundation resource files. This is the only public
/// manager; feature managers stay internal to their feature assemblies.
/// </summary>
[LinguaManager("./Resources.json")]
public sealed partial class FoundationLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
