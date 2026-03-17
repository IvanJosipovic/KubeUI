using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Dock.Model.Controls;
using Dock.Model.Core;
using KubernetesClient.Informer.Client;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class NavigationViewModel : ViewModelBase
{
    private readonly ILogger<NavigationViewModel> _logger;
    private INotificationManager _notificationManager => Application.Current.GetRequiredService<INotificationManager>();

    [ObservableProperty]
    public partial IClusterCatalog ClusterCatalog { get; set; }

    public NavigationViewModel()
    {
        ClusterCatalog = Application.Current.GetRequiredService<IClusterCatalog>();
        Title = Assets.Resources.NavigationViewModel_Title;
        Id = nameof(NavigationViewModel);
        _logger = Application.Current.GetRequiredService<ILogger<NavigationViewModel>>();
        //_notificationManager = Application.Current.GetRequiredService<INotificationManager>();
    }

    public async void TreeView_SelectionChanged(object? item)
    {
        if (item is ICluster cluster)
        {
            _ = Task.Run(cluster.Connect);
            cluster.IsExpanded = !cluster.IsExpanded;
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            SelectResourceNavigationLink(resourceNavLink);
        }
        else if (item is NavigationLink navLink)
        {
            await SelectNavigationLink(navLink);
        }

        if (item is NavigationItem nav && nav.NavigationItems.Count > 0)
        {
            nav.IsExpanded = !nav.IsExpanded;
        }
    }

    private void SelectResourceNavigationLink(ResourceNavigationLink nav)
    {
        var expectedId = $"{nav.Cluster.Name}-{GroupApiVersionKind.From(nav.ControlType)}";

        if (TryActivateExistingDocument(expectedId))
        {
            nav.Count ??= nav.Cluster.GetResourceCount(nav.ControlType);
            return;
        }

        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(nav.ControlType);
        var vm = Application.Current.GetRequiredService(resourceListType) as IDockable;

        if (vm is IInitializeCluster init)
        {
            init.Initialize(nav.Cluster);
        }

        nav.Count ??= nav.Cluster.GetResourceCount(nav.ControlType);

        Factory.AddToDocuments(vm);
    }

    private async Task SelectNavigationLink(NavigationLink link)
    {
        if (link.Id == "load-yaml")
        {
            // Start async operation to open the dialog.
            var files = await TopLevelAccessor.GetRequired().StorageProvider.OpenFilePickerAsync(new()
            {
                Title = Assets.Resources.NavigationViewModel_LoadYaml,
                AllowMultiple = true,
                FileTypeFilter = [new("Yaml") { Patterns = ["*.yaml", ".yml"] }]
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
            var folders = await TopLevelAccessor.GetRequired().StorageProvider.OpenFolderPickerAsync(new()
            {
                Title = Assets.Resources.NavigationViewModel_LoadFolder,
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
            var vmType = link.ViewModelKey switch
            {
                NavigationTargets.ClusterSettings => typeof(ClusterSettingsViewModel),
                NavigationTargets.ClusterWorkspace => typeof(ClusterViewModel),
                NavigationTargets.PortForwarders => typeof(PortForwarderListViewModel),
                NavigationTargets.Visualization => typeof(VisualizationViewModel),
                _ => link.ControlType
            };

            if (vmType == null)
            {
                _logger.LogError("Unable to resolve navigation target for {Name}", link.Name);
                return;
            }

            var vm = Application.Current.GetRequiredService(vmType) as IDockable;

            if (vm is IInitializeCluster init)
            {
                init.Initialize(link.Cluster);
            }

            Factory.AddToDocuments(vm);
        }
    }

    private bool TryActivateExistingDocument(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var existing = Factory.FindDockableById(id);

        if (existing == null)
        {
            return false;
        }
        var documents = Factory.GetDockable<IDocumentDock>("Documents");
        Factory.SetActiveDockable(existing);
        Factory.SetFocusedDockable(documents, existing);
        return true;
    }
}

