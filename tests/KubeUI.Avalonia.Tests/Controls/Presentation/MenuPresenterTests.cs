using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Controls.Presentation;

public sealed class MenuPresenterTests
{
    [AvaloniaFact]
    public void menu_control_preserves_command_parameter_and_can_execute()
    {
        var parameter = new object();
        object? executedParameter = null;
        var canExecute = false;
        var command = new RelayCommand<object>(value => executedParameter = value,
            value => canExecute && ReferenceEquals(value, parameter));
        var model = new MenuItemViewModel
        {
            Title = "Run",
            Command = command,
            CommandParameter = parameter
        };
        var item = MenuPresenter.CreateMenuControl(model).ShouldBeOfType<MenuItem>();
        var menu = new ContextMenu();
        menu.Items.Add(item);
        var target = new Border { ContextMenu = menu };
        var window = new Window { Content = target };
        try
        {
            window.Show();
            menu.Open(target);
            Dispatcher.UIThread.RunJobs();

            item.Header.ShouldBe("Run");
            item.Command.ShouldBeSameAs(command);
            item.CommandParameter.ShouldBeSameAs(parameter);
            item.Command!.CanExecute(item.CommandParameter).ShouldBeFalse();
            item.IsEffectivelyEnabled.ShouldBeFalse();

            canExecute = true;
            command.NotifyCanExecuteChanged();
            Dispatcher.UIThread.RunJobs();

            item.IsEffectivelyEnabled.ShouldBeTrue();
            item.Command.CanExecute(item.CommandParameter).ShouldBeTrue();
            item.Command.Execute(item.CommandParameter);
            executedParameter.ShouldBeSameAs(parameter);
        }
        finally
        {
            menu.Close();
            window.Close();
        }
    }

    [AvaloniaFact]
    public void menu_control_preserves_hidden_empty_groups_visible_submenus_and_separators()
    {
        var empty = MenuPresenter.CreateMenuControl(new MenuItemViewModel
        {
            Title = "Empty",
            Items = new AvaloniaList<MenuItemViewModel>()
        }).ShouldBeOfType<MenuItem>();
        empty.Command.ShouldBeNull();
        empty.IsVisible.ShouldBeFalse();
        empty.Items.ShouldBeEmpty();

        var group = MenuPresenter.CreateMenuControl(new MenuItemViewModel
        {
            Title = "Group",
            Items = new AvaloniaList<MenuItemViewModel>
            {
                new() { IsSeparator = true }
            }
        }).ShouldBeOfType<MenuItem>();
        group.Command.ShouldBeNull();
        group.IsVisible.ShouldBeTrue();
        group.Items.Single().ShouldBeOfType<Separator>().IsVisible.ShouldBeTrue();

        MenuPresenter.CreateMenuControl(new MenuItemViewModel { IsSeparator = true })
            .ShouldBeOfType<Separator>().IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void flyout_renders_nested_items_in_order_with_gesture_hints()
    {
        var gesture = new KeyGesture(Key.K, KeyModifiers.Control);
        var command = new RelayCommand(static () => { });
        var flyout = MenuPresenter.CreateFlyout(new[]
        {
            new MenuItemViewModel
            {
                Title = "Root",
                Items = new AvaloniaList<MenuItemViewModel>
                {
                    new()
                    {
                        Title = "Nested",
                        Items = new AvaloniaList<MenuItemViewModel>
                        {
                            new() { Title = "Action", Command = command, InputGesture = gesture }
                        }
                    },
                    new() { IsSeparator = true },
                    new() { Title = "Last", Command = command }
                }
            }
        });

        var root = flyout.Items.Single().ShouldBeOfType<MenuItem>();
        root.Header.ShouldBe("Root");
        root.Items.Count.ShouldBe(3);
        var nested = root.Items[0].ShouldBeOfType<MenuItem>();
        nested.Header.ShouldBe("Nested");
        var action = nested.Items.Single().ShouldBeOfType<MenuItem>();
        action.Header.ShouldBe("Action");
        action.Command.ShouldBeSameAs(command);
        action.InputGesture.ShouldBeSameAs(gesture);
        root.Items[1].ShouldBeOfType<Separator>();
        root.Items[2].ShouldBeOfType<MenuItem>().Header.ShouldBe("Last");
    }

    [AvaloniaFact]
    public void flyout_and_menu_controls_are_snapshots_not_model_bindings()
    {
        var originalCommand = new RelayCommand(static () => { });
        var originalParameter = new object();
        var originalGesture = new KeyGesture(Key.K, KeyModifiers.Control);
        var child = new MenuItemViewModel { Title = "Original child", Command = originalCommand };
        var model = new MenuItemViewModel
        {
            Title = "Original",
            Command = originalCommand,
            CommandParameter = originalParameter,
            InputGesture = originalGesture,
            FluentIcon = Icon.Add,
            Items = new AvaloniaList<MenuItemViewModel> { child }
        };
        var source = new AvaloniaList<MenuItemViewModel> { model };
        var flyout = MenuPresenter.CreateFlyout(source);
        var standalone = MenuPresenter.CreateMenuControl(model).ShouldBeOfType<MenuItem>();

        source.Clear();
        child.Title = "Changed child";
        model.Title = "Changed";
        model.Command = null;
        model.CommandParameter = new object();
        model.InputGesture = new KeyGesture(Key.F2);
        model.FluentIcon = Icon.Delete;
        model.Items!.Clear();
        model.IsVisible.ShouldBeFalse();

        flyout.Items.Count.ShouldBe(1);
        foreach (var item in new[] { standalone, flyout.Items[0].ShouldBeOfType<MenuItem>() })
        {
            item.Header.ShouldBe("Original");
            item.Command.ShouldBeSameAs(originalCommand);
            item.CommandParameter.ShouldBeSameAs(originalParameter);
            item.InputGesture.ShouldBeSameAs(originalGesture);
            item.Icon.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.Add);
            item.IsVisible.ShouldBeTrue();
            item.Items.Single().ShouldBeOfType<MenuItem>().Header.ShouldBe("Original child");
        }
    }

