using System.ComponentModel;
using FluentIcons.Common;
using Swordfish.NET.Collections;

namespace KubeUI.Avalonia.ViewModels;

public static class NavigationTargets
{
    public const string ClusterSettings = "cluster-settings";
    public const string ClusterWorkspace = "cluster-workspace";
    public const string PortForwarders = "port-forwarders";
    public const string Visualization = "visualization";
}

public partial class ClusterNavigationNode : ObservableObject
{
    [ObservableProperty]
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<NavigationItem> NavigationItems { get; set; } = new ObservableSortedCollection<NavigationItem>(new NavigationItemOrderComparer());

    private INotifyPropertyChanged? _clusterPropertyChanged;

    public bool IsExpanded
    {
        get => Cluster.IsExpanded;
        set
        {
            if (Cluster.IsExpanded == value)
            {
                return;
            }

            Cluster.IsExpanded = value;
            OnPropertyChanged(nameof(IsExpanded));
        }
    }

    partial void OnClusterChanged(ClusterWorkspaceViewModel value)
    {
        UnsubscribeCluster();
        SubscribeCluster(value);

        OnPropertyChanged(nameof(IsExpanded));
    }

    private void SubscribeCluster(ClusterWorkspaceViewModel cluster)
    {
        if (cluster is not INotifyPropertyChanged propertyChanged)
        {
            return;
        }

        propertyChanged.PropertyChanged += OnClusterPropertyChanged;
        _clusterPropertyChanged = propertyChanged;
    }

    private void UnsubscribeCluster()
    {
        if (_clusterPropertyChanged is null)
        {
            return;
        }

        _clusterPropertyChanged.PropertyChanged -= OnClusterPropertyChanged;
        _clusterPropertyChanged = null;
    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClusterWorkspaceViewModel.IsExpanded))
        {
            OnPropertyChanged(nameof(IsExpanded));
        }
    }
}

public partial class NavigationItem : ObservableObject
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
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

    [ObservableProperty]
    public partial Type? ControlType { get; set; }

    [ObservableProperty]
    public partial string? ViewModelKey { get; set; }
}

public partial class ResourceNavigationLink : NavigationLink
{
    [ObservableProperty]
    public partial IObservable<int>? Count { get; set; }

    public string IconPath => Utilities.GetKubeAssetPath(ControlType!);
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


