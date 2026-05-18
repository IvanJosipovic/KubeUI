using Shouldly;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class ApplicationServiceResolutionTests : AvaloniaTestBase
{
    [Fact]
    public void ResetForTest_initializes_services_without_throwing()
    {
        Should.NotThrow(TestApp.ResetForTest);
    }
}
