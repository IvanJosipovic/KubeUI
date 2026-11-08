using System.Linq.Expressions;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.Controls.Converters;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Foundation.Styles;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Settings;

public sealed class ClusterSettingsView : KubeUIView<ClusterSettingsViewModel>
{
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // The view model subscribes to the cluster runtime and to the persisted settings. Releasing it
        // here prevents the view model from outliving the settings document.
        (DataContext as IDisposable)?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    protected override object Build(ClusterSettingsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new ScrollViewer()
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
            .Content(
                new StackPanel()
                    .Children(
                        CreateClusterHeading(vm),
                        CreateNamespacesRow(vm),
                        CreatePermissionsRow(vm),
                        CreateDebugContainerImageRow(vm),
                        CreateMetricsSettings(vm)));
    }

    private static TextBlock CreateClusterHeading(ClusterSettingsViewModel vm)
    {
        return new TextBlock()
            .Margin(0, 0, 0, 12)
            .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
            .Text(vm, x => x.Cluster.Runtime.Name, BindingMode.OneWay, Converters.StringFormat(L10n.Instance.ClusterSettingsView_TitleFormat.CurrentValue()))
            .TextWrapping(TextWrapping.Wrap);
    }

    private static Grid CreateNamespacesRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .IsVisible(vm, x => !x.Cluster.Runtime.ListNamespaces)
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_ManualNamespacesTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_NamespacesLabel.CurrentValue())
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
                    .Content(L10n.Instance.ClusterSettingsView_Add.CurrentValue())
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
                            .Content(L10n.Instance.ClusterSettingsView_Remove.CurrentValue())
                            .Command(vm, x => x.RemoveNamespaceCommand)
                            .CommandParameter(ns)));
    }

    private static Grid CreatePermissionsRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_RefreshPermissionsTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_PermissionsLabel.CurrentValue())
                    .Col(0),
                new Button()
                    .Col(1)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .Content(L10n.Instance.ClusterSettingsView_RefreshPermissions.CurrentValue())
                    .IsEnabled(vm, x => !x.IsRefreshingPermissions, BindingMode.OneWay)
                    .Command(vm, x => x.RefreshPermissionsCommand));
    }

    private static Grid CreateDebugContainerImageRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_DebugContainerImageTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_DebugContainerImageLabel.CurrentValue())
                    .Col(0),
                new TextBox()
                    .Col(1)
                    .Text(vm, x => x.DebugContainerImage, BindingMode.TwoWay));
    }

    private static StackPanel CreateMetricsSettings(ClusterSettingsViewModel vm)
    {
        return new StackPanel()
            .Children(
                CreateHeading(L10n.Instance.ClusterSettingsView_MetricsHeading.CurrentValue()),
                CreateMetricsServiceRow(vm),
                CreateActiveMetricsServiceRow(vm),
                CreatePrometheusProviderRow(vm),
                CreateAzureMonitorSettings(vm),
                CreatePrometheusServiceNameRow(vm),
                CreatePrometheusServiceNamespaceRow(vm),
                CreatePrometheusServicePortRow(vm),
                CreatePrometheusDirectUrlRow(vm),
                CreatePrometheusPathPrefixRow(vm),
                CreatePrometheusHttpsRow(vm),
                CreatePrometheusBearerTokenRow(vm));
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

    private static Grid CreateMetricsServiceRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_MetricsServiceTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_MetricsServiceLabel.CurrentValue())
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
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_PrometheusProviderTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_PrometheusProviderLabel.CurrentValue())
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
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_ActiveMetricsServiceTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_ActiveMetricsServiceLabel.CurrentValue())
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
                            .Content(L10n.Instance.ClusterSettingsView_AzureMonitorSubscriptionLabel.CurrentValue())
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
                            .Content(L10n.Instance.ClusterSettingsView_AzureMonitorWorkspaceLabel.CurrentValue())
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
                            .Content(L10n.Instance.ClusterSettingsView_AzureMonitorRefresh.CurrentValue())
                            .Command(vm, x => x.RefreshAzureMonitorCommand)
                            .IsEnabled(vm, x => !x.IsAzureMonitorBusy)));
    }

    private static Grid CreatePrometheusServiceNameRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            L10n.Instance.ClusterSettingsView_PrometheusServiceNameLabel.CurrentValue(),
            L10n.Instance.ClusterSettingsView_PrometheusServiceNameTooltip.CurrentValue(),
            x => x.ClusterSettings.MetricsSettings.PrometheusServiceName,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusServiceNamespaceRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            L10n.Instance.ClusterSettingsView_PrometheusServiceNamespaceLabel.CurrentValue(),
            L10n.Instance.ClusterSettingsView_PrometheusServiceNamespaceTooltip.CurrentValue(),
            x => x.ClusterSettings.MetricsSettings.PrometheusServiceNamespace,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusServicePortRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            L10n.Instance.ClusterSettingsView_PrometheusServicePortLabel.CurrentValue(),
            L10n.Instance.ClusterSettingsView_PrometheusServicePortTooltip.CurrentValue(),
            x => x.ClusterSettings.MetricsSettings.PrometheusServicePort,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusDirectUrlRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            L10n.Instance.ClusterSettingsView_PrometheusDirectUrlLabel.CurrentValue(),
            L10n.Instance.ClusterSettingsView_PrometheusDirectUrlTooltip.CurrentValue(),
            x => x.ClusterSettings.MetricsSettings.PrometheusDirectUrl,
            x => x.ShowPrometheusDirectUrlSettings);
    }

    private static Grid CreatePrometheusPathPrefixRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            L10n.Instance.ClusterSettingsView_PrometheusPathPrefixLabel.CurrentValue(),
            L10n.Instance.ClusterSettingsView_PrometheusPathPrefixTooltip.CurrentValue(),
            x => x.ClusterSettings.MetricsSettings.PrometheusPathPrefix,
            x => x.ShowPrometheusCustomTransportSettings);
    }

    private static Grid CreatePrometheusBearerTokenRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            L10n.Instance.ClusterSettingsView_PrometheusBearerTokenLabel.CurrentValue(),
            L10n.Instance.ClusterSettingsView_PrometheusBearerTokenTooltip.CurrentValue(),
            x => x.ClusterSettings.MetricsSettings.PrometheusBearerToken,
            x => x.ShowPrometheusCustomTransportSettings);
    }

    private static Grid CreatePrometheusTextRow<TValue>(
        ClusterSettingsViewModel vm,
        string label,
        string tooltip,
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
            .IsVisible(vm, x => x.ShowPrometheusCustomTransportSettings)
            .ToolTip_Tip(L10n.Instance.ClusterSettingsView_PrometheusUseHttpsTooltip.CurrentValue())
            .Children(
                new Label()
                    .Content(L10n.Instance.ClusterSettingsView_PrometheusUseHttpsLabel.CurrentValue())
                    .Col(0),
                new CheckBox()
                    .Col(1)
                    .IsChecked(vm, x => x.ClusterSettings.MetricsSettings.PrometheusUseHttps, BindingMode.TwoWay));
    }
}
