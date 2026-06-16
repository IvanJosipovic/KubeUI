using System.Runtime.InteropServices;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Dock.Avalonia.Controls;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Shell.Main.ViewModels;

namespace KubeUI.Avalonia.Shell.Main.Views;

public sealed class MainView : ViewBase<MainViewModel>
{
    public MainView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<MainViewModel>();
        }
    }

    protected override object Build(MainViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Background(Brushes.Transparent)
            .Rows("Auto,*")
            .Children(
                CreateMenu(vm),
                new DockControl()
                    .Col(0)
                    .Row(1)
                    .Margin(4)
                    .Layout(vm, vm => vm.Layout));
    }

    private static Menu CreateMenu(MainViewModel vm)
    {
        return new Menu()
            .Col(0)
            .Row(0)
            .VerticalAlignment(VerticalAlignment.Top)
            .IsVisible(!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            .Items(
                CreateFileMenu(vm),
                CreateWindowMenu(vm),
                CreateHelpMenu(vm));
    }

    private static MenuItem CreateFileMenu(MainViewModel vm)
    {
        return new MenuItem()
            .Header(Assets.Resources.MainView_Menu_File)
            .Items(
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_File_LoadKubeConfig)
                    .Command(vm, x => x.LoadKubeConfigCommand),
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_File_ImportAksCluster)
                    .Command(vm, x => x.ImportAksClusterCommand),
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_File_Clusters)
                    .Command(vm, x => x.OpenClustersCommand),
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_File_Settings)
                    .Command(vm, x => x.OpenSettingsCommand),
                new Separator(),
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_File_Exit)
                    .Command(vm, x => x.CloseCommand));
    }

    private static MenuItem CreateWindowMenu(MainViewModel vm)
    {
        return new MenuItem()
            .Header(Assets.Resources.MainView_Menu_Window)
            .Items(
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_Window_ResetLayout)
                    .Command(vm, x => x.ResetLayoutCommand),
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_Window_SwitchTheme)
                    .Command(vm, x => x.SwitchThemeCommand));
    }

    private static MenuItem CreateHelpMenu(MainViewModel vm)
    {
        return new MenuItem()
            .Header(Assets.Resources.MainView_Menu_Help)
            .Items(
                new MenuItem()
                    .Header(Assets.Resources.MainView_Menu_Help_About)
                    .Command(vm, x => x.OpenAboutCommand));
    }
}
