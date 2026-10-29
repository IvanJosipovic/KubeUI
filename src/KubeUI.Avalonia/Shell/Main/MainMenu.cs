using System.Windows.Input;

namespace KubeUI.Avalonia.Shell.Main;

internal static class MainMenu
{
    internal static MenuFlyout CreateOptionsMenu(MainViewModel vm)
    {
        return new MenuFlyout { ItemsSource = CreateGroups(vm).Select(CreateMenuItem).ToArray() };
    }

    internal static NativeMenu CreateNativeMenu(MainViewModel vm)
    {
        var menu = new NativeMenu();
        foreach (var group in CreateGroups(vm))
        {
            var children = new NativeMenu();
            foreach (var action in group.Actions)
            {
                children.Add(action is null
                    ? new NativeMenuItemSeparator()
                    : new NativeMenuItem(action.Header).Command(action.Command));
            }
            menu.Add(new NativeMenuItem(group.Header).Menu(children));
        }
        return menu;
    }

    private static MenuItem CreateMenuItem(MenuGroup group)
    {
        return new MenuItem()
            .Header(group.Header)
            .Items(group.Actions.Select(action => action is null
                ? (Control)new Separator()
                : new MenuItem()
                    .Header(action.Header)
                    .Command(action.Command)).ToArray());
    }

    private static MenuGroup[] CreateGroups(MainViewModel vm) =>
    [
        new(Assets.Resources.MainView_Menu_File,
        [
            new(Assets.Resources.MainView_Menu_File_LoadKubeConfig, vm.LoadKubeConfigCommand),
            new(Assets.Resources.MainView_Menu_File_ImportAksCluster, vm.ImportAksClusterCommand),
            new(Assets.Resources.MainView_Menu_File_Clusters, vm.OpenClustersCommand),
            new(Assets.Resources.MainView_Menu_File_Settings, vm.OpenSettingsCommand),
            null,
            new(Assets.Resources.MainView_Menu_File_Exit, vm.CloseCommand)
        ]),
        new(Assets.Resources.MainView_Menu_Window,
        [
            new(Assets.Resources.MainView_Menu_Window_ResetLayout, vm.ResetLayoutCommand),
            new(Assets.Resources.MainView_Menu_Window_SwitchTheme, vm.SwitchThemeCommand)
        ]),
        new(Assets.Resources.MainView_Menu_Help,
        [
            new(Assets.Resources.MainView_Menu_Help_CheckForUpdates, vm.CheckForUpdatesCommand),
            new(Assets.Resources.MainView_Menu_Help_About, vm.OpenAboutCommand)
        ])
    ];

    private sealed record MenuGroup(string Header, MenuAction?[] Actions);
    private sealed record MenuAction(string Header, ICommand Command);
}
