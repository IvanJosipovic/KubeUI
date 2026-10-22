using System.Globalization;
using System.Linq.Expressions;
using Avalonia.Automation;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Options;

namespace KubeUI.Avalonia.Shell.Documents.Settings;

public sealed class SettingsView() : ViewBase<SettingsViewModel>
{
    protected override object Build(SettingsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new ScrollViewer()
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
            .Content(
                new StackPanel()
                    .Children(
                        CreateHeading(Assets.Resources.SettingsView_ApplicationHeading!),
                        CreateToggleRow(
                            vm,
                            Assets.Resources.SettingsView_Logging_ToolTip,
                            Assets.Resources.SettingsView_Logging_Label!,
                            x => x.SettingsService.Settings.LoggingEnabled),
                        CreateToggleRow(
                            vm,
                            Assets.Resources.SettingsView_Telemetry_ToolTip,
                            Assets.Resources.SettingsView_Telemetry_Label!,
                            x => x.SettingsService.Settings.TelemetryEnabled),
                        CreateToggleRow(
                            vm,
                            Assets.Resources.SettingsView_PreRelease_ToolTip,
                            Assets.Resources.SettingsView_PreRelease_Label!,
                            x => x.SettingsService.Settings.PreReleaseChannel),

                        CreateHeading(Assets.Resources.SettingsView_AppearanceHeading!),
                        CreateThemeRow(vm),
                        CreateNumericRow(
                            vm,
                            Assets.Resources.SettingsView_FontSize_ToolTip,
                            Assets.Resources.SettingsView_FontSize_Label!,
                            x => x.SettingsService.Appearance.FontSize,
                            AppearanceSettings.MinimumFontSize,
                            AppearanceSettings.MaximumFontSize),
                        CreateNumericRow(
                            vm,
                            Assets.Resources.SettingsView_ConsoleFontSize_ToolTip,
                            Assets.Resources.SettingsView_ConsoleFontSize_Label!,
                            x => x.SettingsService.Appearance.ConsoleFontSize),
                        CreateHeading(Assets.Resources.SettingsView_AIHeading),
                        CreateToggleRow(
                            vm,
                            Assets.Resources.SettingsView_McpEnabled_ToolTip,
                            Assets.Resources.SettingsView_McpEnabled_Label,
                            x => x.SettingsService.Settings.McpServerEnabled),
                        CreateNumericRow(
                            vm,
                            Assets.Resources.SettingsView_McpPort_ToolTip,
                            Assets.Resources.SettingsView_McpPort_Label,
                            x => x.McpServerPort,
                            1024,
                            65535)
                            .IsEnabled(vm, x => x.SettingsService.Settings.McpServerEnabled),
                        CreateAgentRow(vm),
                        CreateMcpClientGrants(vm)));
    }

    private static Border CreateHeading(string text)
    {
        return new Border()
            .MinHeight(28)
            .Padding(8, 0, 8, 0)
            .Margin(0, 6, 0, 6)
            .Background(new DynamicResourceExtension("SystemAltHighColor"))
            .Child(
                new TextBlock()
                    .Text(text)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Foreground(new DynamicResourceExtension("SystemBaseHighBrush"))
                    .FontWeight(FontWeight.Bold));
    }

    private static Grid CreateAgentRow(SettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(Assets.Resources.SettingsView_Agent_ToolTip)
            .IsEnabled(vm, x => x.SettingsService.Settings.McpServerEnabled)
            .Children(
                new Label()
                    .Content(Assets.Resources.SettingsView_Agent_Label)
                    .Col(0),
                new ComboBox()
                    .ItemsSource(vm, x => x.Agents)
                    .SelectedItem(vm, x => x.SelectedAgent, BindingMode.TwoWay)
                    .ItemTemplate(new FuncDataTemplate<IAgent>((agent, _) => new TextBlock().Text(agent!.Name)))
                    .Col(1));
    }

