using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Chat;
using KubeUI.Features.AI.Configuration;
using KubeUI.Features.AI.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Features.AI.DependencyInjection;

/// <summary>Registers services and views owned by the AI feature.</summary>
public static class KubeUIAIFeatureServiceCollectionExtensions
{
    /// <summary>Adds the AI feature's runtime services and chat UI.</summary>
    /// <param name="services">Services to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKubeUIAIFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAgentRegistry>(serviceProvider => new AcpAgentRegistry(
            AcpAgentDefaults.Definitions,
            serviceProvider.GetRequiredService<IAgentPermissionService>()));
        services.AddSingleton<IAgentPermissionService, AvaloniaAgentPermissionService>();
        services.AddSingleton<IAgentContextService, AgentContextService>();
        services.AddTransient<AgentChatViewModel>();
        services.AddTransient<AgentChatView>();
        return services;
    }
}
