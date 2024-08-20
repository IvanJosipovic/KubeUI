using Avalonia.Styling;
using NodeEditor.Controls;

namespace KubeUI.Views;

public sealed class VisualizationView : MyViewBase<VisualizationViewModel>
{
    protected override StyleGroup? BuildStyles() => [
           new Style(x => x.Is<Pin>())
                .Setter(Pin.ContextFlyoutProperty, new MenuFlyout())
    ];

    protected override object Build(VisualizationViewModel? vm) =>
       new Grid()
            .Children([
                new Editor()
                    .DataContext(@vm.Drawing),
            ]);
}
