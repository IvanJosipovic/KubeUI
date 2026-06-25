using System.Globalization;
using System.Linq.Expressions;
using Avalonia.Data.Converters;
using Avalonia.Markup.Declarative;
using KubeUI.Avalonia.Options;

namespace KubeUI.Avalonia.Shell.Documents.Settings;

public sealed class SettingsView() : ViewBase<SettingsViewModel>
{
    protected override object Build(SettingsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                CreateThemeRow(vm),
                CreateToggleRow(
                    vm,
                    Assets.Resources.SettingsView_Logging_ToolTip!,
                    Assets.Resources.SettingsView_Logging_Label!,
                    x => x.SettingsService.Settings.LoggingEnabled),
                CreateToggleRow(
                    vm,
                    Assets.Resources.SettingsView_Telemetry_ToolTip!,
                    Assets.Resources.SettingsView_Telemetry_Label!,
                    x => x.SettingsService.Settings.TelemetryEnabled),
                CreateToggleRow(
                    vm,
                    Assets.Resources.SettingsView_PreRelease_ToolTip!,
                    Assets.Resources.SettingsView_PreRelease_Label!,
                    x => x.SettingsService.Settings.PreReleaseChannel),
                CreateNumericRow(
                    vm,
                    Assets.Resources.SettingsView_FontSize_ToolTip!,
                    Assets.Resources.SettingsView_FontSize_Label!,
                    x => x.SettingsService.Appearance.FontSize),
                CreateNumericRow(
                    vm,
                    Assets.Resources.SettingsView_ConsoleFontSize_ToolTip!,
                    Assets.Resources.SettingsView_ConsoleFontSize_Label!,
                    x => x.SettingsService.Appearance.ConsoleFontSize),
                CreateNumericRow(
                    vm,
                    Assets.Resources.SettingsView_ListRowHeight_ToolTip!,
                    Assets.Resources.SettingsView_ListRowHeight_Label!,
                    x => x.SettingsService.Appearance.ListRowHeight));
    }

    private static Grid CreateThemeRow(SettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .Children(
                new Label()
                    .Content(Assets.Resources.SettingsView_Theme)
                    .Col(0),
                new StackPanel().Children(
                    CreateThemeChoice(
                        vm,
                        Assets.Resources.SettingsView_Theme_Default!,
                        LocalThemeVariant.Default),
                    CreateThemeChoice(
                        vm,
                        Assets.Resources.SettingsView_Theme_Light!,
                        LocalThemeVariant.Light),
                    CreateThemeChoice(
                        vm,
                        Assets.Resources.SettingsView_Theme_Dark!,
                        LocalThemeVariant.Dark))
                    .Col(1));
    }

    private static RadioButton CreateThemeChoice(SettingsViewModel vm, string content, LocalThemeVariant variant)
    {
        return new RadioButton()
            .Content(content)
            .GroupName("Theme")
            .IsChecked(vm, x => x.SettingsService.Appearance.Theme, BindingMode.TwoWay, new ThemeEqualityConverter(variant));
    }

    private static Grid CreateToggleRow<TValue>(SettingsViewModel vm, string toolTip, string label, Expression<Func<SettingsViewModel, TValue>> getter)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(toolTip)
            .Children(
                new Label()
                    .Content(label)
                    .Col(0),
                new CheckBox()
                    .Col(1)
                    .IsChecked(vm, getter, BindingMode.TwoWay));
    }

    private static Grid CreateNumericRow(SettingsViewModel vm, string toolTip, string label, Expression<Func<SettingsViewModel, decimal>> getter)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(toolTip)
            .Children(
                new Label()
                    .Content(label)
                    .Col(0),
                new NumericUpDown()
                    .Col(1)
                    .Value(vm, getter, BindingMode.TwoWay));
    }
}

internal sealed class ThemeEqualityConverter : IValueConverter
{
    private readonly LocalThemeVariant _variant;

    public ThemeEqualityConverter(LocalThemeVariant variant)
    {
        _variant = variant;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is LocalThemeVariant theme && theme == _variant;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? _variant : BindingOperations.DoNothing;
}
