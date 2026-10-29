using System.Windows.Input;
using FluentIcons.Common;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;
using Swordfish.NET.Collections;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>One fixed cluster-level navigation link.</summary>
/// <param name="Key">Stable key used in the link id and target resolution.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to siblings.</param>
/// <param name="Icon">Link icon.</param>
internal sealed record NavigationTarget(string Key, string Name, int Order, Icon Icon);

public static class NavigationTargets
{
    public const string ClusterSettings = "cluster-settings";
    public const string ClusterWorkspace = "cluster-workspace";
    public const string PortForwarders = "port-forwarders";
    public const string Visualization = "visualization";
    public const string LoadYaml = "load-yaml";
    public const string LoadFolder = "load-folder";

    /// <summary>Gets the fixed links every connected cluster shows above its sections.</summary>
    internal static IReadOnlyList<NavigationTarget> FixedClusterLinks { get; } =
    [
        new(ClusterWorkspace, Assets.Resources.ClusterView_Title!, -500, Icon.Desktop),
        new(Visualization, Assets.Resources.VisualizationView_Title!, -490, Icon.DataUsage),
        new(ClusterSettings, Assets.Resources.ClusterSettingsView_Title!, -480, Icon.Settings),
        new(LoadYaml, Assets.Resources.NavigationView_LoadYaml!, -470, Icon.ArrowUpload),
        new(LoadFolder, Assets.Resources.NavigationView_LoadFolder!, -460, Icon.FolderAdd),
    ];

    internal static NavigationTarget Find(string key)
        => FixedClusterLinks.First(target => string.Equals(target.Key, key, StringComparison.Ordinal));
}

public interface IExpandableNavigationNode
{
    bool IsExpanded { get; set; }
}

public partial class ClusterNavigationNode : NavigationItem, IDisposable
{
    private string _runtimeName;

    public ClusterNavigationNode(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        _runtimeName = cluster.Runtime.Name;
        if (cluster.Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged += OnRuntimePropertyChanged;
        }
        UpdateConnectionNavigation(cluster.Runtime.Connected);
    }

    public ClusterWorkspace Cluster { get; }

    public string ConnectionMenuHeader => Cluster.Runtime.Connected
        ? Assets.Resources.NavigationView_ContextMenu_Disconnect!
        : Assets.Resources.NavigationView_ContextMenu_Connect!;

    public Icon ConnectionMenuIcon => Cluster.Runtime.Connected
        ? Icon.Dismiss
        : Icon.Link;

    /// <summary>
    /// Gets the context-menu header for the expand/collapse action.
    /// </summary>
    public string ExpansionMenuHeader => IsExpanded
        ? Assets.Resources.NavigationView_ContextMenu_Collapse!
        : Assets.Resources.NavigationView_ContextMenu_Expand!;

    /// <summary>
    /// Gets the context-menu icon for the expand/collapse action.
    /// </summary>
    public Icon ExpansionMenuIcon => IsExpanded
        ? Icon.ChevronUp
        : Icon.ChevronDown;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsExpanded))
        {
            OnPropertyChanged(nameof(ExpansionMenuHeader));
            OnPropertyChanged(nameof(ExpansionMenuIcon));
        }
    }

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IClusterRuntime.Connected))
        {
            OnPropertyChanged(nameof(ConnectionMenuHeader));
            OnPropertyChanged(nameof(ConnectionMenuIcon));
        }
    }

    [ObservableProperty]
    public partial ICommand? ToggleConnectionCommand { get; set; }

    [ObservableProperty]
    public partial ICommand? OpenSettingsCommand { get; set; }

    /// <summary>
    /// Gets or sets the command that expands or collapses the cluster node.
    /// </summary>
    [ObservableProperty]
    public partial ICommand? ToggleExpansionCommand { get; set; }

    internal void UpdateNavigationIds(string oldName, string newName)
    {
        if (string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            return;
        }

        var oldPrefix = oldName + "-";
        foreach (var item in NavigationItems)
        {
            UpdateNavigationId(item, oldPrefix, newName);
        }
    }

    internal void UpdateNavigationName(string newName)
    {
        UpdateNavigationIds(_runtimeName, newName);
        _runtimeName = newName;
    }

    private static void UpdateNavigationId(NavigationItem item, string oldPrefix, string newName)
    {
        if (item.Id.StartsWith(oldPrefix, StringComparison.Ordinal))
        {
            item.Id = newName + item.Id[(oldPrefix.Length - 1)..];
        }

        foreach (var child in item.NavigationItems)
        {
            UpdateNavigationId(child, oldPrefix, newName);
        }
    }

    internal void UpdateConnectionNavigation(bool connected)
    {
        NavigationItems.Clear();
        if (!connected)
        {
            return;
        }

        foreach (var target in NavigationTargets.FixedClusterLinks)
        {
            NavigationItems.Add(new NavigationLink
            {
                Cluster = Cluster,
                Id = $"{Cluster.Runtime.Name}-{target.Key}",
                Name = target.Name,
                ViewModelKey = target.Key,
                Order = target.Order,
                FluentIcon = target.Icon,
            });
        }
    }

    public void Dispose()
    {
        if (Cluster.Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged -= OnRuntimePropertyChanged;
        }

        GC.SuppressFinalize(this);
    }
}

public partial class NavigationItem : ObservableObject, IExpandableNavigationNode
{
    [ObservableProperty]
    public partial string Id { get; set; }

    [ObservableProperty]
    public partial string? SvgIcon { get; set; }

    [ObservableProperty]
    public partial string? SvgIconDark { get; set; }

    [ObservableProperty]
    public partial string? StyleIcon { get; set; }

    [ObservableProperty]
    public partial Icon? FluentIcon { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<NavigationItem> NavigationItems { get; set; } = new ObservableSortedCollection<NavigationItem>(new NavigationItemOrderComparer());

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial int Order { get; set; }
}

public partial class NavigationLink : NavigationItem
{
    [ObservableProperty]
    public partial ClusterWorkspace Cluster { get; set; }

    [ObservableProperty]
    public partial string? ViewModelKey { get; set; }

    [ObservableProperty]
    public partial string? DocumentId { get; set; }

    [ObservableProperty]
    public partial GroupApiVersionKind? ResourceKind { get; set; }

    [ObservableProperty]
    public partial IObservable<int>? Count { get; set; }

    internal GroupApiVersionKind? CountResourceKind { get; set; }

    [ObservableProperty]
    public partial ICommand? OpenCommand { get; set; }

    [ObservableProperty]
    public partial ICommand? OpenInNewTabCommand { get; set; }

    [ObservableProperty]
    public partial IImage? ResourceIcon { get; set; }
}

public class NavigationItemNameComparer : IComparer<NavigationItem>
{
    public int Compare(NavigationItem? x, NavigationItem? y)
    {
        return x?.Name.CompareTo(y?.Name, StringComparison.Ordinal) ?? 0;
    }
}

public class NavigationItemOrderComparer : IComparer<NavigationItem>
{
    public int Compare(NavigationItem? x, NavigationItem? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var order = x.Order.CompareTo(y.Order);

        if (order != 0)
        {
            return order;
        }

        var name = string.Compare(x.Name, y.Name, StringComparison.Ordinal);

        if (name != 0)
        {
            return name;
        }

        return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
    }
}
