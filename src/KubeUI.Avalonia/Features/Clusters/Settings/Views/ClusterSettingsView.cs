using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using TextMateSharp.Internal.Rules;

namespace KubeUI.Avalonia.Features.Clusters.Settings.Views;

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
            .Children(
                new TextBlock()
                    .FontSize(25)
                    .Text(vm, x => x.Cluster.Name, BindingMode.OneWay, Converters.Converters.StringFormat(Assets.Resources.ClusterSettingsView_TitleFormat)),
                CreateNamespacesRow(vm),
                CreateDebugContainerImageRow(vm));
    }

    private static Grid CreateNamespacesRow(ClusterSettingsViewModel vm)
    {
        return new Grid()
            .Cols("*,2*")
            .IsEnabled(vm, x => !x.Cluster.ListNamespaces)
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
}
