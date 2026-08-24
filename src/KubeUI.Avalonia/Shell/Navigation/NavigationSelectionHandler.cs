using Avalonia.Platform.Storage;
using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using KubeUI.Avalonia.Features.Clusters.Overview;
using KubeUI.Avalonia.Features.Clusters.Settings;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationSelectionHandler
{
    private readonly ILogger _logger;
    private readonly INotificationManager _notificationManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlatformServices _platformServices;
    private readonly Action<IDockable> _addToDocuments;

    public NavigationSelectionHandler(
        ILogger logger,
        INotificationManager notificationManager,
        IServiceProvider serviceProvider,
        IPlatformServices platformServices,
        Action<IDockable> addToDocuments)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _serviceProvider = serviceProvider;
        _platformServices = platformServices;
        _addToDocuments = addToDocuments;
    }

    public async Task SelectAsync(NavigationLink link)
    {
        if (link.ViewModelKey == NavigationTargets.LoadYaml)
        {
            await ImportYamlAsync(link).ConfigureAwait(false);
            return;
        }

        if (link.ViewModelKey == NavigationTargets.LoadFolder)
        {
            await ImportFolderAsync(link).ConfigureAwait(false);
            return;
        }

        var vmType = link.ViewModelKey switch
        {
            NavigationTargets.ClusterSettings => typeof(ClusterSettingsViewModel),
            NavigationTargets.ClusterWorkspace => typeof(ClusterViewModel),
            NavigationTargets.PortForwarders => typeof(PortForwarderListViewModel),
            NavigationTargets.Visualization => typeof(VisualizationViewModel),
            _ => null
        };

        if (vmType == null)
        {
            _logger.LogError("Unable to resolve navigation target for {Name}", link.Name);
            return;
        }

        var vm = _serviceProvider.GetRequiredService(vmType) as IDockable;
        if (vm == null)
        {
            _logger.LogError("Unable to resolve navigation target dockable for {Name}", link.Name);
            return;
        }

        if (vm is IInitializeCluster init)
        {
            init.Initialize(link.Cluster);
        }

        _addToDocuments(vm);
    }

    private async Task ImportYamlAsync(NavigationLink link)
    {
        var files = await _platformServices.OpenFilePickerAsync(new()
        {
            Title = Assets.Resources.NavigationView_LoadYaml,
            AllowMultiple = true,
            FileTypeFilter = [new("Yaml") { Patterns = ["*.yaml", "*.yml"] }]
        }).ConfigureAwait(false);

        foreach (var file in files)
        {
            try
            {
                await using var stream = await file.OpenReadAsync().ConfigureAwait(false);
                await link.Cluster.Runtime.ImportYaml(stream).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Utilities.HandleException(_logger, _notificationManager, ex, "Error loading yaml file", sendNotification: true);
            }
        }
    }

    private async Task ImportFolderAsync(NavigationLink link)
    {
        var folders = await _platformServices.OpenFolderPickerAsync(new()
        {
            Title = Assets.Resources.NavigationView_LoadFolder,
            AllowMultiple = false
        }).ConfigureAwait(false);

        foreach (var folder in folders)
        {
            try
            {
                var path = folder.TryGetLocalPath();
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                await link.Cluster.Runtime.ImportFolder(path).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Utilities.HandleException(_logger, _notificationManager, ex, "Error loading yaml from folder", sendNotification: true);
            }
        }
    }
}
