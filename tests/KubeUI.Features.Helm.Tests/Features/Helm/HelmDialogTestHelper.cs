using System.ComponentModel;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Avalonia.Tests.Features.Helm;

internal static class HelmDialogTestHelper
{
    public static IDialogService CreateConfirmingDialogService(ICollection<ContentDialogSettings>? capturedDialogs = null)
    {
        var manager = new Mock<IDialogManager>();
        manager.SetupGet(dialogManager => dialogManager.Logger).Returns((ILogger<IDialogManager>?)null);
        manager.SetupProperty(dialogManager => dialogManager.AllowConcurrentDialogs);
        manager
            .Setup(dialogManager => dialogManager.ShowFrameworkDialogAsync(
                It.IsAny<INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .Callback<INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>(
                (_, settings, _) => capturedDialogs?.Add(settings))
            .ReturnsAsync(FAContentDialogResult.Primary);

        return new DialogService(manager.Object);
    }
}
