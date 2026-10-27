using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Shell.Main;

public sealed class MainWindow : Window
{
    public MainWindow() : this(OperatingSystem.IsMacOS())
    {
    }

    internal MainWindow(bool useNativeChrome)
    {
        UseNativeChrome = useNativeChrome;
        this.Title(Assets.Resources.MainWindow_Title)
            .Icon(new WindowIcon(AssetLoader.Open(new Uri("avares://KubeUI.Avalonia/Assets/icon.ico"))))
            .UseLayoutRounding(true)
            .FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey))
            .MinWidth(360)
            .Content(new MainView());
        if (!useNativeChrome)
        {
            this.ExtendClientAreaToDecorationsHint(true)
                .Padding(this, x => x.WindowDecorationMargin);
        }

        if (DesignTimePreview.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<MainViewModel>();
        }

        UpdateMenu();
    }

    internal bool UseNativeChrome { get; }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateMenu();
    }

    /// <summary>
    /// Attaches the shared application menu to this window. macOS presents it as the system menu;
    /// managed decorations present the same actions through their Options flyout.
    /// </summary>
    private void UpdateMenu()
    {
        NativeMenu.SetMenu(this, DataContext is MainViewModel menuViewModel
            ? MainMenu.CreateNativeMenu(menuViewModel)
            : null);

        if (UseNativeChrome)
        {
            WindowDecorationsTheme = null;
            return;
        }

        WindowDecorationsTheme = DataContext is MainViewModel viewModel
            ? MainWindowDecorations.CreateTheme(this, viewModel)
            : null;
    }
}
