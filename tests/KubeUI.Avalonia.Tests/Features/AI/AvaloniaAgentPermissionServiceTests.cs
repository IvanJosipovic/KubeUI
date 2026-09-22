using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Features.AI;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AvaloniaAgentPermissionServiceTests
{
    [Fact]
    public async Task allow_dialog_result_returns_allowed_permission()
    {
        ContentDialogSettings? captured = null;
        var manager = new Mock<IDialogManager>();
        manager.SetupGet(x => x.Logger).Returns((Microsoft.Extensions.Logging.ILogger<IDialogManager>?)null);
        manager.Setup(x => x.ShowFrameworkDialogAsync(
                It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .Callback<System.ComponentModel.INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>((_, settings, _) => captured = settings)
            .ReturnsAsync(FAContentDialogResult.Primary);
        var service = new AvaloniaAgentPermissionService(new DialogService(manager.Object));

        var result = await service.RequestPermissionAsync(new AgentPermissionRequest("write_file", "test.txt", true));

        result.Allowed.ShouldBeTrue();
        captured.ShouldNotBeNull();
        captured!.PrimaryButtonText.ShouldBe("Allow");
        captured.SecondaryButtonText.ShouldBe("Deny");
        captured.Content!.ToString().ShouldContain("write_file");
    }

    [Fact]
    public async Task deny_dialog_result_returns_denied_permission()
    {
        var manager = new Mock<IDialogManager>();
        manager.SetupGet(x => x.Logger).Returns((Microsoft.Extensions.Logging.ILogger<IDialogManager>?)null);
        manager.Setup(x => x.ShowFrameworkDialogAsync(
                It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .ReturnsAsync(FAContentDialogResult.Secondary);
        var service = new AvaloniaAgentPermissionService(new DialogService(manager.Object));

        var result = await service.RequestPermissionAsync(new AgentPermissionRequest("run_process", "cmd.exe", true));

        result.Allowed.ShouldBeFalse();
        result.Reason.ShouldContain("denied");
    }
}
