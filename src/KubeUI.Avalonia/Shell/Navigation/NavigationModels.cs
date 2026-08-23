using System.Windows.Input;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using Swordfish.NET.Collections;

namespace KubeUI.Avalonia.Shell.Navigation;

public static class NavigationTargets
{
    public const string ClusterSettings = "cluster-settings";
    public const string ClusterWorkspace = "cluster-workspace";
    public const string PortForwarders = "port-forwarders";
    public const string Visualization = "visualization";
    public const string LoadYaml = "load-yaml";
    public const string LoadFolder = "load-folder";
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

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IClusterRuntime.Connected))
        {
            OnPropertyChanged(nameof(ConnectionMenuHeader));
            OnPropertyChanged(nameof(ConnectionMenuIcon));
        }
        else if (e.PropertyName == nameof(IClusterRuntime.Name))
        {
            AvaloniaScheduler.Instance.Invoke(() => UpdateNavigationName(Cluster.Runtime.Name));
        }
    }

    [ObservableProperty]
    public partial ICommand? ToggleConnectionCommand { get; set; }

    [ObservableProperty]
    public partial ICommand? OpenSettingsCommand { get; set; }

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

        NavigationItems.Add(new NavigationLink
        {
            Cluster = Cluster,
            Id = $"{Cluster.Runtime.Name}-{NavigationTargets.ClusterWorkspace}",
            Name = Assets.Resources.ClusterView_Title!,
            ViewModelKey = NavigationTargets.ClusterWorkspace,
            Order = -500,
            FluentIcon = Icon.Desktop,
        });
        NavigationItems.Add(new NavigationLink
        {
            Cluster = Cluster,
            Id = $"{Cluster.Runtime.Name}-{NavigationTargets.Visualization}",
            Name = Assets.Resources.VisualizationView_Title!,
            ViewModelKey = NavigationTargets.Visualization,
            Order = -490,
            FluentIcon = Icon.DataUsage,
        });
        NavigationItems.Add(new NavigationLink
        {
            Cluster = Cluster,
            Id = $"{Cluster.Runtime.Name}-{NavigationTargets.ClusterSettings}",
            Name = Assets.Resources.ClusterSettingsView_Title!,
            ViewModelKey = NavigationTargets.ClusterSettings,
            Order = -480,
            FluentIcon = Icon.Settings,
        });
        NavigationItems.Add(new NavigationLink
        {
            Cluster = Cluster,
            Id = $"{Cluster.Runtime.Name}-load-yaml",
            Name = Assets.Resources.NavigationView_LoadYaml!,
            ViewModelKey = NavigationTargets.LoadYaml,
            Order = -470,
            FluentIcon = Icon.ArrowUpload,
        });
        NavigationItems.Add(new NavigationLink
        {
            Cluster = Cluster,
            Id = $"{Cluster.Runtime.Name}-load-folder",
            Name = Assets.Resources.NavigationView_LoadFolder!,
            ViewModelKey = NavigationTargets.LoadFolder,
            Order = -460,
            FluentIcon = Icon.FolderAdd,
        });
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
}

public partial class ResourceNavigationLink : NavigationLink
{
    [ObservableProperty]
    public partial GroupApiVersionKind? ResourceKind { get; set; }

    [ObservableProperty]
    public partial IObservable<int>? Count { get; set; }

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
