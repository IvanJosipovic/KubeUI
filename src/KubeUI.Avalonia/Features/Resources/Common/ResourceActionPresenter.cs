using System.Globalization;
using FluentIcons.Avalonia;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Features.Resources.Common;

internal static class ResourceActionPresenter
{
    public static IEnumerable<MenuItemViewModel> Compose(IResourceConfig resourceConfig, IEnumerable? selectedItems)
    {
        ArgumentNullException.ThrowIfNull(resourceConfig);

        var items = new List<MenuItemViewModel>(resourceConfig.GetDefaultMenuItems(selectedItems));
        var customItems = resourceConfig.GetCustomMenuItems(selectedItems).ToList();
        if (customItems.Count > 0)
        {
            items.Add(new MenuItemViewModel
            {
                IsSeparator = true
            });
            items.AddRange(customItems);
        }

        return items;
    }

    public static MenuFlyout CreateFlyout(IEnumerable<MenuItemViewModel> items)
    {
        var flyout = new MenuFlyout();
        foreach (var item in items)
        {
            flyout.Items.Add(CreateMenuControl(item));
        }

        return flyout;
    }

    public static Control CreateMenuControl(MenuItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.IsSeparator)
        {
            return new Separator
            {
                IsVisible = item.IsVisible
            };
        }

        var menuItem = new MenuItem
        {
            Header = item.Title,
            Command = item.Command,
            CommandParameter = item.CommandParameter,
            Icon = CreateIcon(item),
            IsVisible = item.IsVisible,
        };

        if (item.Items is { Count: > 0 } children)
        {
            foreach (var child in children)
            {
                menuItem.Items.Add(CreateMenuControl(child));
            }
        }

        return menuItem;
    }

    public static Control? CreateIcon(MenuItemViewModel item)
    {
        if (item.FluentIcon is { } fluentIcon)
        {
            return new FluentIcon().Icon(fluentIcon);
        }

        if (!string.IsNullOrWhiteSpace(item.IconResource))
        {
            var data = StaticResourceConverter.Instance.Convert(
                item.IconResource,
                typeof(object),
                parameter: null,
                CultureInfo.InvariantCulture);
            if (data is Geometry geometry)
            {
                return new PathIcon { Data = geometry };
            }
        }

        return null;
    }
}
