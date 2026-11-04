using System.Windows.Input;

namespace KubeUI.App.Shell.Main;

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
        new(SharedL10n.Instance.MainView_Menu_File.CurrentValue()!,
        [
            new(SharedL10n.Instance.MainView_Menu_File_LoadKubeConfig.CurrentValue()!, vm.LoadKubeConfigCommand),
            new(SharedL10n.Instance.MainView_Menu_File_ImportAksCluster.CurrentValue()!, vm.ImportAksClusterCommand),
            new(SharedL10n.Instance.MainView_Menu_File_Clusters.CurrentValue()!, vm.OpenClustersCommand),
            new(SharedL10n.Instance.MainView_Menu_File_Settings.CurrentValue()!, vm.OpenSettingsCommand),
            null,
            new(SharedL10n.Instance.MainView_Menu_File_Exit.CurrentValue()!, vm.CloseCommand)
        ]),
        new(SharedL10n.Instance.MainView_Menu_Window.CurrentValue()!,
        [
            new(SharedL10n.Instance.MainView_Menu_Window_ResetLayout.CurrentValue()!, vm.ResetLayoutCommand),
            new(SharedL10n.Instance.MainView_Menu_Window_SwitchTheme.CurrentValue()!, vm.SwitchThemeCommand)
        ]),
        new(SharedL10n.Instance.MainView_Menu_Help.CurrentValue()!,
        [
            new(SharedL10n.Instance.MainView_Menu_Help_CheckForUpdates.CurrentValue()!, vm.CheckForUpdatesCommand),
            new(SharedL10n.Instance.MainView_Menu_Help_About.CurrentValue()!, vm.OpenAboutCommand)
        ])
    ];

    private sealed record MenuGroup(string Header, MenuAction?[] Actions);
    private sealed record MenuAction(string Header, ICommand Command);
}
