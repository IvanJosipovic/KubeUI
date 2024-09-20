using Avalonia.Data.Converters;

namespace KubeUI.Views;

public sealed class SettingsView : MyViewBase<SettingsViewModel>
{
    protected override object Build(SettingsViewModel vm) =>
        new StackPanel()
            .Children([
                new Grid()
                    .Cols("*,2*")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Theme"),
                        new StackPanel()
                            .Col(1)
                            .Children([
                                new RadioButton()
                                    .GroupName(nameof(Settings.Theme))
                                    .Content("Default")
                                    .OnTapped((x) => {vm.SettingsService.Settings.Theme = LocalThemeVariant.Default;})
                                    .IsChecked(@vm.SettingsService.Settings.Theme, converter: new FuncValueConverter<LocalThemeVariant, bool?>((x) => x == LocalThemeVariant.Default)),
                                new RadioButton()
                                    .GroupName(nameof(Settings.Theme))
                                    .Content("Light")
                                    .OnTapped((x) => {vm.SettingsService.Settings.Theme = LocalThemeVariant.Light;})
                                    .IsChecked(@vm.SettingsService.Settings.Theme, converter: new FuncValueConverter<LocalThemeVariant, bool?>((x) => x == LocalThemeVariant.Light)),
                                new RadioButton()
                                    .GroupName(nameof(Settings.Theme))
                                    .Content("Dark")
                                    .OnTapped((x) => {vm.SettingsService.Settings.Theme = LocalThemeVariant.Dark;})
                                    .IsChecked(@vm.SettingsService.Settings.Theme, converter: new FuncValueConverter<LocalThemeVariant, bool?>((x) => x == LocalThemeVariant.Dark)),
                                ]),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("KubeUI logs to the local file system in the <User Profile>/.kubeui/app.log")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Enable Local File Logging (Restart Required)"),
                        new CheckBox()
                            .Col(1)
                            .IsChecked(@vm.SettingsService.Settings.LoggingEnabled),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("KubeUI gathers telemetry data to help the team gain insights into how users are interacting with the product, troubleshoot issues, and improve overall performance. While this data is valuable for enhancing the user experience, we understand that some users may prefer not to share their usage information.")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Enable Telemetry (Restart Required)"),
                        new CheckBox()
                            .Col(1)
                            .IsChecked(@vm.SettingsService.Settings.TelemetryEnabled),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("Allows updates to pre-release versions of KubeUI")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Pre Release Channel"),
                        new CheckBox()
                            .Col(1)
                            .IsEnabled(false)
                            .IsChecked(@vm.SettingsService.Settings.PreReleaseChannel),
                        ]),
        ]);
}
