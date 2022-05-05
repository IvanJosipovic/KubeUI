using k8s;
using k8s.Models;
using KubeUI.Core.Client;
using KubeUI.Core.Pages;
using KubeUI.Core.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace KubeUI.Core.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class ListComponent<TItem> : IDisposable where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [Inject]
    private ILogger<ListComponent<TItem>> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [CascadingParameter]
    private MainLayout MainLayout { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string? Version;

    private string? Kind;

    private string? Group;

    private List<ColumnComponent<TItem>> Columns { get; set; } = new List<ColumnComponent<TItem>>();

    private Type Type = typeof(TItem);

    protected override void OnInitialized()
    {
        ClusterManager.GetActiveCluster().OnChange += ListComponent_OnChange;

        var kube = GroupApiVersionKind.From<TItem>();

        Version = kube.ApiVersion;
        Group = kube.Group;
        Kind = kube.Kind;
    }

    private async void ListComponent_OnChange(WatchEventType eventType, GroupApiVersionKind type, object item)
    {
        if (type.Equals(GroupApiVersionKind.From<TItem>()))
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    public void AddColumn(ColumnComponent<TItem> column)
    {
        Columns.Add(column);
    }

    public void ClearColumns()
    {
        Columns.Clear();
    }

    public List<ColumnComponent<TItem>> GetColumns()
    {
        return Columns;
    }

    public void Refresh()
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        ClusterManager.GetActiveCluster().OnChange -= ListComponent_OnChange;
    }

    public void DisplayDetails(RenderFragment renderFragment)
    {
        MainLayout.RenderMenu(renderFragment);
    }

    private Dictionary<string, Type>? routeCache;

    private Dictionary<string, Type> GetRoutes()
    {
        if (routeCache != null)
        {
            return routeCache;
        }

        var newRoutes = new Dictionary<string, Type>();

        foreach (Type type in GetType().Assembly.GetTypes())
        {
            var routes = type.GetCustomAttributes(typeof(RouteAttribute), true);

            if (routes != null)
            {
                foreach (RouteAttribute route in routes)
                {
                    newRoutes.Add(route.Template.ToLower(), type);
                }
            }
        }

        routeCache = newRoutes;
        return newRoutes;
    }

    public void DisplayDetails(string @namespace, string group, string version, string kind, string name)
    {
        var routeTemplate = $"/details/{group}/{version}/{kind}";

        if (string.IsNullOrEmpty(group))
        {
            routeTemplate = $"/details/{version}/{kind}";
        }

        if (GetRoutes().ContainsKey(routeTemplate.ToLower()))
        {
            DisplayDetails(CreateRenderFragent(GetRoutes()[routeTemplate.ToLower()], new Dictionary<string, object>
            {
                { "Name", name },
                { "Namespace", @namespace },
                { "IsInSideMenu", true },
            }));

            return;
        }

        // Fall back to Default Component
        DisplayDetails(CreateRenderFragent(typeof(Details), new Dictionary<string, object>
        {
            { "Group", group },
            { "Version", version },
            { "Kind", kind },
            { "Name", name },
            { "Namespace", @namespace },
            { "IsInSideMenu", true },
        }));
    }

    private RenderFragment CreateRenderFragent(Type type, Dictionary<string, object> attributes) => builder =>
    {
        var count = 0;

        builder.OpenComponent(count++, type);

        foreach (var item in attributes)
        {
            builder.AddAttribute(count++, item.Key, item.Value);
        }

        builder.CloseComponent();
    };
}
