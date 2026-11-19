using System.Linq.Expressions;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.Controls.Presentation;
using KubeUI.Foundation.Styles;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Settings;

public sealed class ClusterSettingsView : KubeUIView<ClusterSettingsViewModel>
{
    protected override object Build(ClusterSettingsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new ScrollViewer()
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
            .Content(
                new StackPanel()
                    .Margin(10, 0, 0, 0)
                    .Children(
                        new TextBlock()
                            .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                            .Text(vm, x => x.ClusterTitle, BindingMode.OneWay),
                        CreateNamespacesRow(vm),
                        CreatePermissionsRow(vm),
                        CreateApiDiscoveryRow(vm),
                        CreateDebugContainerImageRow(vm),
                        CreateMetricsSettings(vm)));
    }

    private static Grid CreateNamespacesRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .IsVisible(vm, x => !x.Cluster.Runtime.ListNamespaces)
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_ManualNamespacesTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_NamespacesLabel.ToBinding())
                    .Col(0),
                new StackPanel()
                    .Col(1)
                    .Children(
                        CreateNamespaceEditor(vm),
                        CreateNamespacesList(vm)));
    }

    private static Grid CreateNamespaceEditor(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,Auto")
            .Children(
                new TextBox()
                    .Col(0)
                    .Text(vm, x => x.Namespace, BindingMode.TwoWay),
                new Button()
                    .Col(1)
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_Add.ToBinding())
                    .Command(vm, x => x.AddNamespaceCommand));
    }

    private static Control CreateNamespacesList(ClusterSettingsViewModel vm)
    {
        return new ListBox()
            .ItemsSource(vm, x => x.ClusterSettings.Namespaces)
            .ItemTemplate<string>(ns =>
                new Grid()
                    .Cols("*,Auto")
                    .Children(
                        new TextBlock()
                            .Col(0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(ns),
                        new Button()
                            .Col(1)
                            .Content(ClustersLanguageManager.Instance.ClusterSettingsView_Remove.ToBinding())
                            .Command(vm, x => x.RemoveNamespaceCommand)
                            .CommandParameter(ns)));
    }

    private static Grid CreatePermissionsRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_RefreshPermissionsTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_PermissionsLabel.ToBinding())
                    .Col(0),
                new Button()
                    .Col(1)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_RefreshPermissions.ToBinding())
                    .IsEnabled(vm, x => x.CanRefreshPermissions, BindingMode.OneWay)
                    .Command(vm, x => x.RefreshPermissionsCommand));
    }

    private static Grid CreateApiDiscoveryRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_RefreshApiDiscoveryTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_ApiDiscoveryLabel.ToBinding())
                    .Col(0),
                new Button()
                    .Col(1)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_RefreshApiDiscovery.ToBinding())
                    .IsEnabled(vm, x => x.CanRefreshApiDiscovery, BindingMode.OneWay)
                    .Command(vm, x => x.RefreshApiDiscoveryCommand));
    }

    private static Grid CreateDebugContainerImageRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_DebugContainerImageTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_DebugContainerImageLabel.ToBinding())
                    .Col(0),
                new TextBox()
                    .Col(1)
                    .Text(vm, x => x.DebugContainerImage, BindingMode.TwoWay));
    }

    private static StackPanel CreateMetricsSettings(ClusterSettingsViewModel vm)
    {
        return new StackPanel()
            .Children(
                CreateHeading(ClustersLanguageManager.Instance.ClusterSettingsView_MetricsHeading.ToBinding()),
                CreateMetricsServiceRow(vm),
                CreateActiveMetricsServiceRow(vm),
                CreatePrometheusProviderRow(vm),
                CreateAzureMonitorSettings(vm),
                CreatePrometheusEndpointSettings(vm),
                CreatePrometheusAuthenticationSettings(vm));
    }

    private static StackPanel CreatePrometheusEndpointSettings(ClusterSettingsViewModel vm)
    {
        return new StackPanel()
            .Spacing(6)
            .Children(
                new ExpandableSection()
                    .Header(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusPortForwardSection.ToBinding())
                    .IsVisible(vm, x => x.ShowPrometheusServiceSettings)
                    .Content(
                        new StackPanel()
                            .Spacing(6)
                            .Children(
                                new TextBlock()
                                    .Text(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusPortForwardDescription.ToBinding())
                                    .TextWrapping(TextWrapping.Wrap),
                                CreatePrometheusServiceNameRow(vm),
                                CreatePrometheusServiceNamespaceRow(vm),
                                CreatePrometheusServicePortRow(vm),
                                CreatePrometheusHttpsRow(vm))),
                new ExpandableSection()
                    .Header(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusDirectUrlSection.ToBinding())
                    .IsVisible(vm, x => x.ShowPrometheusDirectUrlSettings)
                    .Content(
                        new StackPanel()
                            .Spacing(6)
                            .Children(
                                new TextBlock()
                                    .Text(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusDirectUrlDescription.ToBinding())
                                    .TextWrapping(TextWrapping.Wrap),
                                CreatePrometheusDirectUrlRow(vm))),
                CreatePrometheusPathPrefixRow(vm));
    }

    private static Border CreateHeading(BindingBase text)
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

    private static Grid CreateMetricsServiceRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServiceTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServiceLabel.ToBinding())
                    .Col(0),
                new ComboBox()
                    .Col(1)
                    .ItemsSource(vm, x => x.MetricsServiceOptions)
                    .SelectedItem(vm, x => x.SelectedMetricsService, BindingMode.TwoWay)
                    .ItemTemplate(new FuncDataTemplate<MetricsServiceOption>((option, _) => new TextBlock().Text(option.Label))));
    }

    private static Grid CreatePrometheusProviderRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .IsVisible(vm, x => x.ShowPrometheusSettings)
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderLabel.ToBinding())
                    .Col(0),
                new ComboBox()
                    .Col(1)
                    .ItemsSource(vm, x => x.PrometheusProviderOptions)
                    .SelectedItem(vm, x => x.SelectedPrometheusProvider, BindingMode.TwoWay)
                    .ItemTemplate(new FuncDataTemplate<PrometheusProviderOption>((option, _) => new TextBlock().Text(option.Label))));
    }

    private static Grid CreateActiveMetricsServiceRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServiceTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServiceLabel.ToBinding())
                    .Col(0),
                new TextBlock()
                    .Col(1)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(vm, x => x.ActiveMetricsServiceText));
    }

    private static StackPanel CreateAzureMonitorSettings(ClusterSettingsViewModel vm)
    {
        return new StackPanel()
            .Spacing(6)
            .IsVisible(vm, x => x.ShowAzureMonitorSettings)
            .Children(
                new Grid()
                    .Cols("*,2*")
                    .Children(
                        new Label()
                            .Content(ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorSubscriptionLabel.ToBinding())
                            .Col(0),
                        new ComboBox()
                            .Col(1)
                            .ItemsSource(vm, x => x.AzureMonitorSubscriptions)
                            .SelectedItem(vm, x => x.SelectedAzureMonitorSubscription, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<AzureMonitorSubscriptionInfo>((subscription, _) => new TextBlock().Text(subscription?.DisplayName ?? string.Empty)))),
                new Grid()
                    .Cols("*,2*")
                    .Children(
                        new Label()
                            .Content(ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorWorkspaceLabel.ToBinding())
                            .Col(0),
                        new ComboBox()
                            .Col(1)
                            .ItemsSource(vm, x => x.AzureMonitorWorkspaces)
                            .SelectedItem(vm, x => x.SelectedAzureMonitorWorkspace, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<AzureMonitorWorkspaceInfo>((workspace, _) =>
                                new TextBlock().Text(workspace is null ? string.Empty : $"{workspace.Name} ({workspace.Location})")))),
                new Grid()
                    .Cols("*,2*")
                    .Children(
                        new TextBlock()
                            .Col(1)
                            .TextWrapping(TextWrapping.Wrap)
                            .Text(vm, x => x.AzureMonitorStatusText)),
                new Grid()
                    .Cols("*,2*")
                    .Children(
                        new Button()
                            .Col(1)
                            .HorizontalAlignment(HorizontalAlignment.Left)
                            .Content(ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorRefresh.ToBinding())
                            .Command(vm, x => x.RefreshAzureMonitorCommand)
                            .IsEnabled(vm, x => !x.IsAzureMonitorBusy)));
    }

    private static Grid CreatePrometheusServiceNameRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusServiceNameLabel.ToBinding(),
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusServiceNameTooltip.ToBinding(),
            x => x.ClusterSettings.MetricsSettings.PrometheusServiceName,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusServiceNamespaceRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusServiceNamespaceLabel.ToBinding(),
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusServiceNamespaceTooltip.ToBinding(),
            x => x.ClusterSettings.MetricsSettings.PrometheusServiceNamespace,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusServicePortRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusServicePortLabel.ToBinding(),
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusServicePortTooltip.ToBinding(),
            x => x.ClusterSettings.MetricsSettings.PrometheusServicePort,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusDirectUrlRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusDirectUrlLabel.ToBinding(),
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusDirectUrlTooltip.ToBinding(),
            x => x.ClusterSettings.MetricsSettings.PrometheusDirectUrl,
            x => x.ShowPrometheusDirectUrlSettings);
    }

    private static Grid CreatePrometheusPathPrefixRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusPathPrefixLabel.ToBinding(),
            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusPathPrefixTooltip.ToBinding(),
            x => x.ClusterSettings.MetricsSettings.PrometheusPathPrefix,
            x => x.ShowPrometheusCustomTransportSettings);
    }

    private static ExpandableSection CreatePrometheusAuthenticationSettings(ClusterSettingsViewModel vm)
    {
        return new ExpandableSection()
            .Header(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusAuthenticationSection.ToBinding())
            .IsVisible(vm, x => x.ShowPrometheusCustomTransportSettings)
            .Content(
                new StackPanel()
                    .Spacing(6)
                    .Children(
                        new TextBlock()
                            .Text(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusAuthenticationNotice.ToBinding())
                            .TextWrapping(TextWrapping.Wrap),
                        new TextBlock()
                            .Text(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusCustomHeadersLabel.ToBinding())
                            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusCustomHeadersTooltip.ToBinding()),
                        new ItemsControl()
                            .ItemsSource(vm, x => x.ClusterSettings.MetricsSettings.PrometheusCustomHeaders)
                            .ItemTemplate(new FuncDataTemplate<PrometheusHeader>((header, _) =>
                            {
                                if (header is null)
                                {
                                    return new TextBlock();
                                }

                                return new Grid()
                                    .Cols("*,2*,Auto")
                                    .Children(
                                        new TextBox()
                                            .Col(0)
                                            .PlaceholderText(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusHeaderNamePlaceholder.ToBinding())
                                            .Text(header, x => x.Name, BindingMode.TwoWay),
                                        new TextBox()
                                            .Col(1)
                                            .PlaceholderText(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusHeaderValuePlaceholder.ToBinding())
                                            .PasswordChar('●')
                                            .Text(header, x => x.Value, BindingMode.TwoWay),
                                        new Button()
                                            .Col(2)
                                            .Content(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusRemoveHeader.ToBinding())
                                            .Command(vm, x => x.RemovePrometheusHeaderCommand)
                                            .CommandParameter(header));
                            })),
                        new Button()
                            .HorizontalAlignment(HorizontalAlignment.Left)
                            .Content(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusAddHeader.ToBinding())
                            .Command(vm, x => x.AddPrometheusHeaderCommand),
                        CreatePrometheusTextRow(
                            vm,
                            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusBasicAuthUsernameLabel.ToBinding(),
                            ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusBasicAuthTooltip.ToBinding(),
                            x => x.ClusterSettings.MetricsSettings.PrometheusBasicAuthUsername,
                            x => x.ShowPrometheusCustomTransportSettings),
                        new Grid()
                            .Cols("*,2*")
                            .IsVisible(vm, x => x.ShowPrometheusCustomTransportSettings)
                            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusBasicAuthTooltip.ToBinding())
                            .Children(
                                new Label()
                                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusBasicAuthPasswordLabel.ToBinding())
                                    .Col(0),
                                new TextBox()
                                    .Col(1)
                                    .PasswordChar('●')
                                    .Text(vm, x => x.ClusterSettings.MetricsSettings.PrometheusBasicAuthPassword, BindingMode.TwoWay)),
                        new TextBlock()
                            .IsVisible(vm, x => x.ShowPrometheusAuthenticationConflictWarning)
                            .Text(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusAuthenticationConflictWarning.ToBinding())
                            .TextWrapping(TextWrapping.Wrap),
                        new TextBlock()
                            .IsVisible(vm, x => x.ShowInsecureAuthenticationWarning)
                            .Text(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusInsecureAuthenticationWarning.ToBinding())
                            .TextWrapping(TextWrapping.Wrap)));
    }

    private static Grid CreatePrometheusTextRow<TValue>(
        ClusterSettingsViewModel vm,
        BindingBase label,
        BindingBase tooltip,
        Expression<Func<ClusterSettingsViewModel, TValue>> getter,
        Expression<Func<ClusterSettingsViewModel, bool>> visibility)
    {
        return new Grid()
            .Cols("*,2*")
            .IsVisible(vm, visibility)
            .ToolTip_Tip(tooltip)
            .Children(
                new Label()
                    .Content(label)
                    .Col(0),
                new TextBox()
                    .Col(1)
                    .Text(vm, getter, BindingMode.TwoWay));
    }

    private static Grid CreatePrometheusHttpsRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .IsVisible(vm, x => x.ShowPrometheusServiceSettings)
            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusUseHttpsTooltip.ToBinding())
            .Children(
                new Label()
                    .Content(ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusUseHttpsLabel.ToBinding())
                    .Col(0),
                new CheckBox()
                    .Col(1)
                    .IsChecked(vm, x => x.ClusterSettings.MetricsSettings.PrometheusUseHttps, BindingMode.TwoWay));
    }
}
