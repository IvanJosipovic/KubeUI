using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
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
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources;

public abstract partial class ResourceConfigBase<T> : ObservableObject, IResourceConfig where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected IServiceProvider ServiceProvider { get; }
    protected readonly ILogger<ResourceConfigBase<T>> _logger;
    protected readonly IDialogService _dialogService;
    protected readonly INotificationManager _notificationManager;
    protected readonly IFactory _factory;

    protected ResourceConfigBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<ResourceConfigBase<T>>>();
        _dialogService = serviceProvider.GetRequiredService<IDialogService>();
        _factory = serviceProvider.GetRequiredService<IFactory>();
        _notificationManager = serviceProvider.GetRequiredService<INotificationManager>();
    }

    public virtual GroupApiVersionKind Kind => GroupApiVersionKind.From<T>();

    public ClusterWorkspace Cluster { get; private set; }

    public virtual string Name => Kind.Kind.Humanize(LetterCasing.Title).Pluralize();

    public virtual string? Category { get; }

    public virtual bool ShowNewResource { get; } = true;

    public virtual bool IsNamespaced { get; private set; }

    public virtual bool IsCustomResource => false;

    public virtual bool SeedOnConnect => false;

    public bool CanListAndWatch { get; protected set; }

    public bool PermissionsLoaded { get; protected set; }

    public Task SeedResource(bool waitForReady = false)
    {
        return IsCustomResource
            ? Cluster.Runtime.SeedResource(Kind, waitForReady)
            : Cluster.Runtime.SeedResource<T>(waitForReady);
    }

    public virtual int Order { get; }

    public virtual Style[] ListStyle() => [];

    public virtual IList<IResourceListColumn> Columns()
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
        return CreateCustomMenuItems(selectedItems?.OfType<T>());
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
        return new MenuItemViewModel
        {
            Title = Assets.Resources.V1PodConfig_MenuItem_ViewLogs,
            FluentIcon = Icon.TextDescription,
            Command = new AsyncRelayCommand<T?>(ViewPodLogsAsync, CanViewPodLogs),
            CommandParameter = selectedList?.Count == 1 ? selectedList[0] : null,
        };
    }

    private async Task ViewPodLogsAsync(T? resource)
    {
        if (resource is null)
        {
            return;
        }

        var viewModel = ServiceProvider.GetRequiredService<PodLogsViewModel>();
        viewModel.Cluster = Cluster.Runtime;
        viewModel.Object = resource;
        viewModel.ContainerName = string.Empty;
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>([
            new PodLogContainerSelectionItem(string.Empty, Assets.Resources.PodLogsView_AllContainers, false, true),
        ]);
        viewModel.Id = $"{nameof(PodLogsViewModel)}-{Cluster.Runtime.Name}-{Kind.Kind}-{resource.Namespace()}-{resource.Name()}-all";

        if (_factory.AddToBottom(viewModel))
        {
            try
            {
                await viewModel.Connect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing logs for {Kind} {Namespace}/{Name}", Kind.Kind, resource.Namespace(), resource.Name());
            }
        }
    }

    private bool CanViewPodLogs(T? resource)
    {
        return resource is not null
            && !string.IsNullOrWhiteSpace(resource.Name())
            && Cluster.Runtime.Permissions.CanI<V1Pod>(Verb.Get, resource.Namespace(), "log");
    }

    public virtual Control[] Properties(T resource) => [];

    protected ResourceListColumn<T, string> NameColumn(SortDirection sort = SortDirection.None)
    {
        return new ResourceListColumn<T, string>()
        {
            Key = "name",
            Name = Assets.Resources.ResourceListView_Name!,
            Field = x => x.Metadata.Name,
            Width = "2*",
            Sort = sort,
        };
    }

    protected ResourceListColumn<T, string> NamespaceColumn()
    {
        return new ResourceListColumn<T, string>()
        {
            Key = "namespace",
            Name = Assets.Resources.ResourceListView_Namespace!,
            Field = x => x.Metadata.NamespaceProperty,
            Width = "*",
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
            Width = "80"
        };
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;

        if (!IsCustomResource)
        {
            Cluster.Runtime.ModelCatalog.RegisterResource(
                Kind,
                typeof(T),
                waitForReady => Cluster.Runtime.SeedResource<T>(waitForReady));
        }
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
            Title = Assets.Resources.ResourceConfigBase_MenuItem_Delete,
            Command = DeleteCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.Delete,
        }
    ];

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

    private bool HasListAndWatchAccess()
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
        var resource = new T();
        resource.Kind = Kind.Kind;
        resource.ApiVersion = Kind.GroupApiVersion;
        resource.Metadata = new()
        {
            Name = "temp",
        };

        if (IsNamespaced)
        {
            resource.Metadata.NamespaceProperty = "default";
        }

        var vm = ServiceProvider.GetRequiredService<ResourceYamlViewModel>();
        vm.Initialize(Cluster, resource);
        vm.EditMode = true;

        _factory.AddToBottom(vm);
    }

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
        var instance = ServiceProvider.GetRequiredService<ResourcePropertiesViewModel<T>>();
        instance.Initialize(Cluster, (T)items[0]!);
        instance.CanFloat = false;

        _factory.AddToRight(instance);
    }

    public bool CanView(IList? items)
    {
        return items?.Count == 1;
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
