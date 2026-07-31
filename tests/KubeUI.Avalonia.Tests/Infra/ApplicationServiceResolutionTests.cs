using Avalonia.Headless.XUnit;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class ApplicationServiceResolutionTests
{
    [AvaloniaFact]
    public void TestAppBuilder_initializes_services()
    {
        TestApp.CurrentServices.ShouldNotBeNull();
    }
}
