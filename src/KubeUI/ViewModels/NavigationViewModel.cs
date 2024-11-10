using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Dock.Model.Core;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class NavigationViewModel : ViewModelBase
{
    private readonly ILogger<NavigationViewModel> _logger;
    private INotificationManager _notificationManager
    {
        get
        {
            return Application.Current.GetRequiredService<INotificationManager>();
        }
    }

    [ObservableProperty]
    private ClusterManager _clusterManager;

    public NavigationViewModel()
    {
        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();
        Title = Resources.NavigationViewModel_Title;
        Id = nameof(NavigationViewModel);
        _logger = Application.Current.GetRequiredService<ILogger<NavigationViewModel>>();
        //_notificationManager = Application.Current.GetRequiredService<INotificationManager>();
    }

    public void TreeView_SelectionChanged(object? item)
    {
        if (item is Cluster cluster)
        {
            _ = Task.Run(cluster.Connect);
            cluster.IsExpanded = !cluster.IsExpanded;
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            _ = Task.Run(() => SelectResourceNavigationLink(resourceNavLink));
        }
        else if (item is NavigationLink navLink)
        {
            _ = Task.Run(async () => await SelectNavigationLink(navLink));
        }

        if (item is NavigationItem nav)
        {
            nav.IsExpanded = !nav.IsExpanded;
        }
    }

    private void SelectResourceNavigationLink(ResourceNavigationLink link)
    {
        var kind = GroupApiVersionKind.From(link.ControlType);

        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(link.ControlType);

        var vm = Application.Current.GetRequiredService(resourceListType) as IDockable;

        if (vm is IInitializeCluster init)
        {
            init.Initialize(link.Cluster);
        }

        Dispatcher.UIThread.Post(() =>
        {
            link.Objects = link.Cluster.Objects[kind].Items;

            Factory.AddToDocuments(vm);
        });
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
                try
                {
                    var stream = await file.OpenReadAsync();
                    await link.Cluster.ImportYaml(stream);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error loading yaml file", sendNotification: true);
                }
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

            foreach (var folder in folders)
            {
                try
                {
                    await link.Cluster.ImportFolder(folder.TryGetLocalPath());
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error loading yaml from folder", sendNotification: true);
                }
            }
        }
        else
        {
            var vm = Application.Current.GetRequiredService(link.ControlType) as IDockable;

            if (vm is IInitializeCluster init)
            {
                init.Initialize(link.Cluster);
            }

            Dispatcher.UIThread.Post(() => Factory.AddToDocuments(vm));
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
    private ICluster _cluster;

    [ObservableProperty]
    private Type _controlType;
}

public partial class ResourceNavigationLink : NavigationLink
{
    [ObservableProperty]
    private ICollection _objects;

    public string IconPath => Utilities.GetKubeAssetPath(ControlType);
}

public class NavigationItemComparer : IComparer<NavigationItem>
{
    public int Compare(NavigationItem? x, NavigationItem? y)
    {
        return x?.Name.CompareTo(y?.Name) ?? 0;
    }
}
