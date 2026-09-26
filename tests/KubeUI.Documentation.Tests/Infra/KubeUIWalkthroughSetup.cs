using Avalonia;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using k8s;
using k8s.Models;

namespace KubeUI.Documentation.Tests.Infra;

internal static class KubeUIWalkthroughServices
{
    public static IServiceProvider GetRequiredServices() =>
        (Application.Current as IServiceProviderHost)?.Services
        ?? throw new InvalidOperationException("Test application services were not initialized.");
}

internal sealed record WalkthroughDemoResources(
    IReadOnlyCollection<IKubernetesObject<V1ObjectMeta>> Resources,
    V1Namespace DefaultNamespace);