    private static ExpandableSection CreateMcpClientGrants(SettingsViewModel vm)
    {
        var tokenTextBox = new TextBox
        {
            IsReadOnly = true,
            TextWrapping = TextWrapping.NoWrap
        };
        AutomationProperties.SetName(tokenTextBox, Assets.Resources.SettingsView_McpToken_AccessibleName!);

        return new ExpandableSection()
            .Margin(8, 6, 0, 0)
            .IsEnabled(vm, x => x.SettingsService.Settings.McpServerEnabled)
            .Header(CreateHeading(Assets.Resources.SettingsView_McpClients_Heading!).Margin(0).Padding(0))
            .IsExpanded(true)
            .Content(
                new StackPanel()
                    .Spacing(8)
                    .Children(
                        new TextBlock()
                            .Text(Assets.Resources.SettingsView_McpServerUnavailable!)
                            .TextWrapping(TextWrapping.Wrap)
                            .IsVisible(vm, x => x.IsMcpServerUnavailable),
                        new StackPanel()
                            .Spacing(8)
                            .IsEnabled(vm, x => x.CanCreateExternalMcpCredential)
                            .Children(
                                CreateMcpSubheading(Assets.Resources.SettingsView_McpClients_CreateHeading!),
                                new TextBlock()
                                    .Text(Assets.Resources.SettingsView_McpClients_Description!)
                                    .TextWrapping(TextWrapping.Wrap),
                                CreateTextEntryRow(
                                    Assets.Resources.SettingsView_McpClientName_Label!,
                                    vm,
                                    x => x.ExternalMcpClientName,
                                    controlName: nameof(SettingsViewModel.ExternalMcpClientName),
                                    maximumLength: 80,
                                    watermark: Assets.Resources.SettingsView_McpClientName_Watermark),
                                CreateMcpClusterScopeEditor(vm),
                                new Button()
                                    .Content(Assets.Resources.SettingsView_McpClientCreate!)
                                    .Command(vm, x => x.CreateExternalMcpCredentialCommand)
                                    .HorizontalAlignment(HorizontalAlignment.Right),
                                new TextBlock()
                                    .Text(vm, x => x.McpGrantMessage)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .IsVisible(vm, x => x.HasMcpGrantMessage),
                                tokenTextBox
                                    .Text(vm, x => x.NewlyCreatedMcpToken, BindingMode.OneWay)
                                    .IsVisible(vm, x => x.HasNewlyCreatedMcpToken)),
                        CreateMcpSubheading(Assets.Resources.SettingsView_McpClients_ExistingHeading!)
                            .Margin(0, 8, 0, 0),
                        new TextBlock()
                            .Text(Assets.Resources.SettingsView_McpClients_Empty!)
                            .IsVisible(vm, x => x.HasNoExternalMcpGrants),
                        CreateMcpGrantList(vm)));
    }

    private static TextBlock CreateMcpSubheading(string text)
    {
        return new TextBlock()
            .Text(text)
            .FontSize(15)
            .FontWeight(FontWeight.Medium);
    }

    private static StackPanel CreateMcpClusterScopeEditor(SettingsViewModel vm)
    {
        return new StackPanel()
            .Spacing(3)
            .Children(
                new TextBlock()
                    .Text(Assets.Resources.SettingsView_McpClientClusters_Label!),
                new TextBlock()
                    .Text(Assets.Resources.SettingsView_McpClientClusters_Description!)
                    .TextWrapping(TextWrapping.Wrap),
                new ItemsControl()
                    .ItemsSource(vm, x => x.ExternalMcpClusterOptions)
                    .ItemTemplate(new FuncDataTemplate<McpExternalClusterOption>((option, _) =>
                    {
                        if (option is null)
                            return new CheckBox();

                        return new CheckBox()
                            .Content(option.Name)
                            .IsChecked(option, x => x.IsSelected, BindingMode.TwoWay);
                    })));
    }

    private static ListBox CreateMcpGrantList(SettingsViewModel vm)
    {
        return new ListBox()
            .ItemsSource(vm, x => x.ExternalMcpGrants)
            .ItemTemplate(new FuncDataTemplate<McpExternalClientGrant>((grant, _) =>
            {
                if (grant is null)
                    return new TextBlock();

                var clusterScope = grant.AllowedClusters.Count == 0
                    ? Assets.Resources.SettingsView_McpGrant_AllClusters
                    : string.Join(", ", grant.AllowedClusters);
                return new Grid()
                    .Cols("*,Auto")
                    .Children(
                        new TextBlock()
                            .Col(0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text($"{grant.Name} — {clusterScope}"),
                        new Button()
                            .Col(1)
                            .Content(Assets.Resources.SettingsView_McpClientRevoke!)
                            .Command(vm, x => x.RevokeExternalMcpCredentialCommand)
                            .CommandParameter(grant));
            }));
    }

    private static Grid CreateTextEntryRow(
        string label,
        SettingsViewModel vm,
        Expression<Func<SettingsViewModel, string>> getter,
        string? controlName = null,
        int? maximumLength = null,
        string? watermark = null)
    {
        var textBox = new TextBox().Text(vm, getter, BindingMode.TwoWay);
        if (controlName is not null)
            textBox.Name = controlName;
        if (maximumLength is { } length)
            textBox.MaxLength = length;
        textBox.PlaceholderText = watermark;

        return new Grid()
            .Cols("*,3*")
            .Children(
                new Label { Content = label, Target = textBox }.Col(0),
                textBox.Col(1));
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
                        Assets.Resources.SettingsView_Theme_Default,
                        LocalThemeVariant.Default),
                    CreateThemeChoice(
                        vm,
                        Assets.Resources.SettingsView_Theme_Light,
                        LocalThemeVariant.Light),
                    CreateThemeChoice(
                        vm,
                        Assets.Resources.SettingsView_Theme_Dark,
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

    private static Grid CreateNumericRow(
        SettingsViewModel vm,
        string toolTip,
        string label,
        Expression<Func<SettingsViewModel, decimal>> getter,
        decimal? minimum = null,
        decimal? maximum = null)
    {
        var control = new NumericUpDown()
            .Col(1)
            .Value(vm, getter, BindingMode.TwoWay);
        if (minimum is { } minimumValue)
            control.Minimum = minimumValue;
        if (maximum is { } maximumValue)
            control.Maximum = maximumValue;

        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(toolTip)
            .Children(
                new Label()
                    .Content(label)
                    .Col(0),
                control);
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
