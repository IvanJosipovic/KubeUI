using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium.Hubble;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class HubbleServiceRegistrationTests
{
    [AvaloniaFact]
    public async Task App_services_register_hubble_factory_and_view_models()
    {
        var services = Application.Current.GetTestServices();

        services.GetRequiredService<HubbleClientFactory>().ShouldNotBeNull();
        await using var flowList = services.GetRequiredService<FlowListViewModel>();
        await using var serviceMap = services.GetRequiredService<ServiceMapViewModel>();
        flowList.ShouldBeAssignableTo<IDisposable>();
        serviceMap.ShouldBeAssignableTo<IDisposable>();
        serviceMap.ShouldNotBeNull();
    }
}
