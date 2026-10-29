using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Shell.Main;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Main;

public sealed class MainWindowMenuTests
{
    [AvaloniaFact]
    public async Task native_menu_exposes_kubeconfig_aks_and_update_commands()
    {
        var viewModel = Application.Current.GetRequiredTestService<MainViewModel>();
        var window = new MainWindow { DataContext = viewModel };
        window.Show();
        try
        {
            await TestApplicationExtensions.WaitForUiAsync();

            var commands = NativeMenu.GetMenu(window).ShouldNotBeNull().Items
                .OfType<NativeMenuItem>()
                .SelectMany(top => top.Menu?.Items.OfType<NativeMenuItem>() ?? [])
                .Where(item => item is not NativeMenuItemSeparator)
                .Select(item => item.Command)
                .ToList();

            commands.ShouldContain(viewModel.LoadKubeConfigCommand);
            commands.ShouldContain(viewModel.ImportAksClusterCommand);
            commands.ShouldContain(viewModel.CheckForUpdatesCommand);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task native_menu_lists_the_expected_items_and_commands()
    {
        var viewModel = Application.Current.GetRequiredTestService<MainViewModel>();
        var window = new MainWindow { DataContext = viewModel };
        window.Show();
        try
        {
            await TestApplicationExtensions.WaitForUiAsync();

            Describe(NativeMenu.GetMenu(window).ShouldNotBeNull(), viewModel).ShouldBe(
            [
                Assets.Resources.MainView_Menu_File!,
                $"  {Assets.Resources.MainView_Menu_File_LoadKubeConfig} -> {nameof(viewModel.LoadKubeConfigCommand)}",
                $"  {Assets.Resources.MainView_Menu_File_ImportAksCluster} -> {nameof(viewModel.ImportAksClusterCommand)}",
                $"  {Assets.Resources.MainView_Menu_File_Clusters} -> {nameof(viewModel.OpenClustersCommand)}",
                $"  {Assets.Resources.MainView_Menu_File_Settings} -> {nameof(viewModel.OpenSettingsCommand)}",
                "  ---",
                $"  {Assets.Resources.MainView_Menu_File_Exit} -> {nameof(viewModel.CloseCommand)}",
                Assets.Resources.MainView_Menu_Window!,
                $"  {Assets.Resources.MainView_Menu_Window_ResetLayout} -> {nameof(viewModel.ResetLayoutCommand)}",
                $"  {Assets.Resources.MainView_Menu_Window_SwitchTheme} -> {nameof(viewModel.SwitchThemeCommand)}",
                Assets.Resources.MainView_Menu_Help!,
                $"  {Assets.Resources.MainView_Menu_Help_CheckForUpdates} -> {nameof(viewModel.CheckForUpdatesCommand)}",
                $"  {Assets.Resources.MainView_Menu_Help_About} -> {nameof(viewModel.OpenAboutCommand)}",
            ]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task managed_chrome_does_not_add_a_duplicate_menu_bar()
    {
        var viewModel = Application.Current.GetRequiredTestService<MainViewModel>();
        var window = new MainWindow(useNativeChrome: false) { DataContext = viewModel };
        window.Show();
        try
        {
            await TestApplicationExtensions.WaitForUiAsync();

            NativeMenu.GetMenu(window).ShouldNotBeNull();
            NativeMenu.GetIsNativeMenuExported(window).ShouldBeFalse();
            window.GetVisualDescendants().OfType<NativeMenuBar>().ShouldBeEmpty();
        }
        finally
        {
            window.Close();
        }
    }

    private static List<string> Describe(NativeMenu menu, MainViewModel viewModel)
    {
        List<string> lines = [];
        foreach (var top in menu.Items.OfType<NativeMenuItem>())
        {
            lines.Add(top.Header ?? string.Empty);
            foreach (var item in top.Menu?.Items ?? [])
            {
                lines.Add(item is NativeMenuItem child and not NativeMenuItemSeparator ? $"  {child.Header} -> {CommandName(child.Command, viewModel)}" : "  ---");
            }
        }

        return lines;
    }

    private static string CommandName(ICommand? command, MainViewModel viewModel)
    {
        return command switch
        {
            null => "<none>",
            _ when ReferenceEquals(command, viewModel.LoadKubeConfigCommand) => nameof(viewModel.LoadKubeConfigCommand),
            _ when ReferenceEquals(command, viewModel.ImportAksClusterCommand) => nameof(viewModel.ImportAksClusterCommand),
            _ when ReferenceEquals(command, viewModel.OpenClustersCommand) => nameof(viewModel.OpenClustersCommand),
            _ when ReferenceEquals(command, viewModel.OpenSettingsCommand) => nameof(viewModel.OpenSettingsCommand),
            _ when ReferenceEquals(command, viewModel.CloseCommand) => nameof(viewModel.CloseCommand),
            _ when ReferenceEquals(command, viewModel.ResetLayoutCommand) => nameof(viewModel.ResetLayoutCommand),
            _ when ReferenceEquals(command, viewModel.SwitchThemeCommand) => nameof(viewModel.SwitchThemeCommand),
            _ when ReferenceEquals(command, viewModel.CheckForUpdatesCommand) => nameof(viewModel.CheckForUpdatesCommand),
            _ when ReferenceEquals(command, viewModel.OpenAboutCommand) => nameof(viewModel.OpenAboutCommand),
            _ => throw new InvalidOperationException($"Unexpected menu command of type '{command.GetType().Name}'."),
        };
    }
}
