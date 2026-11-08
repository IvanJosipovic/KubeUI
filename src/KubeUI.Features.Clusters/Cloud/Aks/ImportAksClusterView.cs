using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.Controls.Converters;
using KubeUI.Features.Azure;
using KubeUI.Features.Clusters.Presentation;


namespace KubeUI.Features.Clusters.Cloud.Aks;

public sealed class ImportAksClusterView : KubeUIView<ImportAksClusterViewModel>
{
    public ImportAksClusterView()
    {
        if (DesignTimePreview.IsDesignMode)
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
                    .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                    .Text(vm, x => x.Title),
                new TextBlock()
                    .Text(L10n.Instance.ImportAksClusterView_Description.ToBinding())
                    .TextWrapping(TextWrapping.Wrap),
                new TextBlock()
                    .FontStyle(FontStyle.Italic)
                    .Opacity(0.8)
                    .Text(L10n.Instance.ImportAksClusterView_Prerequisites.ToBinding())
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
                            .Text(L10n.Instance.ImportAksClusterView_Subscription.ToBinding()),
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
                                            .Opacity(0.65)
                                            .Text(subscription, x => x.SubscriptionId))),
                        new TextBlock()
                            .Row(1)
                            .Col(0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(L10n.Instance.ImportAksClusterView_Cluster.ToBinding()),
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
                                            .Opacity(0.65)
                                            .Text(cluster, x => x.ResourceGroupName, BindingMode.OneWay, Converters.StringFormat(L10n.Instance.ImportAksClusterView_ResourceGroupFormat.CurrentValue()!)),
                                        new TextBlock()
                                            .Opacity(0.65)
                                            .Text(cluster, x => x.KubernetesVersion, BindingMode.OneWay, Converters.StringFormat(L10n.Instance.ImportAksClusterView_VersionFormat.CurrentValue()!)))),
                        new StackPanel()
                            .Row(2)
                            .Col(1)
                            .Orientation(Orientation.Horizontal)
                            .Spacing(8)
                            .Children(
                                new Button()
                                    .Content(L10n.Instance.ImportAksClusterView_Refresh.ToBinding())
                                    .Command(vm, x => x.RefreshCommand),
                                new Button()
                                    .Content(L10n.Instance.ImportAksClusterView_Import.ToBinding())
                                    .Command(vm, x => x.ImportCommand))),
                new ScrollViewer()
                    .MaxHeight(140)
                    .Content(
                        new SelectableTextBlock()
                            .Text(vm, x => x.StatusMessage)
                            .TextWrapping(TextWrapping.Wrap)));
    }
}
