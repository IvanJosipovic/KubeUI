using Avalonia.Controls.Templates;
using k8s.Models;

namespace KubeUI.Views;

public sealed class ClusterSettingsView : MyViewBase<ClusterSettingsViewModel>
{
    protected override object Build(ClusterSettingsViewModel? vm) =>
        new StackPanel()
            .Margin(10,0,0,0)
            .Children([
                new TextBlock()
                    .FontSize(25)
                    .Text($"{vm.Cluster.Name} Settings"),
                new Grid()
                    .IsEnabled(@vm.Cluster.ListNamespaces, converter: Utilities.InverseBooleanConverter)
                    .Cols("*,2*")
                    .ToolTip("Manually specified namespaces")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Namespaces (Restart Required)"),
                        new StackPanel()
                            .Col(1)
                            .Children([
                                    new Grid()
                                        .Cols("*,Auto")
                                        .Children([
                                            new TextBox().Col(0)
                                                .Text(@vm.Namespace),
                                            new Button().Col(1)
                                                .Content("Add")
                                                .Command(vm.AddNamespaceCommand),
                                            ]),
                                    new ListBox()
                                        .ItemsSource(vm.ClusterSettings.Namespaces)
                                        .ItemTemplate(new FuncDataTemplate<string>((item, ns) => {
                                            return new Grid()
                                                    .Cols("*,Auto")
                                                    .Children([
                                                        new TextBlock().Col(0)
                                                            .VerticalAlignment(VerticalAlignment.Center)
                                                            .Text(@item),
                                                        new Button().Col(1)
                                                            .Content("Remove")
                                                            .Command(vm.RemoveNamespaceCommand)
                                                            .CommandParameter(@item),
                                                        ]);
                                        })),
                                ])
                            ,
                        ]),
        ]);
}