    [AvaloniaFact]
    public void icons_prefer_fluent_icons_then_application_geometry_resources()
    {
        var resources = Application.Current.ShouldNotBeNull().Resources;
        var key = $"MenuPresenterTests.Geometry.{Guid.NewGuid():N}";
        var geometry = Geometry.Parse("M0,0 L10,0 L10,10 Z");
        resources.Add(key, geometry);
        try
        {
            var model = new MenuItemViewModel { IconResource = key, FluentIcon = Icon.Add };
            MenuPresenter.CreateIcon(model).ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.Add);

            model.FluentIcon = null;
            MenuPresenter.CreateIcon(model).ShouldBeOfType<PathIcon>().Data.ShouldBeSameAs(geometry);
        }
        finally
        {
            resources.Remove(key);
        }
    }

    [AvaloniaFact]
    public void missing_blank_and_non_geometry_icon_resources_produce_no_icon()
    {
        var resources = Application.Current.ShouldNotBeNull().Resources;
        var key = $"MenuPresenterTests.NonGeometry.{Guid.NewGuid():N}";
        resources.Add(key, "not geometry");
        try
        {
            foreach (var resource in new[] { null, string.Empty, "   ", $"{key}.Missing", key })
            {
                MenuPresenter.CreateIcon(new MenuItemViewModel { IconResource = resource }).ShouldBeNull();
            }
        }
        finally
        {
            resources.Remove(key);
        }
    }

    [AvaloniaFact]
    public void context_menu_generated_containers_follow_live_model_properties()
    {
        var parameter = new object();
        var command = new RelayCommand<object>(static _ => { });
        var model = new MenuItemViewModel { Title = "Before", Command = command, FluentIcon = Icon.Add };
        var menu = MenuPresenter.CreateContextMenu();
        menu.ItemsSource = new AvaloniaList<MenuItemViewModel> { model };
        var target = new Border { ContextMenu = menu };
        var window = new Window { Content = target };
        try
        {
            window.Show();
            menu.Open(target);
            Dispatcher.UIThread.RunJobs();

            var item = menu.ContainerFromIndex(0).ShouldBeOfType<MenuItem>();
            item.Header.ShouldBe("Before");
            item.Command.ShouldBeSameAs(command);
            item.Icon.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.Add);
            item.IsVisible.ShouldBeTrue();

            var replacement = new RelayCommand<object>(static _ => { });
            var gesture = new KeyGesture(Key.F2);
            model.Title = "After";
            model.Command = replacement;
            model.CommandParameter = parameter;
            model.InputGesture = gesture;
            model.FluentIcon = Icon.Delete;
            Dispatcher.UIThread.RunJobs();

            item.Header.ShouldBe("After");
            item.Command.ShouldBeSameAs(replacement);
            item.CommandParameter.ShouldBeSameAs(parameter);
            item.InputGesture.ShouldBeSameAs(gesture);
            item.Icon.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.Delete);

            model.Command = null;
            Dispatcher.UIThread.RunJobs();
            item.Command.ShouldBeNull();
            item.IsVisible.ShouldBeFalse();

            model.Items = new AvaloniaList<MenuItemViewModel>
            {
                new() { Title = "Child", Command = command }
            };
            Dispatcher.UIThread.RunJobs();
            item.IsVisible.ShouldBeTrue();
            item.Items.Count.ShouldBe(1);
            model.Items.Add(new MenuItemViewModel { Title = "Second child", Command = command });
            Dispatcher.UIThread.RunJobs();
            item.Items.Count.ShouldBe(2);
        }
        finally
        {
            menu.Close();
            window.Close();
        }
    }

    [AvaloniaFact]
    public void context_menu_updates_geometry_icons_and_renders_model_backed_submenus_and_separators()
    {
        var resources = Application.Current.ShouldNotBeNull().Resources;
        var key = $"MenuPresenterTests.LiveGeometry.{Guid.NewGuid():N}";
        var geometry = Geometry.Parse("M0,0 L8,0 L8,8 Z");
        resources.Add(key, geometry);
        var child = new MenuItemViewModel
        {
            Title = "Child",
            Command = new RelayCommand(static () => { }),
            InputGesture = new KeyGesture(Key.F2)
        };
        var model = new MenuItemViewModel
        {
            Title = "Group",
            FluentIcon = Icon.Add,
            Items = new AvaloniaList<MenuItemViewModel> { child, new() { IsSeparator = true } }
        };
        var menu = MenuPresenter.CreateContextMenu();
        menu.ItemsSource = new[] { model };
        var target = new Border { ContextMenu = menu };
        var window = new Window { Content = target };
        try
        {
            window.Show();
            menu.Open(target);
            Dispatcher.UIThread.RunJobs();
            var item = menu.ContainerFromIndex(0).ShouldBeOfType<MenuItem>();
            model.IconResource = key;
            model.FluentIcon = null;
            Dispatcher.UIThread.RunJobs();
            item.Icon.ShouldBeOfType<PathIcon>().Data.ShouldBeSameAs(geometry);

            model.IconResource = $"{key}.Missing";
            Dispatcher.UIThread.RunJobs();
            item.Icon.ShouldBeNull();

            item.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            var childItem = item.ContainerFromIndex(0).ShouldBeOfType<MenuItem>();
            childItem.Header.ShouldBe("Child");
            childItem.Command.ShouldBeSameAs(child.Command);
            childItem.InputGesture.ShouldBeSameAs(child.InputGesture);
            var separator = item.ContainerFromIndex(1).ShouldBeOfType<MenuItem>();
            separator.IsVisible.ShouldBeTrue();
            separator.GetVisualDescendants().OfType<Separator>().ShouldNotBeEmpty();

            child.Title = "Updated child";
            Dispatcher.UIThread.RunJobs();
            childItem.Header.ShouldBe("Updated child");
        }
        finally
        {
            menu.Close();
            window.Close();
            resources.Remove(key);
        }
    }

    [AvaloniaFact]
    public void input_gesture_is_a_hint_not_a_window_hotkey()
    {
        var executions = 0;
        var item = MenuPresenter.CreateMenuControl(new MenuItemViewModel
        {
            Title = "Action",
            Command = new RelayCommand(() => executions++),
            InputGesture = new KeyGesture(Key.K, KeyModifiers.Control)
        }).ShouldBeOfType<MenuItem>();
        var menu = new Menu();
        menu.Items.Add(item);
        var window = new Window { Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            item.InputGesture.ShouldNotBeNull();
            HotKeyManager.GetHotKey(item).ShouldBeNull();

            window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.K, "k");
            window.KeyRelease(Key.K, RawInputModifiers.Control, PhysicalKey.K, "k");
            Dispatcher.UIThread.RunJobs();
            executions.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void null_arguments_are_rejected_and_empty_flyouts_are_supported()
    {
        Should.Throw<ArgumentNullException>(() => MenuPresenter.CreateFlyout(null!));
        Should.Throw<ArgumentNullException>(() => MenuPresenter.CreateMenuControl(null!));
        Should.Throw<ArgumentNullException>(() => MenuPresenter.CreateIcon(null!));
        Should.Throw<ArgumentNullException>(() => MenuPresenter.CreateFlyout(new MenuItemViewModel[] { null! }));
        Should.Throw<ArgumentNullException>(() => MenuPresenter.CreateMenuControl(new MenuItemViewModel
        {
            Items = new AvaloniaList<MenuItemViewModel> { null! }
        }));
        MenuPresenter.CreateFlyout(Array.Empty<MenuItemViewModel>()).Items.ShouldBeEmpty();
    }
}
