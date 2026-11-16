using Irihi.Lingua;

#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(LinguaHotReload<GatewayApiLanguageManager>))]
#endif

namespace KubeUI.Features.GatewayApi.Assets;

/// <summary>
/// The Lingua manager for the Gateway API feature's resource files.
/// </summary>
[LinguaManager("./Resources.json")]
internal sealed partial class GatewayApiLanguageManager
#if DEBUG
    : ILinguaHotReload
#endif
{
#if DEBUG
    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
#endif
}
