using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using k8s.Models;
using NodeEditor.Controls;
using Ursa.Controls;

namespace KubeUI.Views;

public sealed class VisualizationView : MyViewBase<VisualizationViewModel>
{
    protected override StyleGroup? BuildStyles() => [
           new Style(x => x.Is<Pin>())
                .Setter(Pin.ContextFlyoutProperty, new MenuFlyout())
    ];

    protected override object Build(VisualizationViewModel? vm) =>
       new Grid()
            .Rows("Auto, *")
            .Children([
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .Children([
                        new Label()
                            .Col(1)
                            .Width(200)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Content(@vm.Drawing.Nodes.Count, null, new FuncValueConverter<int, string>((x) => string.Format("Items: {0}", x))),
                    ]),
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Children([
                        new MultiComboBox()
                            .Col(1)
                            .Width(300)
                            .MaxHeight(20)
                            .Classes("ClearButton")
                            .ItemsSource(@vm.Cluster.Namespaces.Values)
                            .SelectedItems(@vm.Cluster.SelectedNamespaces)
                            .SelectedItemTemplate(new FuncDataTemplate<V1Namespace?>((x,y) => new Label().Content(@x?.Metadata.Name)))
                            .ItemTemplate(new FuncDataTemplate<V1Namespace?>((x,y) => new Label().Content(@x?.Metadata.Name)))
                    ]),
                new Editor()
                    .Row(1)
                    .DataContext(@vm.Drawing),
            ]);
}
