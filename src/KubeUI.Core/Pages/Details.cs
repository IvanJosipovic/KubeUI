using KubeUI.Core.Pages.Workloads.Details;
using Microsoft.AspNetCore.Components.Rendering;

namespace KubeUI.Core.Pages;

[Route("/details/{Group}/{Version}/{Kind}")]
[Route("/details/{Version}/{Kind}")]
public partial class Details : ComponentBase
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

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Namespace { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Name { get; set; }

    [Parameter]
    public bool IsInSideMenu { get; set; }

    private Type? ItemType { get; set; }

    private Type? ComponentType { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ItemType = ClusterManager.GetActiveCluster().GetResourceType(Group, Version, Kind);
        ComponentType = typeof(DetailsGeneric<>).MakeGenericType(new[] { ItemType });
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.OpenComponent(1, ComponentType);
        builder.AddAttribute(2, "Name", Name);
        builder.AddAttribute(3, "Namespace", Namespace);
        builder.AddAttribute(4, "IsInSideMenu", IsInSideMenu);
        builder.CloseComponent();
    }
}