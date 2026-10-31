using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Avalonia;
using FluentIcons.Avalonia.Internals;
using FluentIcons.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Styles;
using static KubeUI.Avalonia.Shell.Main.DecorationsMarkup;
using Path = Avalonia.Controls.Shapes.Path;

namespace KubeUI.Avalonia.Shell.Main;

internal static class MainWindowDecorations
{
    private const double TitleBarIconSize = 16;

    internal static ControlTheme CreateTheme(Window window, MainViewModel viewModel)
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application is not initialized.");
        var fluentTheme = FluentResources.GetResource<ControlTheme>(
            application,
            typeof(WindowDrawnDecorations),
            application.ActualThemeVariant);
        var captionButtonTheme = FluentResources.GetResource<ControlTheme>(
            application,
            "FluentDrawnCaptionButton",
            application.ActualThemeVariant);

        return Theme(fluentTheme, names => Build(window, viewModel, captionButtonTheme, names));
    }

    private static WindowDrawnDecorationsContent Build(
        Window window, MainViewModel viewModel, ControlTheme captionButtonTheme, NameScope names)
    {
        var title = new TextBlock()
            .FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey))
            .TextAlignment(TextAlignment.Center)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(window, window => window.Title);
        var titleContent = new StackPanel()
            .Orientation(Orientation.Horizontal)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .VerticalAlignment(VerticalAlignment.Center)
            .Children(
                new Image()
                    .Name("PART_TitleBarIcon")
                    .Width(TitleBarIconSize)
                    .Height(TitleBarIconSize)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Margin(new Thickness(0, 0, 4, 0))
                    .Source(ApplicationIcons.CreateControlPlaneImage()),
                title,
                new Border().Width(TitleBarIconSize + 4))
            .ElementRole(WindowDecorationsElementRole.TitleBar);

        var menu = MainMenu.CreateOptionsMenu(viewModel);
        var options = new Button()
            .Name("PART_OptionsButton")
            .Theme(captionButtonTheme)
            .Content(new FluentIcon()
                .Icon(Icon.Navigation)
                .BindValue(
                    GenericIcon.FontSizeProperty,
                    CompiledBinding.Create<Window, double>(window => window.FontSize, source: window)))
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Top)
            .ToolTip_Tip(Assets.Resources.MainWindow_Options)
            .ElementRole(WindowDecorationsElementRole.User)
            .AutomationName(Assets.Resources.MainWindow_Options)
            .AttachedFlyout(menu)
            .IsVisible(window, window => window.WindowState, BindingMode.OneWay,
                new FuncValueConverter<WindowState, bool>(state => state != WindowState.FullScreen))
            .With(button => button.Command(new RelayCommand(() => ShowOptionsMenu(window, button, menu))));

        var captionButtons = new StackPanel()
            .Orientation(Orientation.Horizontal)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .VerticalAlignment(VerticalAlignment.Top)
            .Children(
                CaptionButton(names, captionButtonTheme, WindowDecorationsElementRole.MinimizeButton,
                    CaptionIcon("M2048 1229v-205h-2048v205h2048z")),
                CaptionButton(names, captionButtonTheme, WindowDecorationsElementRole.MaximizeButton,
                    MaximizeIcon(window)),
                CaptionButton(names, captionButtonTheme, WindowDecorationsElementRole.CloseButton,
                    CaptionIcon("M1169 1024l879 -879l-145 -145l-879 879l-879 -879l-145 145l879 879l-879 879l145 145l879 -879l879 879l145 -145z"))
                    .Background(new SolidColorBrush(Color.Parse("#ffe81123")))
                    .BorderBrush(new SolidColorBrush(Color.Parse("#fff1707a"))));

        var overlay = new Grid()
            .Name("PART_TitleBar")
            .VerticalAlignment(VerticalAlignment.Top)
            .Background(Brushes.Transparent)
            .ElementRole(WindowDecorationsElementRole.TitleBar)
            .Height(Template(WindowDrawnDecorations.TitleBarHeightProperty))
            .Margin(Template(WindowDrawnDecorations.ShadowThicknessProperty))
            .Children(titleContent, options, captionButtons);

        var underlay = new Panel().Children(
            new Border()
                .Name("PART_WindowBorder")
                .Margin(Template(WindowDrawnDecorations.ShadowThicknessProperty))
                .Background(new DynamicResourceExtension("SystemRegionBrush"))
                .BorderBrush(new DynamicResourceExtension("SystemChromeHighColor"))
                .BorderThickness(Template(WindowDrawnDecorations.FrameThicknessProperty)));

        return new WindowDrawnDecorationsContent
        {
            Underlay = underlay,
            Overlay = overlay
        };
    }

    private static Viewbox CaptionIcon(string geometry) =>
        CaptionIcon(new Path().Data(Geometry.Parse(geometry)));

    private static Viewbox MaximizeIcon(Window window)
    {
        var maximize = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
        var restore = Geometry.Parse("M2048 410h-410v-410h-1638v1638h410v410h1638v-1638zM1434 1434h-1229v-1229h1229v1229zM1843 1843h-1229v-205h1024v-1024h205v1229z");
        var path = new Path()
            .Name("RestoreButtonPath")
            .BindValue(Path.DataProperty, CompiledBinding.Create<Window, WindowState>(
                x => x.WindowState, source: window,
                converter: new FuncValueConverter<WindowState, Geometry>(
                    state => state == WindowState.Maximized ? restore : maximize)));

        return CaptionIcon(path).RenderTransform(new RotateTransform(-90));
    }

    private static Viewbox CaptionIcon(Path path) =>
        new Viewbox()
            .Width(11)
            .Margin(new Thickness(2))
            .Child(path
                .Stretch(Stretch.UniformToFill)
                .Fill(new DynamicResourceExtension("CaptionButtonForeground")));

    private static void ShowOptionsMenu(Window window, Button button, MenuFlyout menu)
    {
        // Decorations live beside the Window in TopLevelHost, so flyouts must target the Window.
        var buttonOrigin = button.TranslatePoint(default, window)
            ?? throw new InvalidOperationException("The Options button is not in the window visual tree.");
        menu.Popup.PlacementRect(new Rect(buttonOrigin, button.Bounds.Size));
        menu.Placement(PlacementMode.AnchorAndGravity)
            .PlacementAnchor(PopupAnchor.BottomLeft)
            .PlacementGravity(PopupGravity.BottomRight);
        menu.ShowAt(window);
    }

}
