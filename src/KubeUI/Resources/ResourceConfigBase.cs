using k8s.Models;
using k8s;
using KubeUI.Controls;

namespace KubeUI.Resources;

public abstract partial class ResourceConfigBase<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public abstract string Category { get; }

    public bool DefaultMenuItems { get; set; } = true;

    public bool ShowNewResource { get; set; } = true;

    public bool ShowNamespaces { get; set; } = true;

    public abstract IList<IResourceListViewDefinitionColumn> Columns();

    public abstract IList<ResourceListViewMenuItem> MenuItems();

    public abstract object? Properties(T resource);

    public Func<StyleGroup>? SetStyle { get; set; } = () => [];

    public ResourceListViewDefinitionColumn<T, string> NameColumn(SortDirection sort = SortDirection.None)
    {
        return new ResourceListViewDefinitionColumn<T, string>()
        {
            Name = "Name",
            Field = x => x.Metadata.Name,
            Width = "2*",
            Sort = sort,
        };
    }

    public ResourceListViewDefinitionColumn<T, string> NamespaceColumn()
    {
        return new ResourceListViewDefinitionColumn<T, string>()
        {
            Name = "Namespace",
            Field = x => x.Metadata.NamespaceProperty,
            Width = "*",
        };
    }

    public ResourceListViewDefinitionColumn<T, DateTime?> AgeColumn()
    {
        return new ResourceListViewDefinitionColumn<T, DateTime?>()
        {
            Name = "Age",
            CustomControl = typeof(AgeCell),
            Field = x => x.Metadata.CreationTimestamp,
            Display = x => x.Metadata.CreationTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            Width = "80"
        };
    }
}

public interface IResourceListViewDefinitionColumn
{
    string Name { get; set; }

    public SortDirection Sort { get; set; }

    public Type? CustomControl { get; set; }

    public string? Width { get; set; }
}

public class ResourceListViewDefinitionColumn<T, T2> : IResourceListViewDefinitionColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
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

public class ResourceListViewMenuItem
{
    public string? Header { get; set; }

    public IBinding? HeaderBinding { get; set; }


    public string? CommandPath { get; set; }

    public string? CommandParameterPath { get; set; }

    public IBinding? CommandParameterBinding { get; set; }


    public string? ItemSourcePath { get; set; }


    public string? IconResource { get; set; }

    public ResourceListViewMenuItem? ItemTemplate { get; set; }

    public IList<ResourceListViewMenuItem> MenuItems { get; set; }
}
