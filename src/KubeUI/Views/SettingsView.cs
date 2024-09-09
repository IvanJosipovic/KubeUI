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
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Enable Logging (Restart Required)"),
                        new CheckBox()
                            .Col(1)
                            .IsChecked(@vm.SettingsService.Settings.LoggingEnabled),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Pre Release Channel"),
                        new CheckBox()
                            .Col(1)
                            .IsChecked(@vm.SettingsService.Settings.PreReleaseChannel),
                        ]),
        ]);
}
