using Avalonia.Automation;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Avalonia.Internals;
using FluentIcons.Common;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Shell.Main;

internal sealed class MainWindowDecorations : IWindowDrawnDecorationsTemplate
{
    private readonly Window _window;
    private readonly IWindowDrawnDecorationsTemplate _fluentTemplate;
    private readonly ControlTheme _captionButtonTheme;
    private readonly MainViewModel _viewModel;

    private MainWindowDecorations(
        Window window,
        MainViewModel viewModel,
        IWindowDrawnDecorationsTemplate fluentTemplate,
        ControlTheme captionButtonTheme)
    {
        _window = window;
        _viewModel = viewModel;
        _fluentTemplate = fluentTemplate;
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
        var fluentTemplate = fluentTheme.Setters
            .OfType<Setter>()
            .SingleOrDefault(setter => setter.Property == WindowDrawnDecorations.TemplateProperty)
            ?.Value as IWindowDrawnDecorationsTemplate
            ?? throw new InvalidOperationException("The Fluent window decorations template was not found.");
        var captionButtonTheme = GetResource<ControlTheme>(
            application,
            "FluentDrawnCaptionButton",
            application.ActualThemeVariant);
        var theme = new ControlTheme(typeof(WindowDrawnDecorations)) { BasedOn = fluentTheme };
        theme.Setters.Add(new Setter(
            WindowDrawnDecorations.TemplateProperty,
            new MainWindowDecorations(window, viewModel, fluentTemplate, captionButtonTheme)));
        return theme;
    }

    TemplateResult<WindowDrawnDecorationsContent> IWindowDrawnDecorationsTemplate.Build()
    {
        var result = _fluentTemplate.Build();
        var overlay = result.Result.Overlay
            ?? throw new InvalidOperationException("The Fluent decorations template has no overlay.");
        var titlePanel = overlay.GetVisualDescendants()
            .OfType<Panel>()
            .SingleOrDefault(panel => panel.Name == "PART_TitleTextPanel")
            ?? throw new InvalidOperationException("The Fluent decorations title panel was not found.");
        var title = titlePanel.GetVisualDescendants()
            .OfType<TextBlock>()
            .SingleOrDefault()
            ?? throw new InvalidOperationException("The Fluent decorations title text was not found.");
        title.TextAlignment = TextAlignment.Center;
        title.Margin = new Thickness(0);
        title.FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey));

        var overlayWrapper = overlay is Panel { Name: "PART_OverlayWrapper" } rootWrapper
            ? rootWrapper
            : overlay.GetVisualDescendants()
                .OfType<Panel>()
                .SingleOrDefault(panel => panel.Name == "PART_OverlayWrapper")
            ?? throw new InvalidOperationException("The Fluent decorations overlay wrapper was not found.");
        overlayWrapper.Children.Insert(1, CreateOptionsButton());
        return result;
    }

    object ITemplate.Build() => ((IWindowDrawnDecorationsTemplate)this).Build();

    private Button CreateOptionsButton()
    {
        var menu = MainMenu.CreateOptionsMenu(_viewModel);
        var icon = new FluentIcon()
            .Icon(Icon.Navigation)
            .BindValue(
                GenericIcon.FontSizeProperty,
                CompiledBinding.Create<Window, double>(window => window.FontSize, source: _window));

        return new Button()
            .Theme(_captionButtonTheme)
            .Content(icon)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Top)
            .ToolTip_Tip(Assets.Resources.MainWindow_Options)
            .BindValue(
                Visual.IsVisibleProperty,
                CompiledBinding.Create<Window, WindowState>(
                    window => window.WindowState,
                    source: _window,
                    converter: new FuncValueConverter<WindowState, bool>(
                        state => state != WindowState.FullScreen)))
            .With(button =>
            {
                WindowDecorationProperties.SetElementRole(button, WindowDecorationsElementRole.User);
                AutomationProperties.SetName(button, Assets.Resources.MainWindow_Options);
                FlyoutBase.SetAttachedFlyout(button, menu);
                button.Click += (_, _) => ShowOptionsMenu(button, menu);
            });
    }

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
