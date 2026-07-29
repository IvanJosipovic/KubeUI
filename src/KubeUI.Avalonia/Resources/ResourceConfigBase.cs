using System.Text.Json;
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
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Docking;
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

    public Type Type { get; } = typeof(T);

    public GroupApiVersionKind Kind { get; } = GroupApiVersionKind.From<T>();

    public ClusterWorkspace Cluster { get; private set; }

    public virtual string Name => Kind.Kind.Humanize(LetterCasing.Title).Pluralize();

    public virtual string? Category { get; }

    public virtual bool ShowNewResource { get; } = true;

    public virtual bool IsNamespaced { get; private set; }

    public virtual bool IsCustomResource => false;

    public virtual bool SeedOnConnect => false;

    public bool CanListAndWatch { get; private set; }

    public bool PermissionsLoaded { get; private set; }

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
        return Permissions().Select(permission => new AuthorizationRequest(Type, permission.verb, permission.subresource));
    }

    public virtual IEnumerable<AuthorizationRequest> ListWatchAuthorizationRequests()
    {
        return [
            new AuthorizationRequest(Type, Verb.List, null),
            new AuthorizationRequest(Type, Verb.Watch, null),
        ];
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

    public async Task EvaluateListWatchAccessAsync()
    {
        PermissionsLoaded = false;
        CanListAndWatch = false;

        try
        {
            CanListAndWatch = HasListAndWatchAccess();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate cached list/watch permissions for {Type}", typeof(T).FullName);
            CanListAndWatch = false;
        }

        PermissionsLoaded = true;
    }

    private bool HasListAndWatchAccess()
    {
        if (Cluster.Runtime.Permissions.CanIAnyNamespace<T>(Verb.List)
            && Cluster.Runtime.Permissions.CanIAnyNamespace<T>(Verb.Watch))
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

            if (Cluster.Runtime.Permissions.CanI<T>(Verb.List, namespaceName)
                && Cluster.Runtime.Permissions.CanI<T>(Verb.Watch, namespaceName))
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
        var resource = Activator.CreateInstance<T>();
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
        return Cluster.Runtime.Permissions.CanIAnyNamespace<T>(Verb.Create);
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
                    await Cluster.Runtime.DeleteResource<T>(item);
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
            if (!Cluster.Runtime.Permissions.CanI<T>(Verb.Delete, item.Key))
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

        string sRestartControllerPatch = $$"""
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
                    using var genClient = KubeUI.Kubernetes.KubernetesClientExtensions.GetGenericClient(Cluster.Runtime.Client, item);

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
            if (!Cluster.Runtime.Permissions.CanI<T>(Verb.Patch, item.Key))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
