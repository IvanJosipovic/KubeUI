using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Resources;

/// <summary>Base for typed, statically registered Kubernetes resource configurations.</summary>
public abstract partial class ResourceConfig<T> : ResourceConfigBase<T>
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    /// <summary>Creates a config whose services are supplied when its cluster is initialized.</summary>
    protected ResourceConfig()
        : base()
    {
    }

    /// <summary>Creates a typed configuration for a statically registered resource kind.</summary>
    /// <param name="serviceProvider">Application services used by shared resource actions.</param>
    protected ResourceConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    /// <inheritdoc />
    public override Task SeedResource(bool waitForReady, CancellationToken cancellationToken)
        => Cluster.Runtime.SeedResource<T>(waitForReady, cancellationToken);

    /// <inheritdoc />
    public override void Initialize(ClusterWorkspace cluster)
    {
        base.Initialize(cluster);
        RegisterResourceModel(cluster);
    }

    /// <summary>Registers the statically typed model used by this resource configuration.</summary>
    /// <param name="cluster">Workspace that owns the model catalog.</param>
    protected virtual void RegisterResourceModel(ClusterWorkspace cluster)
    {
        cluster.Runtime.ModelCatalog.RegisterResource(
            Kind,
            typeof(T),
            (waitForReady, cancellationToken) => cluster.Runtime.SeedResource<T>(waitForReady, cancellationToken));
    }
}
