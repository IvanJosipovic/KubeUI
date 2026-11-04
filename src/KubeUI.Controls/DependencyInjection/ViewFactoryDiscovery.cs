using Avalonia.Controls;
using KubeUI.Controls.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Controls.DependencyInjection;

/// <summary>
/// Creates and registers view factories for views discovered through
/// <c>ServiceScan</c> <c>ScanForTypes</c> scanning of <see cref="IViewFactoryDefinition"/> implementations.
/// </summary>
public static class ViewFactoryDiscovery
{
    /// <summary>Creates a view factory that matches view models accepted by <typeparamref name="TView"/> and resolves the view from a service provider.</summary>
    /// <typeparam name="TView">Discovered view type implementing <see cref="IViewFactoryDefinition"/>.</typeparam>
    /// <returns>The view factory consumed by view locator resolution.</returns>
    public static IViewFactory Create<TView>()
        where TView : Control, IViewFactoryDefinition
    {
        return new DiscoveredViewFactory(
            TView.CanCreate,
            static serviceProvider => serviceProvider.GetRequiredService<TView>());
    }

    /// <summary>Registers discovered view factories as singletons.</summary>
    /// <param name="services">Services to configure.</param>
    /// <param name="viewFactories">View factories discovered by a <c>ScanForTypes</c> generated method.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddViewFactories(this IServiceCollection services, params IViewFactory[] viewFactories)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var viewFactory in viewFactories)
        {
            services.AddSingleton(viewFactory);
        }

        return services;
    }
}
