using System.Text.Json;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Humanizer;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.Editor;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources;

public abstract partial class ResourceConfigBase<T> : ObservableObject, IResourceConfig
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ILogger<ResourceConfigBase<T>> _logger = null!;
    protected IDialogService _dialogService = null!;
    protected INotificationManager _notificationManager = null!;
    protected IFactory _factory = null!;
    private IPodLogsLauncher _podLogsLauncher = null!;
    private ResourcePropertiesCoordinator _propertiesCoordinator = null!;

    protected ResourceConfigBase()
    {
    }

    protected ResourceConfigBase(IServiceProvider serviceProvider)
    {
        InitializeServices(serviceProvider);
    }

    private void InitializeServices(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        // Services are resolved once per service provider. A configuration that is created
        // without a service provider receives one when it is initialized against a cluster.
        // If initialized with a different service provider (e.g., different cluster), re-initialize.
        if (ServiceProvider == serviceProvider)
        {
            return;
        }

        _logger = serviceProvider.GetRequiredService<ILogger<ResourceConfigBase<T>>>();
        _dialogService = serviceProvider.GetRequiredService<IDialogService>();
        _factory = serviceProvider.GetRequiredService<IFactory>();
        _propertiesCoordinator = new ResourcePropertiesCoordinator(_factory, serviceProvider);
        _podLogsLauncher = serviceProvider.GetRequiredService<IPodLogsLauncher>();
        _notificationManager = serviceProvider.GetRequiredService<INotificationManager>();
        ServiceProvider = serviceProvider;
    }

    public virtual GroupApiVersionKind Kind => GroupApiVersionKind.From<T>();

    /// <inheritdoc />
    public virtual ResourceOrigin Origin => ResourceOrigin.BuiltIn;

    /// <inheritdoc />
    public virtual V1CustomResourceDefinition? CustomResourceDefinition => null;

    public ClusterWorkspace Cluster { get; private set; }

    public virtual string Name => Kind.Kind.Humanize(LetterCasing.Title).Pluralize();

    /// <inheritdoc />
    public virtual NavigationPlacement Navigation => NavigationPlacement.Root;

    public virtual bool ShowNewResource { get; } = true;

    public virtual bool IsNamespaced { get; private set; }

    public virtual bool SeedOnConnect => false;

    public bool CanListAndWatch { get; protected set; }

    public bool PermissionsLoaded { get; protected set; }

    public virtual Task SeedResource(bool waitForReady = false)
        => SeedResource(waitForReady, CancellationToken.None);

    /// <summary>Seeds this resource kind using its typed or dynamic resource implementation.</summary>
    /// <param name="waitForReady">Waits until the informer reports that it is ready.</param>
    /// <param name="cancellationToken">Cancels the seed request.</param>
    public abstract Task SeedResource(bool waitForReady, CancellationToken cancellationToken);

    public virtual int Order { get; }

    public virtual Style[] ListStyle() => [];

    public IList<IResourceListColumn> Columns()
    {
        return CreateDefaultColumns();
    }

    protected virtual IList<IResourceListColumn> CreateDefaultColumns()
    {
        if (IsNamespaced)
        {
            return
            [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                AgeColumn()
            ];
        }
        else
        {
            return
            [
                NameColumn(SortDirection.Ascending),
                AgeColumn()
            ];
        }
    }

    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems)
    {
        var typedItems = selectedItems?.OfType<T>();
        return CreateCustomMenuItems(typedItems);
    }

    protected virtual IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<T>? selectedItems) => [];

    public virtual IList<(Verb verb, string? subResource)> CustomPermissions() => [];

    public IEnumerable<(Verb verb, string? subresource)> Permissions()
    {
        return DefaultPermissions()
            .Concat(CustomPermissions())
            .Distinct();
    }

    public virtual IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return Permissions().Select(permission => new AuthorizationRequest(Kind, permission.verb, permission.subresource));
    }

    public virtual IEnumerable<AuthorizationRequest> ListWatchAuthorizationRequests()
    {
        return [
            new AuthorizationRequest(Kind, Verb.List, null),
            new AuthorizationRequest(Kind, Verb.Watch, null),
        ];
    }

    protected MenuItemViewModel CreatePodLogsMenuItem(IEnumerable<T>? selectedItems)
    {
        var selectedList = selectedItems?.ToList();
        AsyncRelayCommand<IReadOnlyList<T>?> openCommand = new(ViewPodLogsAsync, CanViewPodLogs);
        AvaloniaList<MenuItemViewModel> actions =
        [
            new()
            {
                Title = Assets.Resources.Shared_OpenNewLogsView,
                FluentIcon = Icon.Open,
                Command = openCommand,
                CommandParameter = selectedList,
            },
        ];
        if (Cluster is not null && _podLogsLauncher.CanAddToActive(Cluster))
        {
            actions.Add(new MenuItemViewModel
            {
                Title = Assets.Resources.Shared_AddToCurrentLogsView,
                FluentIcon = Icon.Add,
                Command = new AsyncRelayCommand<IReadOnlyList<T>?>(AddToPodLogsAsync, CanViewPodLogs),
                CommandParameter = selectedList,
            });
        }

        return new MenuItemViewModel
        {
            Title = Assets.Resources.Shared_ViewLogs,
            FluentIcon = Icon.TextDescription,
            Items = actions,
        };
    }

    private async Task ViewPodLogsAsync(IReadOnlyList<T>? resources)
    {
        if (Cluster is null || resources is not { Count: > 0 } || resources.Count > PodLogsViewModel.MaxScopeCount)
        {
            return;
        }

        await _podLogsLauncher.LaunchAsync(Cluster, resources, Kind.Kind);
    }

    private async Task AddToPodLogsAsync(IReadOnlyList<T>? resources)
    {
        if (Cluster is null || resources is not { Count: > 0 } || resources.Count > PodLogsViewModel.MaxScopeCount)
        {
            return;
        }

        await _podLogsLauncher.AddToActiveAsync(Cluster, resources, Kind.Kind);
    }

    private bool CanViewPodLogs(IReadOnlyList<T>? resources)
    {
        if (Cluster is null || resources is not { Count: > 0 })
        {
            return false;
        }

        for (var i = 0; i < resources.Count; i++)
        {
            var resource = resources[i];
            if (!string.IsNullOrWhiteSpace(resource.Name())
                && Cluster.Runtime.Permissions.CanI<V1Pod>(Verb.Get, resource.Namespace(), "log"))
            {
                return true;
            }
        }

        return false;
    }

    public virtual Control[] Properties(T resource)
        => [];

    protected ResourceListColumn<T, string> NameColumn(SortDirection sort = SortDirection.None)
    {
        return new ResourceListColumn<T, string>()
        {
            Key = "name",
            Name = Assets.Resources.ResourceListView_Name!,
            Field = x => x?.Metadata?.Name ?? string.Empty,
            Width = 2,
            WidthMode = DynamicTableViewWidthMode.Star,
            Sort = sort,
        };
    }

    protected ResourceListColumn<T, string> NamespaceColumn()
    {
        return new ResourceListColumn<T, string>()
        {
            Key = "namespace",
            Name = Assets.Resources.ResourceListView_Namespace!,
            Field = x => x?.Metadata?.NamespaceProperty ?? string.Empty,
            Width = 1,
            WidthMode = DynamicTableViewWidthMode.Star,
        };
    }

    protected ResourceListColumn<T, DateTime?> AgeColumn()
    {
        return new ResourceListColumn<T, DateTime?>()
        {
            Key = "age",
            Name = Assets.Resources.ResourceListView_Age!,
            CustomControl = typeof(AgeCell),
            Field = x => x.Metadata.CreationTimestamp,
            Width = 80,
            WidthMode = DynamicTableViewWidthMode.Pixel
        };
    }

    public virtual void Initialize(ClusterWorkspace cluster)
    {
        if (ServiceProvider is null)
        {
            InitializeServices(cluster.ServiceProvider);
        }

        Cluster = cluster;
    }

    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems)
    {
        return CreateDefaultMenuItems(selectedItems?.OfType<T>());
    }

    protected virtual IEnumerable<MenuItemViewModel> CreateDefaultMenuItems(IEnumerable<T>? selectedItems) => [
        new()
        {
            Title = Assets.Resources.ResourceConfigBase_MenuItem_View,
            Command = ViewCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.PanelRight,
            ShowInPropertiesView = false,
        },
        new()
        {
            Title = Assets.Resources.ResourceConfigBase_MenuItem_ViewYaml,
            Command = ViewYamlCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.Code,
        },
        new()
        {
            Title = Assets.Resources.ResourceConfigBase_MenuItem_Visualize,
            Command = VisualizeCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.DataUsage,
        },
        new()
        {
            Title = Assets.Resources.ResourceConfigBase_MenuItem_Edit,
            Command = EditResourceCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.Edit,
        },
        new()
        {
            Title = Assets.Resources.ResourceConfigBase_MenuItem_Delete,
            Command = DeleteCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.Delete,
        }
    ];

    [RelayCommand(CanExecute = nameof(CanEditResource))]
    private async Task EditResource(IList items)
    {
        if (items.Count != 1 || items[0] is not T resource)
            return;

        await ServiceProvider.GetRequiredService<IResourceEditorLauncher>().Open(Cluster, resource);
    }

    private bool CanEditResource(IList? items)
    {
        if (items is null || items.Count != 1 || items[0] is not T resource)
            return false;

        return Cluster.Runtime.Permissions.CanI(Kind, Verb.Update, resource.Namespace());
    }

    public IList<(Verb verb, string? subResource)> DefaultPermissions() => [
        (Verb.Create, null),
        (Verb.Delete, null),
        (Verb.List, null),
        (Verb.Patch, null),
        (Verb.Update, null),
        (Verb.Watch, null),
    ];

    public virtual Task EvaluateListWatchAccessAsync()
    {
        PermissionsLoaded = false;
        CanListAndWatch = false;

        try
        {
            CanListAndWatch = HasListAndWatchAccess();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate cached list/watch permissions for {Kind}", Kind);
            CanListAndWatch = false;
        }

        PermissionsLoaded = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines whether this kind can be listed and watched, either cluster-wide or in at
    /// least one namespace. Subclasses that resolve access differently must not narrow this.
    /// </summary>
    protected bool HasListAndWatchAccess()
    {
        if (Cluster.Runtime.Permissions.CanIAnyNamespace(Kind, IsNamespaced, Verb.List)
            && Cluster.Runtime.Permissions.CanIAnyNamespace(Kind, IsNamespaced, Verb.Watch))
        {
            return true;
        }

        if (!IsNamespaced)
        {
            return false;
        }

        foreach (var @namespace in Cluster.Runtime.Namespaces)
        {
            var namespaceName = @namespace.Name();
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                continue;
            }

            if (Cluster.Runtime.Permissions.CanI(Kind, Verb.List, namespaceName)
                && Cluster.Runtime.Permissions.CanI(Kind, Verb.Watch, namespaceName))
            {
                return true;
            }
        }

        return false;
    }

    #region Actions

    [RelayCommand(CanExecute = nameof(CanNewResource))]
    public void NewResource()
    {
        var resource = CreateNewResource();

        var vm = ServiceProvider.GetRequiredService<ResourceYamlViewModel>();
        vm.Initialize(Cluster, resource);
        vm.EditMode = true;

        _factory.AddToDocuments(vm);
    }

    [RelayCommand(CanExecute = nameof(CanNewResource))]
    public async Task NewResourceUiEditor()
    {
        await ServiceProvider.GetRequiredService<IResourceEditorLauncher>().OpenForCreate(Cluster, CreateNewResource());
    }

    IRelayCommand IResourceConfig.NewResourceUiEditorCommand => NewResourceUiEditorCommand;

    internal virtual T CreateNewResource()
    {
        var resource = CreateInitialResourceTemplate();
        resource.Kind = Kind.Kind;
        resource.ApiVersion = Kind.GroupApiVersion;
        resource.Metadata ??= new();
        if (string.IsNullOrWhiteSpace(resource.Metadata.Name))
        {
            resource.Metadata.Name = "temp";
        }

        if (IsNamespaced && string.IsNullOrWhiteSpace(resource.Metadata.NamespaceProperty))
        {
            resource.Metadata.NamespaceProperty = "default";
        }

        return resource;
    }

    /// <summary>Creates a fresh, kind-specific object used to start a new resource document.</summary>
    /// <remarks>Override to return a fully typed template. Called for each new resource.</remarks>
    protected virtual T CreateInitialResourceTemplate() => new();

    public bool CanNewResource()
    {
        return Cluster.Runtime.Permissions.CanIAnyNamespace(Kind, IsNamespaced, Verb.Create);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public virtual async Task Delete(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListView_Delete_Title,
            Content = string.Format(Assets.Resources.ResourceListView_Delete_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListView_Delete_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListView_Delete_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == FAContentDialogResult.Primary)
        {
            var exceptions = new List<Exception>();

            foreach (var item in items.Cast<T>().ToList())
            {
                try
                {
                    await Cluster.Runtime.DeleteResource(item);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, $"JsonException occurred while deleting resource {item.Namespace()}/{item.Name()}");
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Utilities.HandleException(_logger, _notificationManager, ex, $"Error Deleting {item.Namespace()}/{item.Name()}", sendNotification: true);
                }
            }

            if (exceptions.Count > 0)
            {
                _logger.LogError(new AggregateException(exceptions), "Error Deleting Resources");
            }
        }
    }

    public virtual bool CanDelete(IList? items)
    {
        if (items == null || items.Count == 0)
        {
            return false;
        }

        foreach (var item in items.Cast<T>().ToList().GroupBy(x => x.Namespace()))
        {
            if (!Cluster.Runtime.Permissions.CanI(Kind, Verb.Delete, item.Key))
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    public void View(IList items)
    {
        _propertiesCoordinator.Open(Cluster, (T)items[0]!);
    }

    public bool CanView(IList? items)
    {
        return items?.Count == 1;
    }

    /// <summary>
    /// Shows a single selected resource in the Properties view when one is already open (docked or auto-hidden); otherwise does nothing.
    /// Keeping the Properties view open opts into following the list selection. It is never opened, expanded or focused here,
    /// and multi-selection leaves it unchanged.
    /// </summary>
    /// <param name="items">The current selection.</param>
    public void ShowInOpenProperties(IList? items)
    {
        if (items is { Count: 1 } && items[0] is T resource)
            _propertiesCoordinator.Follow(Cluster, resource);
    }

    [RelayCommand(CanExecute = nameof(CanViewYaml))]
    public void ViewYaml(IList items)
    {
        var vm = ServiceProvider.GetRequiredService<ResourceYamlViewModel>();

        vm.Initialize(Cluster, (T)items[0]!);

        _factory.AddToBottom(vm);
    }

    public bool CanViewYaml(IList? items)
    {
        return items?.Count == 1;
    }

    [RelayCommand(CanExecute = nameof(CanVisualize))]
    public void Visualize(IList items)
    {
        var selectedItem = items.Cast<T>().Single();
        var vm = ServiceProvider.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(Cluster, selectedItem);
        _factory.AddToDocuments(vm);
    }

    public bool CanVisualize(IList? items) => items?.Count == 1;

    [RelayCommand(CanExecute = nameof(CanRestart))]
    private async Task Restart(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListView_Restart_Title,
            Content = string.Format(Assets.Resources.ResourceListView_Restart_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListView_Restart_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListView_Restart_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var sRestartControllerPatch = $$"""
                {
                    "spec": {
                        "template": {
                            "metadata": {
                                "annotations": {
                                    "kubectl.kubernetes.io/restartedAt": "{{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}}"
                                }
                            }
                        }
                    }
                }
                """;

        if (result == FAContentDialogResult.Primary)
        {
            var exceptions = new List<Exception>();

            foreach (var item in items.Cast<T>().ToList())
            {
                try
                {
                    using var genClient = Cluster.Runtime.Client!.GetGenericClient(Kind);

                    await genClient.PatchNamespacedAsync<T>(new V1Patch(sRestartControllerPatch, V1Patch.PatchType.MergePatch), item.Metadata.NamespaceProperty, item.Metadata.Name);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, $"JsonException occurred while deleting resource {item.Namespace()}/{item.Name()}");
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Utilities.HandleException(_logger, _notificationManager, ex, $"Error Restarting {item.Namespace()}/{item.Name()}", sendNotification: true);
                }
            }

            if (exceptions.Count > 0)
            {
                _logger.LogError(new AggregateException(exceptions), "Error Restarting Resources");
            }
        }
    }

    private bool CanRestart(IList? items)
    {
        if (items == null || items.Count == 0)
        {
            return false;
        }

        foreach (var item in items.Cast<T>().ToList().GroupBy(x => x.Namespace()))
        {
            if (!Cluster.Runtime.Permissions.CanI(Kind, Verb.Patch, item.Key))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
