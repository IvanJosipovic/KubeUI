using System.Globalization;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FluentIcons.Avalonia;
using KubeUI.Controls.Converters;

namespace KubeUI.Controls.Presentation;

/// <summary>Renders shared menu descriptions without owning feature actions or menu composition.</summary>
public static class MenuPresenter
{
    /// <summary>Creates a context menu with live compiled bindings for generated menu-item containers.</summary>
    /// <remarks>
    /// The caller supplies the menu's items source. Item properties, child collections, visibility,
    /// separator state, and both icon properties remain bound to their menu descriptions.
    /// </remarks>
    public static ContextMenu CreateContextMenu()
    {
        var iconBinding = new MultiBinding
        {
            Converter = new MenuIconConverter(),
            Bindings =
            {
                CompiledBinding.Create<MenuItemViewModel, Icon?>(x => x.FluentIcon),
                CompiledBinding.Create<MenuItemViewModel, string?>(x => x.IconResource)
            }
        };

        return new ContextMenu()
            .Styles(
                new Style<MenuItem>()
                    .Setter(Visual.IsVisibleProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsVisible))
                    .Setter(HeaderedItemsControl.HeaderProperty, CompiledBinding.Create<MenuItemViewModel, string?>(x => x.Title))
                    .Setter(MenuItem.CommandProperty, CompiledBinding.Create<MenuItemViewModel, ICommand?>(x => x.Command))
                    .Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<MenuItemViewModel, object?>(x => x.CommandParameter))
                    .Setter(MenuItem.InputGestureProperty, CompiledBinding.Create<MenuItemViewModel, KeyGesture?>(x => x.InputGesture))
                    .Setter(ItemsControl.ItemsSourceProperty, CompiledBinding.Create<MenuItemViewModel, IEnumerable?>(x => x.Items))
                    .Setter(Control.TagProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsSeparator))
                    .Setter(MenuItem.IconProperty, iconBinding),
                new Style<MenuItem>(x => x.PropertyEquals(Control.TagProperty, true))
                    .Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, _) => new Separator())));
    }

    /// <summary>Creates a flyout containing a snapshot of the supplied menu descriptions.</summary>
    /// <remarks>
    /// No subscriptions to the descriptions or their collections are added. Features must recreate
    /// the flyout when their menu changes. Command objects retain their normal command behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The items collection or an item is null.</exception>
    public static MenuFlyout CreateFlyout(IEnumerable<MenuItemViewModel> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var flyout = new MenuFlyout();
        foreach (var item in items)
        {
            flyout.Items.Add(CreateMenuControl(item));
        }

        return flyout;
    }

    /// <summary>Creates a snapshot menu item or separator, including its current child items.</summary>
    /// <remarks>
    /// Properties and child collections are copied, not bound. Features must recreate the control
    /// when the description changes. Command objects retain their normal command behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The item or a child item is null.</exception>
    public static Control CreateMenuControl(MenuItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.IsSeparator)
        {
            return new Separator { IsVisible = item.IsVisible };
        }

        var menuItem = new MenuItem
        {
            Header = item.Title,
            Command = item.Command,
            CommandParameter = item.CommandParameter,
            InputGesture = item.InputGesture,
            Icon = CreateIcon(item),
            IsVisible = item.IsVisible
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

    /// <summary>Creates a snapshot icon, preferring a Fluent icon over a geometry resource.</summary>
    /// <remarks>Returns null when neither icon is available. No property subscriptions are added.</remarks>
    /// <exception cref="ArgumentNullException">The item is null.</exception>
    public static Control? CreateIcon(MenuItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return CreateIcon(item.FluentIcon, item.IconResource);
    }

    private static Control? CreateIcon(Icon? fluentIcon, string? iconResource)
    {
        if (fluentIcon is { } icon)
        {
            return new FluentIcon { Icon = icon };
        }

        if (!string.IsNullOrWhiteSpace(iconResource))
        {
            var data = StaticResourceConverter.Instance.Convert(
                iconResource,
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

    private sealed class MenuIconConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            Icon? fluentIcon = values.Count > 0 && values[0] is Icon icon ? icon : null;
            var iconResource = values.Count > 1 ? values[1] as string : null;
            return CreateIcon(fluentIcon, iconResource);
        }
    }
}
