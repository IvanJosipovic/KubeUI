using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.ViewModels;

public sealed partial class NavigationViewModel : ViewModelBase
{
    [ObservableProperty]
    private ClusterManager _clusterManager;

    public NavigationViewModel()
    {
        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();
        Factory = Application.Current.GetRequiredService<IFactory>();
    }

    [ActivatorUtilitiesConstructor]
    public NavigationViewModel(ClusterManager clusterManager, IFactory factory)
    {
        ClusterManager = clusterManager;
        Factory = factory;
    }

    public void TreeView_SelectionChanged(object? item)
    {
        var doc = Factory.GetDockable<IDocumentDock>("Documents");

        if (item is Cluster cluster)
        {
            Task.Run(cluster.Connect);
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            var kind = GroupApiVersionKind.From(resourceNavLink.ControlType);

            resourceNavLink.Cluster.Seed(resourceNavLink.ControlType);
            resourceNavLink.Objects = resourceNavLink.Cluster.Objects[kind].Items;

            var vm = GetContext(resourceNavLink.ControlType, resourceNavLink.Objects, kind, resourceNavLink.Cluster) as IDockable;
            vm.Title = kind.ToString();
            vm.Id = resourceNavLink.Cluster.Name + "-" + kind.ToString();

            var existingDock = doc.VisibleDockables.FirstOrDefault(x => x.Id == vm.Id);

            if (existingDock == null)
            {
                Factory?.AddDockable(doc, vm);
                Factory?.SetActiveDockable(vm);
                Factory?.SetFocusedDockable(doc, vm);
            }
            else
            {
                Factory?.SetActiveDockable(existingDock);
                Factory?.SetFocusedDockable(doc, existingDock);
            }
        }
        else if (item is NavigationLink navLink)
        {
            var vm = Application.Current.GetRequiredService(navLink.ControlType) as IDockable;

            vm.Title = navLink.Cluster.Name;
            vm.Id = navLink.Cluster.Name + "" + navLink.Name;

            navLink.ControlType.GetProperty("Cluster").SetValue(vm, navLink.Cluster);

            var existingDock = doc.VisibleDockables.FirstOrDefault(x => x.Id == vm.Id);

            if (existingDock == null)
            {
                Factory?.AddDockable(doc, vm);
                Factory?.SetActiveDockable(vm);
                Factory?.SetFocusedDockable(doc, vm);
            }
            else
            {
                Factory?.SetActiveDockable(existingDock);
                Factory?.SetFocusedDockable(doc, existingDock);
            }
        }
    }

    private static object GetContext(Type type, object list, GroupApiVersionKind kind, Cluster cluster)
    {
        var constructedType = typeof(ResourceListViewModel<>).MakeGenericType(type);

        var instance = Application.Current.GetRequiredService(constructedType);

        constructedType.GetProperty(nameof(ResourceListViewModel<V1Pod>.Cluster)).SetValue(instance, cluster);
        constructedType.GetProperty(nameof(ResourceListViewModel<V1Pod>.Objects)).SetValue(instance, list);
        constructedType.GetProperty(nameof(ResourceListViewModel<V1Pod>.Kind)).SetValue(instance, kind);

        constructedType.GetMethod("Initialize").Invoke(instance, null);

        return instance;
    }
}

public partial class NavigationItem : ObservableObject
{
    [ObservableProperty]
    private string _id;

    [ObservableProperty]
    private string _icon;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private ObservableCollection<NavigationItem> _navigationItems = [];

    [ObservableProperty]
    private bool _isExpanded;
}

public partial class NavigationLink : NavigationItem
{
    [ObservableProperty]
    private Cluster _cluster;

    [ObservableProperty]
    private Type _controlType;
}

public partial class ResourceNavigationLink : NavigationLink
{
    [ObservableProperty]
    private ICollection _objects;
}
