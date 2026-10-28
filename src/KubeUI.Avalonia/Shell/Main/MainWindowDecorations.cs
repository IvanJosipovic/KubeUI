using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using FluentIcons.Avalonia;
using FluentIcons.Avalonia.Internals;
using FluentIcons.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Shell.Main;

internal sealed class MainWindowDecorations : IWindowDrawnDecorationsTemplate
{
    private const double TitleBarIconSize = 16;

    private readonly Window _window;
    private readonly ControlTheme _captionButtonTheme;
    private readonly MainViewModel _viewModel;

    private MainWindowDecorations(Window window, MainViewModel viewModel, ControlTheme captionButtonTheme)
    {
        _window = window;
        _viewModel = viewModel;
        _captionButtonTheme = captionButtonTheme;
    }

    internal static ControlTheme CreateTheme(Window window, MainViewModel viewModel)
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application is not initialized.");
        var fluentTheme = GetResource<ControlTheme>(
            application,
            typeof(WindowDrawnDecorations),
            application.ActualThemeVariant);
        var captionButtonTheme = GetResource<ControlTheme>(
            application,
            "FluentDrawnCaptionButton",
            application.ActualThemeVariant);

        return new ControlTheme(typeof(WindowDrawnDecorations))
        {
            BasedOn = fluentTheme,
            Setters =
            {
                new Setter(
                    WindowDrawnDecorations.TemplateProperty,
                    new MainWindowDecorations(window, viewModel, captionButtonTheme))
            }
        };
    }

    TemplateResult<WindowDrawnDecorationsContent> IWindowDrawnDecorationsTemplate.Build()
    {
        var names = new NameScope();
        var title = new TextBlock()
            .FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey))
            .TextAlignment(TextAlignment.Center)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(_window, window => window.Title);
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
                    .Stretch(Stretch.Uniform)
                    .Margin(new Thickness(0, 0, 4, 0))
                    .Source(ApplicationIcons.CreateControlPlaneImage()),
                title,
                new Border().Width(TitleBarIconSize + 4))
            .ElementRole(WindowDecorationsElementRole.TitleBar);

        var menu = MainMenu.CreateOptionsMenu(_viewModel);
        var options = new Button()
            .Name("PART_OptionsButton")
            .Theme(_captionButtonTheme)
            .Content(new FluentIcon()
                .Icon(Icon.Navigation)
                .BindValue(
                    GenericIcon.FontSizeProperty,
                    CompiledBinding.Create<Window, double>(window => window.FontSize, source: _window)))
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Top)
            .ToolTip_Tip(Assets.Resources.MainWindow_Options)
            .ElementRole(WindowDecorationsElementRole.User)
            .AutomationName(Assets.Resources.MainWindow_Options)
            .AttachedFlyout(menu)
            .IsVisible(_window, window => window.WindowState, BindingMode.OneWay,
                new FuncValueConverter<WindowState, bool>(state => state != WindowState.FullScreen))
            .With(button => button.Command = new RelayCommand(() => ShowOptionsMenu(button, menu)));

        var captionButtons = new StackPanel()
            .Orientation(Orientation.Horizontal)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .VerticalAlignment(VerticalAlignment.Top)
            .Children(
                CreateCaptionButton("PART_MinimizeButton", "−", WindowDecorationsElementRole.MinimizeButton, names),
                CreateCaptionButton("PART_MaximizeButton", "□", WindowDecorationsElementRole.MaximizeButton, names),
                CreateCaptionButton("PART_CloseButton", "×", WindowDecorationsElementRole.CloseButton, names));

        var overlay = new Grid()
            .Name("PART_TitleBar")
            .Height(new TemplateBinding { Property = WindowDrawnDecorations.TitleBarHeightProperty })
            .Margin(new TemplateBinding { Property = WindowDrawnDecorations.ShadowThicknessProperty })
            .Children(titleContent, options, captionButtons);

        var underlay = new Panel().Children(
            new Border()
                .Margin(new TemplateBinding { Property = WindowDrawnDecorations.ShadowThicknessProperty })
                .Background(new DynamicResourceExtension("SystemRegionBrush"))
                .BorderBrush(new DynamicResourceExtension("SystemChromeHighColor"))
                .BorderThickness(new Thickness(1))
                .BoxShadow(BoxShadows.Parse("0 8 32 0 #40000000")));

        return new TemplateResult<WindowDrawnDecorationsContent>(
            new WindowDrawnDecorationsContent
            {
                Underlay = underlay,
                Overlay = overlay
            },
            names);
    }

    object ITemplate.Build() => ((IWindowDrawnDecorationsTemplate)this).Build();

    private Button CreateCaptionButton(string name, string content, WindowDecorationsElementRole role, NameScope names) =>
        new Button().Name(name, names).Theme(_captionButtonTheme).Content(content).ElementRole(role);

    private void ShowOptionsMenu(Button button, MenuFlyout menu)
    {
        // Decorations live beside the Window in TopLevelHost, so flyouts must target the Window.
        var buttonOrigin = button.TranslatePoint(default, _window)
            ?? throw new InvalidOperationException("The Options button is not in the window visual tree.");
        menu.Popup.PlacementRect = new Rect(buttonOrigin, button.Bounds.Size);
        menu.Placement = PlacementMode.AnchorAndGravity;
        menu.PlacementAnchor = PopupAnchor.BottomLeft;
        menu.PlacementGravity = PopupGravity.BottomRight;
        menu.ShowAt(_window);
    }

    private static T GetResource<T>(Application application, object key, ThemeVariant themeVariant)
        where T : class
    {
        if (!application.TryGetResource(key, themeVariant, out var resource) || resource is not T typed)
        {
            throw new InvalidOperationException($"Required Fluent resource '{key}' was not found.");
        }

        return typed;
    }
}
