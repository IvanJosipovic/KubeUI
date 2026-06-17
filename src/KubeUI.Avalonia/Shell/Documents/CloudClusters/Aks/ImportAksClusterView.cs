using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks;

public sealed class ImportAksClusterView : ViewBase<ImportAksClusterViewModel>
{
    public ImportAksClusterView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<ImportAksClusterViewModel>();
        }
    }

    protected override object Build(ImportAksClusterViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Spacing(12)
            .Children(
                new TextBlock()
                    .FontSize(24)
                    .Text(vm, x => x.Title),
                new TextBlock()
                    .Text(Assets.Resources.ImportAksClusterView_Description)
                    .TextWrapping(TextWrapping.Wrap),
                new TextBlock()
                    .FontStyle(FontStyle.Italic)
                    .Opacity(0.8)
                    .Text(Assets.Resources.ImportAksClusterView_Prerequisites)
                    .TextWrapping(TextWrapping.Wrap),
                new Border()
                    .Padding(12)
                    .Background(Brushes.Transparent)
                    .CornerRadius(8)
                    .Child(
                        new StackPanel()
                            .Spacing(8)
                            .Children(
                                new TextBlock()
                                    .Text(vm, x => x.AuthenticationStatusMessage)
                                    .TextWrapping(TextWrapping.Wrap))),
                new Grid()
                    .Cols("Auto,*")
                    .ColumnSpacing(12)
                    .Rows("Auto,Auto,Auto,Auto")
                    .RowSpacing(8)
                    .Children(
                        new TextBlock()
                            .Row(0)
                            .Col(0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(Assets.Resources.ImportAksClusterView_Subscription),
                        new ComboBox()
                            .Row(0)
                            .Col(1)
                            .Width(420)
                            .ItemsSource(vm, x => x.Subscriptions)
                            .SelectedItem(vm, x => x.SelectedSubscription, BindingMode.TwoWay)
                            .ItemTemplate<AksSubscriptionInfo>(subscription =>
                                new StackPanel()
                                    .Children(
                                        new TextBlock()
                                            .Text(subscription, x => x.DisplayName),
                                        new TextBlock()
                                            .FontSize(11)
                                            .Opacity(0.65)
                                            .Text(subscription, x => x.SubscriptionId))),
                        new TextBlock()
                            .Row(1)
                            .Col(0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(Assets.Resources.ImportAksClusterView_Cluster),
                        new ListBox()
                            .Row(1)
                            .Col(1)
                            .Height(220)
                            .ItemsSource(vm, x => x.Clusters)
                            .SelectedItem(vm, x => x.SelectedCluster, BindingMode.TwoWay)
                            .ItemTemplate<AksClusterInfo>(cluster =>
                                new StackPanel()
                                    .Spacing(2)
                                    .Children(
                                        new TextBlock()
                                            .Text(cluster, x => x.Name),
                                        new TextBlock()
                                            .FontSize(11)
                                            .Opacity(0.65)
                                            .Text(cluster, x => x.ResourceGroupName, BindingMode.OneWay, Converters.Converters.StringFormat(Assets.Resources.ImportAksClusterView_ResourceGroupFormat)),
                                        new TextBlock()
                                            .FontSize(11)
                                            .Opacity(0.65)
                                            .Text(cluster, x => x.KubernetesVersion, BindingMode.OneWay, Converters.Converters.StringFormat(Assets.Resources.ImportAksClusterView_VersionFormat)))),
                        new StackPanel()
                            .Row(2)
                            .Col(1)
                            .Orientation(Orientation.Horizontal)
                            .Spacing(8)
                            .Children(
                                new Button()
                                    .Content(Assets.Resources.ImportAksClusterView_Refresh)
                                    .Command(vm, x => x.RefreshCommand),
                                new Button()
                                    .Content(Assets.Resources.ImportAksClusterView_Import)
                                    .Command(vm, x => x.ImportCommand))),
                new ScrollViewer()
                    .MaxHeight(140)
                    .Content(
                        new SelectableTextBlock()
                            .Text(vm, x => x.StatusMessage)
                            .TextWrapping(TextWrapping.Wrap)));
    }
}
