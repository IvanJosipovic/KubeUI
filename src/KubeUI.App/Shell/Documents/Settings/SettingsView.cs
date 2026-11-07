using System.Globalization;
using System.Linq.Expressions;
using Avalonia.Automation;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Options;
using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Mcp;
using KubeUI.Features.Clusters.Presentation;
using KubeUI.Controls.Presentation;


namespace KubeUI.App.Shell.Documents.Settings;

public sealed class SettingsView() : KubeUIView<SettingsViewModel>
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
                        CreateHeading(AppLanguageManager.Instance.SettingsView_ApplicationHeading.CurrentValue()!),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_Logging_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_Logging_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.LoggingEnabled),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_Telemetry_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_Telemetry_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.TelemetryEnabled),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_PreRelease_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_PreRelease_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.PreReleaseChannel),

                        CreateHeading(AppLanguageManager.Instance.SettingsView_ClustersHeading.CurrentValue()!),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_AutoDiscovery_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_AutoDiscovery_Label.CurrentValue()!,
                            x => x.KubeConfigAutoDiscoveryEnabled),
                        CreateHeading(AppLanguageManager.Instance.SettingsView_AppearanceHeading.CurrentValue()!),
                        CreateThemeRow(vm),
                        CreateNumericRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_FontSize_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_FontSize_Label.CurrentValue()!,
                            x => x.SettingsService.Appearance.FontSize,
                            AppearanceSettings.MinimumFontSize,
                            AppearanceSettings.MaximumFontSize),
                        CreateNumericRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_ConsoleFontSize_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_ConsoleFontSize_Label.CurrentValue()!,
                            x => x.SettingsService.Appearance.ConsoleFontSize),
                        CreateHeading(AppLanguageManager.Instance.SettingsView_AIHeading.CurrentValue()!),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_McpEnabled_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_McpEnabled_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.McpServerEnabled),
                        CreateNumericRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_McpPort_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_McpPort_Label.CurrentValue()!,
                            x => x.McpServerPort,
                            1024,
                            65535)
                            .IsEnabled(vm, x => x.SettingsService.Settings.McpServerEnabled),
                        CreateAgentRow(vm),
                        CreateMcpClientGrants(vm),

                        CreateHeading(AppLanguageManager.Instance.SettingsView_FeaturesHeading.CurrentValue()!),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_HelmFeature_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_HelmFeature_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.PreviewFeatures.Helm),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_HubbleFeature_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_HubbleFeature_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.PreviewFeatures.Hubble),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_DisableJsonSerializationReflection_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_DisableJsonSerializationReflection_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.PreviewFeatures.DisableJsonSerializationReflection),
                        CreateToggleRow(
                            vm,
                            AppLanguageManager.Instance.SettingsView_DisableYamlSerializationReflection_ToolTip.CurrentValue()!,
                            AppLanguageManager.Instance.SettingsView_DisableYamlSerializationReflection_Label.CurrentValue()!,
                            x => x.SettingsService.Settings.PreviewFeatures.DisableYamlSerializationReflection)));
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
            .ToolTip_Tip(AppLanguageManager.Instance.SettingsView_Agent_ToolTip.ToBinding())
            .IsEnabled(vm, x => x.SettingsService.Settings.McpServerEnabled)
            .Children(
                new Label()
                    .Content(AppLanguageManager.Instance.SettingsView_Agent_Label.ToBinding())
                    .Col(0),
                new ComboBox()
                    .ItemsSource(vm, x => x.Agents)
                    .SelectedItem(vm, x => x.SelectedAgent, BindingMode.TwoWay)
                    .ItemTemplate(new FuncDataTemplate<IAgent>((agent, _) => new TextBlock().Text(agent?.Name ?? string.Empty)))
                    .Col(1));
    }

    private static ExpandableSection CreateMcpClientGrants(SettingsViewModel vm)
    {
        var tokenTextBox = new TextBox
        {
            IsReadOnly = true,
            TextWrapping = TextWrapping.NoWrap
        };
        AutomationProperties.SetName(tokenTextBox, AppLanguageManager.Instance.SettingsView_McpToken_AccessibleName.CurrentValue()!);

        return new ExpandableSection()
            .Margin(8, 6, 0, 0)
            .IsEnabled(vm, x => x.SettingsService.Settings.McpServerEnabled)
            .Header(CreateHeading(AppLanguageManager.Instance.SettingsView_McpClients_Heading.CurrentValue()!).Margin(0).Padding(0))
            .IsExpanded(true)
            .Content(
                new StackPanel()
                    .Spacing(8)
                    .Children(
                        new TextBlock()
                            .Text(AppLanguageManager.Instance.SettingsView_McpServerUnavailable.ToBinding())
                            .TextWrapping(TextWrapping.Wrap)
                            .IsVisible(vm, x => x.IsMcpServerUnavailable),
                        new StackPanel()
                            .Spacing(8)
                            .IsEnabled(vm, x => x.CanCreateExternalMcpCredential)
                            .Children(
                                CreateMcpSubheading(AppLanguageManager.Instance.SettingsView_McpClients_CreateHeading.CurrentValue()!),
                                new TextBlock()
                                    .Text(AppLanguageManager.Instance.SettingsView_McpClients_Description.ToBinding())
                                    .TextWrapping(TextWrapping.Wrap),
                                CreateTextEntryRow(
                                    AppLanguageManager.Instance.SettingsView_McpClientName_Label.CurrentValue()!,
                                    vm,
                                    x => x.ExternalMcpClientName,
                                    controlName: nameof(SettingsViewModel.ExternalMcpClientName),
                                    maximumLength: 80,
                                    watermark: AppLanguageManager.Instance.SettingsView_McpClientName_Watermark.CurrentValue()!),
                                CreateMcpClusterScopeEditor(vm),
                                new Button()
                                    .Content(AppLanguageManager.Instance.SettingsView_McpClientCreate.ToBinding())
                                    .Command(vm, x => x.CreateExternalMcpCredentialCommand)
                                    .HorizontalAlignment(HorizontalAlignment.Right),
                                new TextBlock()
                                    .Text(vm, x => x.McpGrantMessage)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .IsVisible(vm, x => x.HasMcpGrantMessage),
                                tokenTextBox
                                    .Text(vm, x => x.NewlyCreatedMcpToken, BindingMode.OneWay)
                                    .IsVisible(vm, x => x.HasNewlyCreatedMcpToken)),
                        CreateMcpSubheading(AppLanguageManager.Instance.SettingsView_McpClients_ExistingHeading.CurrentValue()!)
                            .Margin(0, 8, 0, 0),
                        new TextBlock()
                            .Text(AppLanguageManager.Instance.SettingsView_McpClients_Empty.ToBinding())
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
                    .Text(AppLanguageManager.Instance.SettingsView_McpClientClusters_Label.ToBinding()),
                new TextBlock()
                    .Text(AppLanguageManager.Instance.SettingsView_McpClientClusters_Description.ToBinding())
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
                    ? AppLanguageManager.Instance.SettingsView_McpGrant_AllClusters.CurrentValue()!
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
                            .Content(AppLanguageManager.Instance.SettingsView_McpClientRevoke.ToBinding())
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
                    .Content(AppLanguageManager.Instance.SettingsView_Theme.ToBinding())
                    .Col(0),
                new StackPanel().Children(
                    CreateThemeChoice(
                        vm,
                        AppLanguageManager.Instance.SettingsView_Theme_Default.CurrentValue()!,
                        LocalThemeVariant.Default),
                    CreateThemeChoice(
                        vm,
                        AppLanguageManager.Instance.SettingsView_Theme_Light.CurrentValue()!,
                        LocalThemeVariant.Light),
                    CreateThemeChoice(
                        vm,
                        AppLanguageManager.Instance.SettingsView_Theme_Dark.CurrentValue()!,
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
