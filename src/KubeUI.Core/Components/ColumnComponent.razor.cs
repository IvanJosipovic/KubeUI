using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace KubeUI.Core.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class ColumnComponent<TItem> where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [Inject]
    private ILogger<ColumnComponent<TItem>> Logger { get; set; }

    [CascadingParameter]
    public ListComponent<TItem> ListComponent { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public SortDirection Sort { get; set; }

    [Parameter]
    public Func<TItem, object>? Object { get; set; }

    [Parameter]
    public RenderFragment<Tuple<TItem, ListComponent<TItem>>>? DisplayContent { get; set; }

    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    [Parameter]
    public bool Hide { get; set; }

    [Parameter]
    public string Width { get; set; }

    protected override void OnInitialized()
    {
        ListComponent.AddColumn(this);
    }
}
