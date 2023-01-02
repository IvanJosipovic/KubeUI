using KubeUI.UI.Shared;

namespace KubeUI.UI.Components;

public partial class DetailsComponent<TItem> : IDisposable where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [Inject]
    private ILogger<DetailsComponent<TItem>> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [CascadingParameter]
    private MainLayout MainLayout { get; set; }

    [Parameter]
    [EditorRequired]
    public string Name { get; set; }

    [Parameter]
    public string? Namespace { get; set; }

    [Parameter]
    public bool IsInSideMenu { get; set; }

    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    private TItem? Item { get; set; }

    private GroupApiVersionKind groupApiVersionKind;

    protected override void OnInitialized()
    {
        ClusterManager.GetActiveCluster().OnChange += DetailsComponent_OnChange;
        groupApiVersionKind = GroupApiVersionKind.From<TItem>();
    }

    private async void DetailsComponent_OnChange(WatchEventType eventType, GroupApiVersionKind type, object item)
    {
        if (type.Equals(groupApiVersionKind))
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateData();
    }

    private void UpdateData()
    {
        Item = ClusterManager.GetActiveCluster().GetObject<TItem>(Namespace, Name);
    }

    public void Dispose()
    {
        ClusterManager.GetActiveCluster().OnChange -= DetailsComponent_OnChange;
    }
}
