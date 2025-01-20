using k8s.Models;
using k8s;

namespace KubeUI.Resources;

public abstract partial class ResourceConfigBase<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public bool DefaultMenuItems { get; set; } = true;

    public bool ShowNewResource { get; set; } = true;

    public bool ShowNamespaces { get; set; } = true;

    public abstract IList<IResourceListViewDefinitionColumn> Columns();

    public abstract IList<ResourceListViewMenuItem> MenuItems();

    public abstract object? Properties(T resource);

    public Func<StyleGroup>? SetStyle { get; set; } = () => [];
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
