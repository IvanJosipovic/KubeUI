using k8s.Models;

namespace KubeUI.Views;

public sealed class VisualizationResourceView : MyViewBase<VisualizationResourceViewModel>
{
    protected override object Build(VisualizationResourceViewModel? vm) =>
        new StackPanel()
            .ContextFlyout([
                new MenuItem()
                    .Header("Properties")
                    .Command(vm.ViewPropertiesCommand)
                    .CommandParameter(@vm.Object),
                new MenuItem()
                    .Header("View Yaml")
                    .Command(vm.ViewYamlCommand)
                    .CommandParameter(@vm.Object),
            ])
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .ToolTip(vm.Object.ApiGroupAndVersion() + "/" + vm.Object.Kind + " " + vm.Object.Name())
            .Children([
                new Avalonia.Svg.Skia.Svg(new Uri("avares://KubeUI/"))
                    .Path(@vm.IconPath),
                new TextBlock()
                    .Margin(0, 5, 0, 0)
                    .Width(120)
                    .TextAlignment(TextAlignment.Center)
                    .TextWrapping(TextWrapping.Wrap)
                    .ClipToBounds(false)
                    .Text(vm.Object.Kind + "\n" + vm.Object.Name())
            ]);
}
