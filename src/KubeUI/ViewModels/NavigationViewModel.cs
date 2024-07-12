using Avalonia.Platform.Storage;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class NavigationViewModel : ViewModelBase
{
    [ObservableProperty]
    private ClusterManager _clusterManager;

    public NavigationViewModel()
    {
        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();
        Factory = Application.Current.GetRequiredService<IFactory>();
        Title = Resources.NavigationViewModel_Title;
        Id = nameof(NavigationViewModel);
    }

    public async void TreeView_SelectionChanged(object? item)
    {
        if (item is Cluster cluster)
        {
            _ = Task.Run(cluster.Connect);
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            SelectResourceNavigationLink(resourceNavLink);
        }
        else if (item is NavigationLink navLink)
        {
            await SelectNavigationLink(navLink);
        }
    }

    private void SelectResourceNavigationLink(ResourceNavigationLink link)
    {
        var kind = GroupApiVersionKind.From(link.ControlType);

        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(link.ControlType);

        var vm = Application.Current.GetRequiredService(resourceListType) as IResourceListViewModel;

        vm.Initialize(link.Cluster);

        link.Objects = link.Cluster.Objects[kind].Items;

        Factory.AddToDocuments(vm);
    }

    private async Task SelectNavigationLink(NavigationLink link)
    {
        if (link.Id == "load-yaml")
        {
            // Start async operation to open the dialog.
            var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                Title = Resources.NavigationViewModel_LoadYaml,
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType>() { new("Yaml") { Patterns = ["*.yaml", ".yml"] } }
            });

            foreach (var file in files)
            {
                var stream = await file.OpenReadAsync();
                link.Cluster.ImportYaml(stream);
            }
        }
        else if (link.Id == "load-folder")
        {
            // Start async operation to open the dialog.
            var folders = await App.TopLevel.StorageProvider.OpenFolderPickerAsync(new()
            {
                Title = Resources.NavigationViewModel_LoadFolder,
                AllowMultiple = false
            });

            foreach (var file in folders)
            {
                link.Cluster.ImportFolder(file.TryGetLocalPath());
            }
        }
        else
        {
            var vm = Application.Current.GetRequiredService(link.ControlType) as IDockable;

            vm.Title = link.Name;
            vm.Id = link.Cluster.Name + "-" + link.Name;

            link.ControlType.GetProperty(nameof(ResourceListViewModel<V1Pod>.Cluster))?.SetValue(vm, link.Cluster);

            Factory.AddToDocuments(vm);
        }
    }
}

public partial class NavigationItem : ObservableObject
{
    [ObservableProperty]
    private string _id;

    [ObservableProperty]
    private string? _svgIcon;

    [ObservableProperty]
    private string? _styleIcon;

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

public class NavigationItemComparer : IComparer<NavigationItem>
{
    public int Compare(NavigationItem? x, NavigationItem? y)
    {
        return x?.Name.CompareTo(y?.Name) ?? 0;
    }
}
