using Mapster;

namespace KubeUI.Kubernetes;

internal static class MapsterConfiguration
{
    private static int _isConfigured;

    internal static void Configure()
    {
        if (Interlocked.Exchange(ref _isConfigured, 1) == 1)
        {
            return;
        }

        TypeAdapterConfig.GlobalSettings.Default.ShallowCopyForSameType(true);
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
    }
}
