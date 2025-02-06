using k8s.Models;
using k8s;
using KubeUI.Controls;
using KubeUI.Client;
using Humanizer;

namespace KubeUI.Resources;

public abstract partial class ResourceConfigBase<T> : ObservableObject, IResourceConfig where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public Type Type { get; } = typeof(T);

    public GroupApiVersionKind GroupApiVersionKind { get; } = GroupApiVersionKind.From<T>();

    public virtual string Name => GroupApiVersionKind.Kind.Humanize(LetterCasing.Title).Pluralize();

    public virtual string? Category { get; } = null;

    public virtual bool DefaultMenuItems { get; } = true;

    public virtual bool ShowNewResource { get; } = true;

    public virtual bool ShowNamespaces { get; } = true;

    public virtual int Order { get; }

    public virtual StyleGroup ListStyle() => [];

    public virtual IList<IResourceListColumn> Columns() =>
    [
        NameColumn(SortDirection.Ascending),
        AgeColumn()
    ];

    public virtual IList<ResourceMenuItem> MenuItems()=> [];

    public virtual Control[] Properties(T resource)=> [];

    protected ResourceListColumn<T, string> NameColumn(SortDirection sort = SortDirection.None)
    {
        return new ResourceListColumn<T, string>()
        {
            Name = "Name",
            Field = x => x.Metadata.Name,
            Width = "2*",
            Sort = sort,
        };
    }

    protected ResourceListColumn<T, string> NamespaceColumn()
    {
        return new ResourceListColumn<T, string>()
        {
            Name = "Namespace",
            Field = x => x.Metadata.NamespaceProperty,
            Width = "*",
        };
    }

    protected ResourceListColumn<T, DateTime?> AgeColumn()
    {
        return new ResourceListColumn<T, DateTime?>()
        {
            Name = "Age",
            CustomControl = typeof(AgeCell),
            Field = x => x.Metadata.CreationTimestamp,
            Display = x => x.Metadata.CreationTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            Width = "80"
        };
    }

    public static readonly string sRestartControllerPatch = $$"""
    {
        "spec": {
            "template": {
                "metadata": {
                    "annotations": {
                        "kubectl.kubernetes.io/restartedAt": "{{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}}"
                    }
                }
            }
        }
    }
    """;
}

public interface IResourceListColumn
{
    string Name { get; set; }

    public SortDirection Sort { get; set; }

    public Type? CustomControl { get; set; }

    public string? Width { get; set; }
}

public class ResourceListColumn<T, T2> : IResourceListColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public required string Name { get; set; }

    public required Func<T, T2> Field { get; set; }

    public Func<T, string>? Display { get; set; }

    public SortDirection Sort { get; set; }

    public Type? CustomControl { get; set; }

    public string? Width { get; set; }
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}

public class ResourceMenuItem
{
    public string? Header { get; set; }

    public IBinding? HeaderBinding { get; set; }

    public string? CommandPath { get; set; }

    public string? CommandParameterPath { get; set; }

    public bool? CommandParameterAddSelectedItem { get; set; }

    public IBinding? CommandParameterBinding { get; set; }

    public string? ItemSourcePath { get; set; }

    public string? IconResource { get; set; }

    public ResourceMenuItem? ItemTemplate { get; set; }

    public IList<ResourceMenuItem> MenuItems { get; set; }
}
