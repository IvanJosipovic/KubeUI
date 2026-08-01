using Avalonia;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Tests.Infra;

internal static class TestApplicationExtensions
{
    public static IServiceProvider GetTestServices(this Application? application)
    {
        return application is IServiceProviderHost host
            ? host.Services
            : throw new InvalidOperationException("Test services are not initialized.");
    }
}
