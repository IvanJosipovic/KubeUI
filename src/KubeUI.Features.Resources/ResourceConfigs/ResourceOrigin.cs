namespace KubeUI.Features.Resources;

/// <summary>Identifies where a resource kind's model comes from.</summary>
public enum ResourceOrigin
{
    /// <summary>The kind is a built-in, statically registered Kubernetes resource.</summary>
    BuiltIn,

    /// <summary>The kind is discovered at runtime from a custom resource definition.</summary>
    CustomResourceDefinition,

    /// <summary>The kind is discovered from the Kubernetes API resource snapshot.</summary>
    ApiDiscovery,
}
