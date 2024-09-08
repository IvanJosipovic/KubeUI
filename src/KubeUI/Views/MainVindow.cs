using AvaloniaEdit.Utils;

namespace KubeUI.Views;

public static class MainWindow
{
    public static Window Build(MainViewModel vm) =>
        (Window)new Window()
            .Title("KubeUI")
            .UseLayoutRounding(true)
            .SetProp(NativeMenu.MenuProperty, new NativeMenu()
                .Items([
                    new NativeMenuItem()
                        .Header(Resources.MainView_Menu_File)
                        .Items([
                            new NativeMenuItem()
                                .Header(Resources.MainView_Menu_File_Clusters)
                                .Command(vm.OpenClustersCommand),
                            new NativeMenuItem()
                                .Header(Resources.MainView_Menu_File_Settings)
                                .Command(vm.OpenSettingsCommand),

                            new NativeMenuItemSeparator(),

                            new NativeMenuItem()
                                .Header(Resources.MainView_Menu_File_Exit)
                                .Command(vm.CloseCommand),
                            ]),
                    new NativeMenuItem()
                        .Header(Resources.MainView_Menu_Window)
                        .Items([
                            new NativeMenuItem()
                                .Header(Resources.MainView_Menu_Window_ResetLayout)
                                .Command(vm.ResetLayoutCommand),
                            new NativeMenuItem()
                                .Header(Resources.MainView_Menu_Window_SwitchTheme)
                                .Command(vm.SwitchThemeCommand),
                            ]),
                    new NativeMenuItem()
                        .Header(Resources.MainView_Menu_Help)
                        .Items([
                            new NativeMenuItem()
                                .Header(Resources.MainView_Menu_Help_About)
                                .Command(vm.OpenAboutCommand),
                            ]),
                    ])
            )
            .Content(
                new MainView()
            );
}

public static class WindowExtensions
{
    public static NativeMenu Items(this NativeMenu menu, NativeMenuItemBase[] items)
    {
        menu.Items.AddRange(items);

        return menu;
    }

    public static NativeMenuItem Items(this NativeMenuItem menu, NativeMenuItemBase[] items)
    {
        menu.Menu ??= [];

        menu.Menu.Items.AddRange(items);

        return menu;
    }
}
