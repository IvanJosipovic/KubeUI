namespace KubeUI.Avalonia.Resources;

internal static class ResourceCategories
{
    private static readonly IReadOnlyDictionary<string, int> s_orders = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [Assets.Resources.ResourceConfig_Category_Workloads!] = 8,
        [Assets.Resources.ResourceConfig_Category_Configuration!] = 9,
        [Assets.Resources.ResourceConfig_Category_Network!] = 10,
        [Assets.Resources.ResourceConfig_Category_Storage!] = 11,
        [Assets.Resources.ResourceConfig_Category_AccessControl!] = 12,
        [Assets.Resources.ResourceConfig_Category_CustomResourceDefinitions!] = 13,
    };

    public static int GetOrder(string? category, int fallbackOrder)
    {
        return !string.IsNullOrWhiteSpace(category) && s_orders.TryGetValue(category, out var order)
            ? order
            : fallbackOrder;
    }

    public static string Network => Assets.Resources.ResourceConfig_Category_Network!;

    public static string CustomResourceDefinitions => Assets.Resources.ResourceConfig_Category_CustomResourceDefinitions!;

    public static int CustomResourceDefinitionsNavigationOrder => GetOrder(CustomResourceDefinitions, 13);
}
