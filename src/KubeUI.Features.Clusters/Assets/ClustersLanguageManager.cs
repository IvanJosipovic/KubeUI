using Irihi.Lingua;

#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<ClustersLanguageManager>))]
#endif

namespace KubeUI.Features.Clusters.Assets;

/// <summary>
/// The Lingua manager for the Clusters feature's resource files.
/// </summary>
[LinguaManager("./Resources.json")]
internal sealed partial class ClustersLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
