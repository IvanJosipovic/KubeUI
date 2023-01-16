using KubeUI.UI.Pages.Workloads.List;
using Microsoft.AspNetCore.Components.Rendering;

namespace KubeUI.UI.Pages;

[Route("/list/{Group}/{Version}/{Kind}")]
[Route("/list/{Version}/{Kind}")]
public partial class List : ComponentBase
{
    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Parameter]
    public string Group { get; set; } = "";

    [Parameter]
    [EditorRequired]
    public string Version { get; set; }

    [Parameter]
    [EditorRequired]
    public string Kind { get; set; }

    private Type? ItemType { get; set; }

    private Type? ComponentType { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ItemType = ClusterBase.GetResourceType(Group, Version, Kind);
        ComponentType = typeof(ListGeneric<>).MakeGenericType(new[] { ItemType });
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.OpenComponent(1, ComponentType);
        builder.CloseComponent();
    }
}