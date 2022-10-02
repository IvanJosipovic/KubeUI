using System.Text.Json.JsonDiffPatch.Diffs;
using System.Linq;

namespace KubeUI.Core.Components;

public partial class DiffTree
{
    [Inject]
    private ILogger<DiffTree> Logger { get; set; }

    [Parameter]
    public JsonDiffDelta Item { get; set; }

    [Parameter]
    public EventCallback<JsonDiffDelta> ObjectSelected { get; set; }

    private HashSet<DiffItem> Items { get; set; } = new HashSet<DiffItem>();

    private DiffItem SelectedValue { get; set; }

    protected override void OnParametersSet()
    {
        var treeItems = BuildTree("Root", Item);
        Items.Clear();
        foreach (var item in treeItems)
        {
            Items.Add(item);
        }

        StateHasChanged();
    }

    private static HashSet<DiffItem> BuildTree(string name, JsonDiffDelta delta)
    {
        HashSet<DiffItem> Tree = new HashSet<DiffItem>();

        switch (delta.Kind)
        {
            case DeltaKind.None:
            case DeltaKind.Added:
            case DeltaKind.Modified:
            case DeltaKind.Deleted:
                break;
            case DeltaKind.Array:
                foreach (var item in delta.GetArrayChangeEnumerable())
                {
                    var arrayDiff = new DiffItem()
                    {
                        Name = item.Index.ToString(),
                        Items = BuildTree(item.Index.ToString(), item.Diff),
                        Delta = item.Diff,
                        Icon = GetIcon(item.Diff.Kind)
                    };

                    Tree.Add(arrayDiff);
                }
                break;
            case DeltaKind.ArrayMove:
                break;
            case DeltaKind.Object:
                if (delta.Document != null)
                {
                    foreach (var item in delta.Document.AsObject())
                    {
                        if (item.Value != null)
                        {
                            var newDelta = new JsonDiffDelta(item.Value);

                            var treeItem = new DiffItem()
                            {
                                Name = item.Key,
                                Items = BuildTree(item.Key, newDelta),
                                Delta = newDelta,
                                Icon = GetIcon(newDelta.Kind)
                            };

                            Tree.Add(treeItem);
                        }
                    }
                }
                break;
            case DeltaKind.Text:
                break;
            default:
                break;
        }

        return Tree;
    }

    private static string? GetIcon(DeltaKind deltaKind)
    {
        switch (deltaKind)
        {
            case DeltaKind.None:
                return string.Empty;
            case DeltaKind.Added:
                return MudBlazor.Icons.Material.Filled.Add;
            case DeltaKind.Modified:
                return MudBlazor.Icons.Material.Filled.Edit;
            case DeltaKind.Deleted:
                return MudBlazor.Icons.Material.Filled.Delete;
            case DeltaKind.Array:
                return string.Empty;
            case DeltaKind.ArrayMove:
                return string.Empty;
            case DeltaKind.Object:
                return string.Empty;
            case DeltaKind.Text:
                return string.Empty;
            default:
                return string.Empty;
        }
    }

    private class DiffItem
    {
        public string Name { get; set; }

        public bool IsExpanded { get; set; } = true;

        public HashSet<DiffItem> Items { get; set; } = new HashSet<DiffItem>();

        public JsonDiffDelta Delta { get; set; }

        public string Icon { get; set; }
    }
}