namespace KubeUI.UI.Pages.Workloads.List;

public class ListBase<TItem> : ComponentBase
{
    [Inject]
    protected ClusterManager ClusterManager { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Parameter]
    public bool HideNamespace { get; set; }

    [Parameter]
    public bool HideName { get; set; }

    [Parameter]
    public bool HideAge { get; set; }

    [Parameter]
    public bool MultiSelection { get; set; } = true;

    [Parameter]
    public bool HideActions { get; set; }

    [Parameter]
    public Func<IEnumerable<TItem>, IEnumerable<TItem>>? Query { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool HideNew { get; set; }

    [Parameter]
    public bool HideNamespaceSelector { get; set; }

    [Parameter]
    public HashSet<TItem> SelectedItems { get; set; } = new HashSet<TItem>();
}
