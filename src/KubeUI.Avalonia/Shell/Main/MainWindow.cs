using System.Linq.Expressions;
using System.Windows.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Platform;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Shell.Main;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = Assets.Resources.MainWindow_Title;
        Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://KubeUI.Avalonia/Assets/icon.ico")));
        UseLayoutRounding = true;
        Content = new MainView();

        if (Design.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<MainViewModel>();
        }

        UpdateMenu();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        if (DataContext is MainViewModel vm)
        {
            NativeMenu.SetMenu(this, CreateMenu(vm));
        }
    }

    private static NativeMenu CreateMenu(MainViewModel vm)
    {
        return [
            CreateFileMenu(vm),
            CreateWindowMenu(vm),
            CreateHelpMenu(vm)
        ];
    }

    private static NativeMenuItem CreateFileMenu(MainViewModel vm)
    {
        return new NativeMenuItem(Assets.Resources.MainView_Menu_File!)
            .Menu([
                CreateCommandItem(vm, Assets.Resources.MainView_Menu_File_Clusters!, x => x.OpenClustersCommand),
                CreateCommandItem(vm, Assets.Resources.MainView_Menu_File_Settings!, x => x.OpenSettingsCommand),
                new NativeMenuItemSeparator(),
                CreateCommandItem(vm, Assets.Resources.MainView_Menu_File_Exit!, x => x.CloseCommand)
            ]);
    }

    private static NativeMenuItem CreateWindowMenu(MainViewModel vm)
    {
        return new NativeMenuItem(Assets.Resources.MainView_Menu_Window!)
            .Menu([
                CreateCommandItem(vm, Assets.Resources.MainView_Menu_Window_ResetLayout!, x => x.ResetLayoutCommand),
                CreateCommandItem(vm, Assets.Resources.MainView_Menu_Window_SwitchTheme!, x => x.SwitchThemeCommand)
            ]);
    }

    private static NativeMenuItem CreateHelpMenu(MainViewModel vm)
    {
        return new NativeMenuItem(Assets.Resources.MainView_Menu_Help!)
            .Menu([
                CreateCommandItem(vm, Assets.Resources.MainView_Menu_Help_About!, x => x.OpenAboutCommand)
            ]);
    }

    private static NativeMenuItem CreateCommandItem(MainViewModel vm, string header, Expression<Func<MainViewModel, ICommand>> command)
    {
        return new NativeMenuItem(header)
            .Command(vm, command);
    }
}
