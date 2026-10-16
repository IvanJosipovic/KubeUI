using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Kubernetes.Tests.Configuration;

public sealed class KubeUIKubernetesServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureKubeUIKubernetesJsonLogging_remains_available_for_existing_consumers()
    {
        using var services = new ServiceCollection().BuildServiceProvider();

#pragma warning disable CS0618
        services.ConfigureKubeUIKubernetesJsonLogging();
#pragma warning restore CS0618
    }
}
