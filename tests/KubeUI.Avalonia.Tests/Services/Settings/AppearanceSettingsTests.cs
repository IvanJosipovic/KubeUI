using Avalonia;
using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Services.Settings;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Services.Settings;

public sealed class AppearanceSettingsTests
{
    [AvaloniaFact]
    public void default_appearance_settings_are_applied_to_application_resources()
    {
        var application = Application.Current!;
        var settings = application.GetRequiredTestService<ISettingsService>();

        settings.Appearance.ListRowHeight.ShouldBe(22);
        application.Resources.ContainsKey("DataGridRowHeight").ShouldBeTrue();
        application.Resources["DataGridRowHeight"].ShouldBe(22d);
    }
}
