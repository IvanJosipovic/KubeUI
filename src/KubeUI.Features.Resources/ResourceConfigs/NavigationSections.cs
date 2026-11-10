namespace KubeUI.Features.Resources;

/// <summary>Built-in navigation sections shared by statically registered resource kinds.</summary>
public static class NavigationSections
{
    /// <summary>Stable id of the Workloads section.</summary>
    public const string WorkloadsId = "workloads";
    /// <summary>Stable id of the Configuration section.</summary>
    public const string ConfigurationId = "configuration";
    /// <summary>Stable id of the Network section.</summary>
    public const string NetworkId = "network";
    /// <summary>Stable id of the Storage section.</summary>
    public const string StorageId = "storage";
    /// <summary>Stable id of the Access Control section.</summary>
    public const string AccessControlId = "access-control";
    /// <summary>Stable id of the Custom Resources section.</summary>
    public const string CustomResourcesId = "custom-resources";
    /// <summary>Stable id of the Cilium section.</summary>
    public const string CiliumId = "cilium";

    /// <summary>Gets the Workloads section.</summary>
    public static NavigationSection Workloads { get; } = new(
        WorkloadsId, ResourcesLanguageManager.Instance.ResourceConfig_Category_Workloads.CurrentValue()!, 8, null);

    /// <summary>Gets the Configuration section.</summary>
    public static NavigationSection Configuration { get; } = new(
        ConfigurationId, ResourcesLanguageManager.Instance.ResourceConfig_Category_Configuration.CurrentValue()!, 9, null);

    /// <summary>Gets the Network section.</summary>
    public static NavigationSection Network { get; } = new(
        NetworkId, ResourcesLanguageManager.Instance.ResourceConfig_Category_Network.CurrentValue()!, 10, null);

    /// <summary>Gets the Storage section.</summary>
    public static NavigationSection Storage { get; } = new(
        StorageId, ResourcesLanguageManager.Instance.ResourceConfig_Category_Storage.CurrentValue()!, 12, null);

    /// <summary>Gets the Access Control section.</summary>
    public static NavigationSection AccessControl { get; } = new(
        AccessControlId, ResourcesLanguageManager.Instance.ResourceConfig_Category_AccessControl.CurrentValue()!, 13, null);

    /// <summary>Gets the Custom Resources section.</summary>
    public static NavigationSection CustomResources { get; } = new(
        CustomResourcesId, ResourcesLanguageManager.Instance.ResourceConfig_Category_CustomResourceDefinitions.CurrentValue()!, 14, null);

    /// <summary>Order used by navigation sections that follow the custom resources section.</summary>
    public static int AfterCustomResourcesOrder => CustomResources.Order + 1;

    /// <summary>Gets all built-in sections in display order.</summary>
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
    /// <summary>Finds a built-in section by id.</summary>
    /// <param name="id">Stable section id.</param>
    /// <returns>The matching section, or <see langword="null"/>.</returns>
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
