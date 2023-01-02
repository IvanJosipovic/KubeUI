namespace KubeUI.UI.Components;

public partial class ColumnPicker<TItem> where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [Inject]
    private ILogger<ColumnPicker<TItem>> Logger { get; set; }

    [CascadingParameter]
    public ListComponent<TItem> ListComponent { get; set; }

    private bool Toggle(ColumnComponent<TItem> column)
    {
        column.Hide = !column.Hide;
        ListComponent.Refresh();

        return column.Hide;
    }
}
