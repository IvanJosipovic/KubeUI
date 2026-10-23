using System.Windows.Input;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Shell.Main;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Main;

public sealed class MainWindowTests
{
    [AvaloniaFact]
    public void managed_window_centers_fluent_title_and_options_opens_shared_menu()
    {
        using var scope = Application.Current.GetTestServices().CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<MainViewModel>();
        var window = new MainWindow(useNativeChrome: false).DataContext(vm);
        try
        {
            var theme = window.WindowDecorationsTheme.ShouldNotBeNull();
            theme.BasedOn.ShouldNotBeNull();
            var template = theme.Setters
                .OfType<Setter>()
                .Single(setter => setter.Property == WindowDrawnDecorations.TemplateProperty)
                .Value
                .ShouldBeAssignableTo<IWindowDrawnDecorationsTemplate>();
            var result = (TemplateResult<WindowDrawnDecorationsContent>)((ITemplate)template).Build();
            var overlay = result.Result.Overlay.ShouldNotBeNull();
            var titlePanel = overlay.GetVisualDescendants()
                .OfType<Panel>()
                .Single(panel => panel.Name == "PART_TitleTextPanel");
            var title = titlePanel.GetVisualDescendants().OfType<TextBlock>().Single();
            title.TextAlignment.ShouldBe(TextAlignment.Center);
            title.Margin.ShouldBe(new Thickness(0));

            var options = overlay.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => AutomationProperties.GetName(button) == Assets.Resources.MainWindow_Options);
            WindowDecorationProperties.GetElementRole(options).ShouldBe(WindowDecorationsElementRole.User);
            ToolTip.GetTip(options).ShouldBe(Assets.Resources.MainWindow_Options);

            var roles = overlay.GetVisualDescendants()
                .OfType<Button>()
                .Select(WindowDecorationProperties.GetElementRole)
                .ToArray();
            roles.ShouldContain(WindowDecorationsElementRole.MinimizeButton);
            roles.ShouldContain(WindowDecorationsElementRole.MaximizeButton);
            roles.ShouldContain(WindowDecorationsElementRole.CloseButton);

            window.Content(overlay);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var menu = FlyoutBase.GetAttachedFlyout(options).ShouldBeOfType<MenuFlyout>();
            var popupOpened = false;
            menu.Popup.Opened += (_, _) => popupOpened = true;
            var clickPoint = options.TranslatePoint(
                new Point(options.Bounds.Width / 2, options.Bounds.Height / 2),
                window) ?? throw new InvalidOperationException("Options button has no window position.");
            window.MouseMove(clickPoint);
            window.MouseDown(clickPoint, MouseButton.Left);
            window.MouseUp(clickPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            popupOpened.ShouldBeTrue();
            menu.IsOpen.ShouldBeTrue();
            menu.Popup.IsOpen.ShouldBeTrue();
            menu.Target.ShouldBeSameAs(window);
            menu.Popup.PlacementTarget.ShouldBeSameAs(window);
            menu.PlacementGravity.ShouldBe(PopupGravity.BottomRight);
            menu.Popup.PlacementRect.ShouldBe(new Rect(
                options.TranslatePoint(default, window).ShouldNotBeNull(),
                options.Bounds.Size));
            GetCommands(menu).ShouldBe(ExpectedCommands(vm), ignoreOrder: true);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void options_menu_rejects_detached_decoration_button()
    {
        using var scope = Application.Current.GetTestServices().CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<MainViewModel>();
        var window = new MainWindow(useNativeChrome: false).DataContext(vm);
        try
        {
            var template = window.WindowDecorationsTheme!.Setters
                .OfType<Setter>()
                .Single(setter => setter.Property == WindowDrawnDecorations.TemplateProperty)
                .Value
                .ShouldBeAssignableTo<IWindowDrawnDecorationsTemplate>();
            var overlay = ((TemplateResult<WindowDrawnDecorationsContent>)((ITemplate)template).Build())
                .Result.Overlay.ShouldNotBeNull();
            var options = overlay.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => AutomationProperties.GetName(button) == Assets.Resources.MainWindow_Options);

            var exception = Should.Throw<InvalidOperationException>(() =>
                options.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));
            exception.Message.ShouldBe("The Options button is not in the window visual tree.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void mac_window_keeps_native_decorations_and_native_menu()
    {
        using var scope = Application.Current.GetTestServices().CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<MainViewModel>();
        var window = new MainWindow(useNativeChrome: true).DataContext(vm);
        try
        {
            window.WindowDecorations.ShouldBe(WindowDecorations.Full);
            window.WindowDecorationsTheme.ShouldBeNull();
            window.ExtendClientAreaToDecorationsHint.ShouldBeFalse();
            var menu = NativeMenu.GetMenu(window).ShouldNotBeNull();
            menu.Items.Count.ShouldBe(3);
            GetCommands(menu.Items.Cast<NativeMenuItem>())
                .ShouldBe(ExpectedCommands(vm), ignoreOrder: true);

            window.Show();
            window.GetVisualDescendants().OfType<Menu>().ShouldBeEmpty();

            window.DataContext = null;
            NativeMenu.GetMenu(window).ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    private static ICommand[] GetCommands(MenuFlyout menu)
    {
        return menu.Items.OfType<MenuItem>()
            .SelectMany(group => group.Items.OfType<MenuItem>())
            .Select(item => item.Command)
            .Where(command => command is not null)
            .Cast<ICommand>()
            .ToArray();
    }

    private static ICommand[] GetCommands(IEnumerable<NativeMenuItem> groups)
    {
        return groups.SelectMany(group => group.Menu!.Items.OfType<NativeMenuItem>())
            .Select(item => item.Command)
            .Where(command => command is not null)
            .Cast<ICommand>()
            .ToArray();
    }

    private static ICommand[] ExpectedCommands(MainViewModel vm) =>
    [
        vm.LoadKubeConfigCommand, vm.ImportAksClusterCommand, vm.OpenClustersCommand, vm.OpenSettingsCommand,
        vm.CloseCommand, vm.ResetLayoutCommand, vm.SwitchThemeCommand, vm.CheckForUpdatesCommand, vm.OpenAboutCommand
    ];
}
