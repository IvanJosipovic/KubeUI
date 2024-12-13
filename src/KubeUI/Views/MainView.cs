using System.Runtime.InteropServices;
using Dock.Avalonia.Controls;

namespace KubeUI.Views;

public sealed class MainView : MyViewBase<MainViewModel>
{
    protected override object Build(MainViewModel? vm) =>
        new Grid()
            .Rows("Auto,*")
            .Children([
                new Menu()
                    .Row(0)
                    .VerticalAlignment(VerticalAlignment.Top)
                    .Items([
                        new MenuItem()
                            .Header(Assets.Resources.MainView_Menu_File)
                            .Items([
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_File_LoadKubeConfig)
                                    .Command(vm.LoadKubeConfigCommand),
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_File_Clusters)
                                    .Command(vm.OpenClustersCommand),
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_File_Settings)
                                    .Command(vm.OpenSettingsCommand),

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
                                    new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_Window_SwitchTheme)
                                    .Command(vm.SwitchThemeCommand),
                                ]),
                        new MenuItem()
                            .Header(Assets.Resources.MainView_Menu_Help)
                            .Items([
                                new MenuItem()
                                    .Header(Assets.Resources.MainView_Menu_Help_About)
                                    .Command(vm.OpenAboutCommand),
                                ])
                        ])
                    .IsVisible(() => !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)),
                new DockControl()
                    .Row(1)
                    .Margin(4)
                    .Layout(@vm.Layout)
            ]);
}
