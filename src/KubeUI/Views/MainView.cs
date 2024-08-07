using Avalonia.Styling;
using Dock.Avalonia.Controls;

namespace KubeUI.Views;

public sealed class MainView : MyViewBase<MainViewModel>
{
    protected override StyleGroup? BuildStyles() => [
        new Style(x => x.Name("PART_PinnedDockGrid").Child().OfType<GridSplitter>().Child().OfType<Border>())
            .Setter(Border.OpacityProperty, 0.0)
        ];

    protected override object Build(MainViewModel vm) =>
        new Grid()
            .Rows("Auto,*")
            .Background(Brushes.Transparent)
            .Children([
                new Menu()
                    .Row(0)
                    .VerticalAlignment(VerticalAlignment.Top)
                    .Items([
                        new MenuItem()
                            .Header(Assets.Resources.MainView_Menu_File)
                            .Items([
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_File_Clusters)
                                    .Command(vm.OpenClustersCommand),
                                new Separator(),
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_File_Exit)
                                    .Command(vm.CloseCommand),
                                ]),
                        new MenuItem()
                            .Header(Assets.Resources.MainView_Menu_Window)
                            .Items([
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_Window_ResetLayout)
                                    .Command(vm.ResetLayoutCommand),
                                ]),
                        new MenuItem()
                            .Header(Assets.Resources.MainView_Menu_Help)
                            .Items([
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_Help_About)
                                    .Command(vm.OpenAboutCommand),
                                ])
                        ]),
                new DockControl()
                    .Row(1)
                    .Margin(4)
                    .Set(DockControl.LayoutProperty, @vm.Layout)
            ]);
}
