using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using k8s.Models;
using KubeUI.Client.Metrics;
using Ursa.Controls;

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
                    .IsVisible(@vm.Cluster.ListNamespaces, converter: Utilities.InverseBooleanConverter)
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
                                ]),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("Metrics Server Type")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Metrics Server Type"),
                        new EnumSelector()
                            .Col(1)
                            .DisplayDescription(true)
                            .EnumType(typeof(MetricsServiceType))
                            .Value(@vm.ClusterSettings.MetricsServiceType),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("Prometheus Server Url")
                    .IsVisible(@vm.ClusterSettings.MetricsServiceType, new FuncValueConverter<MetricsServiceType, bool>((x) => x == MetricsServiceType.AzureManagedPrometheus))
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Prometheus Server Url"),
                        new TextBox()
                            .Col(1)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(@vm.ClusterSettings.PrometheusServerUrl),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("Prometheus Client Id")
                    .IsVisible(@vm.ClusterSettings.MetricsServiceType, new FuncValueConverter<MetricsServiceType, bool>((x) => x == MetricsServiceType.AzureManagedPrometheus))
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Prometheus Client Id"),
                        new TextBox()
                            .Col(1)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(@vm.ClusterSettings.PrometheusClientId),
                        ]),
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("Prometheus Client Secret")
                    .IsVisible(@vm.ClusterSettings.MetricsServiceType, new FuncValueConverter<MetricsServiceType, bool>((x) => x == MetricsServiceType.AzureManagedPrometheus))
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Prometheus Client Secret"),
                        new TextBox()
                            .Col(1)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(@vm.ClusterSettings.PrometheusClientSecret),
                        ]),
            ]);
}
