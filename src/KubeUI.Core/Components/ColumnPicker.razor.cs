using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace KubeUI.Core.Components;

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
