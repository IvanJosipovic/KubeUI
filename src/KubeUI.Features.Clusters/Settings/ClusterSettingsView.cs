using Avalonia.Markup.Xaml.MarkupExtensions;

namespace KubeUI.Features.Clusters.Settings;

public sealed class ClusterSettingsView : KubeUIView<ClusterSettingsViewModel>
{
    public ClusterSettingsView()
    {
    }

    protected override object Build(ClusterSettingsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Margin(10, 0, 0, 0)
            .Children(
                new TextBlock()
                    .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                    .Text(vm, x => x.ClusterTitle, BindingMode.OneWay),
                CreateNamespacesRow(vm),
                CreatePermissionsRow(vm),
                CreateApiDiscoveryRow(vm),
                CreateDebugContainerImageRow(vm));
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
}
