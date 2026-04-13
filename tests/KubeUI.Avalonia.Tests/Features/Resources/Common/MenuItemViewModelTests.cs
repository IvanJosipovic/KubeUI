using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using KubeUI.Avalonia.Features.Resources.Common;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Common;

public sealed class MenuItemViewModelTests
{
    [Fact]
    public void group_menu_items_without_children_are_hidden()
    {
        MenuItemViewModel menuItem = new()
        {
            Header = "Group",
            Items = new AvaloniaList<MenuItemViewModel>()
        };

        menuItem.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void button_menu_items_stay_visible_even_without_children()
    {
        MenuItemViewModel menuItem = new()
        {
            Header = "Action",
            Command = new RelayCommand(static () => { }),
            Items = new AvaloniaList<MenuItemViewModel>()
        };

        menuItem.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void group_menu_items_with_children_are_visible()
    {
        MenuItemViewModel menuItem = new()
        {
            Header = "Group",
            Items = new AvaloniaList<MenuItemViewModel>
            {
                new()
                {
                    Header = "Child"
                }
            }
        };

        menuItem.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void group_menu_items_become_visible_when_children_are_added_later()
    {
        MenuItemViewModel menuItem = new()
        {
            Header = "Group",
            Items = new AvaloniaList<MenuItemViewModel>()
        };

        menuItem.IsVisible.ShouldBeFalse();

        menuItem.Items!.Add(new MenuItemViewModel
        {
            Header = "Child"
        });

        menuItem.IsVisible.ShouldBeTrue();
    }
}
