using Irihi.Lingua;

#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<AppLanguageManager>))]
#endif

namespace KubeUI.App.Assets;

/// <summary>
/// The Lingua manager for the App shell's resource files.
/// </summary>
[LinguaManager("./Resources.json")]
internal sealed partial class AppLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
