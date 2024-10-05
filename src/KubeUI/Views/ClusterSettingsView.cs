using Avalonia.Data.Converters;

namespace KubeUI.Views;

public sealed class ClusterSettingsView : MyViewBase<ClusterSettingsViewModel>
{
    protected override object Build(ClusterSettingsViewModel? vm) =>
        new StackPanel()
            .Children([
                new Grid()
                    .Cols("*,2*")
                    .ToolTip("Manually specified namespaces")
                    .Children([
                        new Label()
                            .Col(0)
                            .Content("Namespaces"),
                        new ListBox()
                            .Col(1)
                            ,
                        ]),
        ]);
}
