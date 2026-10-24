using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class ServiceMapView : ViewBase<ServiceMapViewModel>
{
    protected override object Build(ServiceMapViewModel vm)
    {
        return new Grid()
            .Rows("Auto,*")
            .Children(
                CreateTopBar(vm).Row(0),
                new TextBlock()
                    .Text("Service Map visualization - coming soon")
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .FontSize(16)
                    .Foreground(new DynamicResourceExtension("SystemBaseMediumBrush"))
                    .Row(1)
            );
    }

    private static Border CreateTopBar(ServiceMapViewModel vm)
    {
        var graphTextBinding = CompiledBinding.Create<ServiceMapViewModel, string>(x => GetGraphText(x));
        return new Border()
            .Classes("toolbar")
            .Height(40)
            .Child(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Margin(8, 0)
                    .Children(
                        new ComboBox()
                            .Width(140)
                            .ItemsSource(new[] { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(60) })
                            .SelectedItem(CompiledBinding.Create<ServiceMapViewModel, TimeSpan>(x => x.TimeWindow, mode: BindingMode.TwoWay))
                            .ItemTemplate(new FuncDataTemplate<TimeSpan>((ts, _) => new TextBlock().Text($"{ts.TotalMinutes} {Assets.Resources.ServiceMapView_TimeWindowMinutes}"))),

                        new CheckBox()
                            .Content(Assets.Resources.ServiceMapView_AggregateByNamespace)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.AggregateByNamespace, mode: BindingMode.TwoWay)),

                        new CheckBox()
                            .Content(Assets.Resources.ServiceMapView_ShowDeniedOnly)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.ShowOnlyErrors, mode: BindingMode.TwoWay)),

                        new Button()
                            .Content(Assets.Resources.ServiceMapView_ResetLayout)
                            .Command(vm.ResetLayoutCommand),

                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Margin(16, 0, 0, 0)
                            .Text(graphTextBinding)
                    )
            );
    }

    private static string GetGraphText(ServiceMapViewModel vm)
    {
        var nodeCount = vm.Graph?.NodeCount ?? 0;
        var edgeCount = vm.Graph?.EdgeCount ?? 0;
        return $"{Assets.Resources.ServiceMapView_Nodes}: {nodeCount}  {Assets.Resources.ServiceMapView_Edges}: {edgeCount}";
    }
}