using Avalonia;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Shell.Main.ViewModels;

namespace KubeUI.Avalonia.Shell.Main.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DesignTimePreview.Run(() => DataContext = DesignTimePreview.Get<MainViewModel>());
    }
}

