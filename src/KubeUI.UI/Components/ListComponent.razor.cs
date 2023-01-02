namespace KubeUI.UI.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class ListComponent<TItem> : IDisposable where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [Inject]
    private ILogger<ListComponent<TItem>> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private IDialogService Dialog { get; set; }

    [CascadingParameter]
    private MainLayout MainLayout { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool MultiSelection { get; set; }

    [Parameter]
    public HashSet<TItem> SelectedItems { get; set; } = new HashSet<TItem>();

    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    [Parameter]
    public Func<IEnumerable<TItem>, IEnumerable<TItem>>? Query { get; set; }

    [Parameter]
    public string? Title { get; set; }

    private string? Version;

    private string? Kind;

    private string? Group;

    private string? SearchQuery;

    private List<ColumnComponent<TItem>> Columns { get; set; } = new List<ColumnComponent<TItem>>();

    private Type Type = typeof(TItem);

    private System.Timers.Timer Timer { get; set; }

    protected override void OnInitialized()
    {
        var kube = GroupApiVersionKind.From<TItem>();

        Version = kube.ApiVersion;
        Group = kube.Group;
        Kind = kube.Kind;

        Timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        Timer.Elapsed += Timer_Elapsed;
        Timer.Enabled = true;
        Timer.AutoReset = true;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private IEnumerable<TItem> GetData()
    {
        var items = ClusterManager.GetActiveCluster().GetObjects<TItem>();

        if (SearchQuery is not null)
        {
            items = items.Where(x => GlobalSearchQuery(x));
        }

        if (Query is not null)
        {
            items = Query.Invoke(items);
        }

        return items;
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
        Timer?.Dispose();
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

        foreach (var type in GetType().Assembly.GetTypes())
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

    public void DisplayDetails(string @namespace, string groupVersion, string kind, string name)
    {
        var group = groupVersion.Substring(0, groupVersion.IndexOf('/'));
        var version = groupVersion.Substring(groupVersion.IndexOf('/') + 1);

        DisplayDetails(@namespace, group, version, kind, name);
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

    private async Task Compare()
    {
        var left = SelectedItems.ElementAt(0);
        var right = SelectedItems.ElementAt(1);
        var parameters = new DialogParameters()
            {
                {"Left", left},
                {"Right", right},
            };
        var dialog = Dialog.Show<CompareObject>($"Compare {left.Name()} and {right.Name()}", parameters, new DialogOptions()
        {
            CloseButton = true,
            FullScreen = true
        });
    }

    private async Task New()
    {
        var parameters = new DialogParameters()
        {
        };

        var dialog = Dialog.Show<Edit<TItem>>($"New", parameters, new DialogOptions()
        {
            CloseButton = true,
            FullScreen = true
        });
    }

    private async Task Delete()
    {
        var parameters = new DialogParameters()
        {
            { "ContentText", $"Do you want to delete {SelectedItems.Count} Objects?" },
            { "ButtonText", "Delete" },
            { "Color", Color.Error }
        };
        var dialog = Dialog.Show<Dialog>("Delete", parameters, new DialogOptions()
        {
            CloseButton = true
        });

        if (!(await dialog.Result).Cancelled)
        {
            foreach (var item in SelectedItems)
            {
                await ClusterManager.GetActiveCluster().Delete(item);
            }
        }
    }

    private bool GlobalSearchQuery(TItem item)
    {
        return SearchQuery.Split(' ').All(x => GlobalSearchQuery(item, x));
    }

    private bool GlobalSearchQuery(TItem item, string query)
    {
        foreach (var column in Columns.Where(x => x.Object != null))
        {
            object obj = null;

            try
            {
                obj = column.Object.Invoke(item);
            }
            catch { }

            if (obj == null)
            {
                continue;
            }

            if (obj.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
