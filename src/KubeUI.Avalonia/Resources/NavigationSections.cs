using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Resources;

/// <summary>Built-in navigation sections shared by statically registered resource kinds.</summary>
internal static class NavigationSections
{
    public const string WorkloadsId = "workloads";
    public const string ConfigurationId = "configuration";
    public const string NetworkId = "network";
    public const string StorageId = "storage";
    public const string AccessControlId = "access-control";
    public const string CustomResourcesId = "custom-resources";

    public static NavigationSection Workloads { get; } = new(
        WorkloadsId, Assets.Resources.ResourceConfig_Category_Workloads!, 8, null);

    public static NavigationSection Configuration { get; } = new(
        ConfigurationId, Assets.Resources.ResourceConfig_Category_Configuration!, 9, null);

    public static NavigationSection Network { get; } = new(
        NetworkId, Assets.Resources.ResourceConfig_Category_Network!, 10, null);

    public static NavigationSection Storage { get; } = new(
        StorageId, Assets.Resources.ResourceConfig_Category_Storage!, 12, null);

    public static NavigationSection AccessControl { get; } = new(
        AccessControlId, Assets.Resources.ResourceConfig_Category_AccessControl!, 13, null);

    public static NavigationSection CustomResources { get; } = new(
        CustomResourcesId, Assets.Resources.ResourceConfig_Category_CustomResourceDefinitions!, 14, null);

    /// <summary>Order used by navigation sections that follow the custom resources section.</summary>
    public static int AfterCustomResourcesOrder => CustomResources.Order + 1;

    /// <summary>Gets all built-in sections in display order.</summary>
    public static IReadOnlyList<NavigationSection> BuiltIn { get; } =
    [
        Workloads,
        Configuration,
        Network,
        Storage,
        AccessControl,
        CustomResources,
    ];

    /// <summary>Finds a built-in section by id.</summary>
    public static NavigationSection? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        foreach (var section in BuiltIn)
        {
            if (string.Equals(section.Id, id, StringComparison.Ordinal))
            {
                return section;
            }
        }

        return null;
    }
}
