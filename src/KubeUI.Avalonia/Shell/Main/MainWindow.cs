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

        if (Design.IsDesignMode)
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

    private void UpdateMenu()
    {
        if (UseNativeChrome)
        {
            WindowDecorationsTheme = null;
            NativeMenu.SetMenu(this, DataContext is MainViewModel nativeViewModel
                ? MainMenu.CreateNativeMenu(nativeViewModel)
                : null);
            return;
        }

        NativeMenu.SetMenu(this, null);
        WindowDecorationsTheme = DataContext is MainViewModel viewModel
            ? MainWindowDecorations.CreateTheme(this, viewModel)
            : null;
    }
}
