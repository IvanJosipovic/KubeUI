using KubeUI.Features.Resources;

namespace KubeUI.Features.Resources.Common;

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
}
