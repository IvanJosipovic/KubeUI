using System.Linq.Expressions;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Controls.Templates;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Styles;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Settings;

public sealed class ClusterSettingsView : ViewBase<ClusterSettingsViewModel>
{
    public ClusterSettingsView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<ClusterSettingsViewModel>();
        }
    }

    protected override object Build(ClusterSettingsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Margin(10, 0, 0, 0)
            .Spacing(8)
            .Children(
                new TextBlock()
                    .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                    .Text(vm, x => x.Cluster.Runtime.Name, BindingMode.OneWay, Converters.Converters.StringFormat(Assets.Resources.ClusterSettingsView_TitleFormat)),
                CreateNamespacesRow(vm),
                CreateDebugContainerImageRow(vm),
                CreateMetricsSettings(vm));
    }

    private static Grid CreateNamespacesRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .IsVisible(vm, x => !x.Cluster.Runtime.ListNamespaces)
            .ToolTip_Tip(Assets.Resources.ClusterSettingsView_ManualNamespacesTooltip)
            .Children(
                new Label()
                    .Content(Assets.Resources.ClusterSettingsView_NamespacesLabel)
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
                    .Content(Assets.Resources.ClusterSettingsView_Add)
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
                            .Content(Assets.Resources.ClusterSettingsView_Remove)
                            .Command(vm, x => x.RemoveNamespaceCommand)
                            .CommandParameter(ns)));
    }

    private static Grid CreateDebugContainerImageRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(Assets.Resources.ClusterSettingsView_DebugContainerImageTooltip)
            .Children(
                new Label()
                    .Content(Assets.Resources.ClusterSettingsView_DebugContainerImageLabel)
                    .Col(0),
                new TextBox()
                    .Col(1)
                    .Text(vm, x => x.DebugContainerImage, BindingMode.TwoWay));
    }

    private static StackPanel CreateMetricsSettings(ClusterSettingsViewModel vm)
    {
        return new StackPanel()
            .Spacing(8)
            .Children(
                new TextBlock()
                    .FontWeight(FontWeight.Bold)
                    .Text(Assets.Resources.ClusterSettingsView_MetricsHeading),
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

    private static Grid CreateMetricsServiceRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .ToolTip_Tip(Assets.Resources.ClusterSettingsView_MetricsServiceTooltip)
            .Children(
                new Label()
                    .Content(Assets.Resources.ClusterSettingsView_MetricsServiceLabel)
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
            .ToolTip_Tip(Assets.Resources.ClusterSettingsView_PrometheusProviderTooltip)
            .Children(
                new Label()
                    .Content(Assets.Resources.ClusterSettingsView_PrometheusProviderLabel)
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
            .ToolTip_Tip(Assets.Resources.ClusterSettingsView_ActiveMetricsServiceTooltip)
            .Children(
                new Label()
                    .Content(Assets.Resources.ClusterSettingsView_ActiveMetricsServiceLabel)
                    .Col(0),
                new TextBlock()
                    .Col(1)
                    .Text(vm, x => x.ActiveMetricsServiceText));
    }

    private static StackPanel CreateAzureMonitorSettings(ClusterSettingsViewModel vm)
    {
        return new StackPanel()
            .Spacing(6)
            .IsVisible(vm, x => x.ShowAzureMonitorSettings)
            .Children(
                new TextBlock()
                    .Text(vm, x => x.AzureMonitorStatusText),
                new Button()
                    .Content(Assets.Resources.ClusterSettingsView_AzureMonitorRefresh)
                    .Command(vm, x => x.RefreshAzureMonitorCommand)
                    .IsEnabled(vm, x => !x.IsAzureMonitorBusy),
                new Grid()
                    .Cols("*,2*")
                    .Children(
                        new Label()
                            .Content(Assets.Resources.ClusterSettingsView_AzureMonitorSubscriptionLabel)
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
                            .Content(Assets.Resources.ClusterSettingsView_AzureMonitorWorkspaceLabel)
                            .Col(0),
                        new ComboBox()
                            .Col(1)
                            .ItemsSource(vm, x => x.AzureMonitorWorkspaces)
                            .SelectedItem(vm, x => x.SelectedAzureMonitorWorkspace, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<AzureMonitorWorkspaceInfo>((workspace, _) =>
                                new TextBlock().Text(workspace is null ? string.Empty : $"{workspace.Name} ({workspace.Location})")))));
    }

    private static Grid CreatePrometheusServiceNameRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            Assets.Resources.ClusterSettingsView_PrometheusServiceNameLabel,
            Assets.Resources.ClusterSettingsView_PrometheusServiceNameTooltip,
            x => x.ClusterSettings.PrometheusServiceName,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusServiceNamespaceRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            Assets.Resources.ClusterSettingsView_PrometheusServiceNamespaceLabel,
            Assets.Resources.ClusterSettingsView_PrometheusServiceNamespaceTooltip,
            x => x.ClusterSettings.PrometheusServiceNamespace,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusServicePortRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            Assets.Resources.ClusterSettingsView_PrometheusServicePortLabel,
            Assets.Resources.ClusterSettingsView_PrometheusServicePortTooltip,
            x => x.ClusterSettings.PrometheusServicePort,
            x => x.ShowPrometheusServiceSettings);
    }

    private static Grid CreatePrometheusDirectUrlRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            Assets.Resources.ClusterSettingsView_PrometheusDirectUrlLabel,
            Assets.Resources.ClusterSettingsView_PrometheusDirectUrlTooltip,
            x => x.ClusterSettings.PrometheusDirectUrl,
            x => x.ShowPrometheusDirectUrlSettings);
    }

    private static Grid CreatePrometheusPathPrefixRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            Assets.Resources.ClusterSettingsView_PrometheusPathPrefixLabel,
            Assets.Resources.ClusterSettingsView_PrometheusPathPrefixTooltip,
            x => x.ClusterSettings.PrometheusPathPrefix,
            x => x.ShowPrometheusCustomTransportSettings);
    }

    private static Grid CreatePrometheusBearerTokenRow(ClusterSettingsViewModel vm)
    {
        return CreatePrometheusTextRow(
            vm,
            Assets.Resources.ClusterSettingsView_PrometheusBearerTokenLabel,
            Assets.Resources.ClusterSettingsView_PrometheusBearerTokenTooltip,
            x => x.ClusterSettings.PrometheusBearerToken,
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
            .ToolTip_Tip(Assets.Resources.ClusterSettingsView_PrometheusUseHttpsTooltip)
            .Children(
                new Label()
                    .Content(Assets.Resources.ClusterSettingsView_PrometheusUseHttpsLabel)
                    .Col(0),
                new CheckBox()
                    .Col(1)
                    .IsChecked(vm, x => x.ClusterSettings.PrometheusUseHttps, BindingMode.TwoWay));
    }
}
