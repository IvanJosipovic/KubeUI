using Avalonia.Data;
using FluentIcons.Common;

namespace KubeUI.Avalonia.Resources;

public interface IResourceListColumn
{
    string Name { get; }
    string? Width { get; }
    SortDirection Sort { get; set; }
    Type CustomControl { get; }
    Type ItemType { get; }
    Type ValueType { get; }
    Func<object, IComparable?> SortKey { get; }
    Func<object, string> DisplayValue { get; }
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
    public Icon? FluentIcon { get; set; }
    public ResourceMenuItem? ItemTemplate { get; set; }
    public IList<ResourceMenuItem> MenuItems { get; set; } = [];
}

