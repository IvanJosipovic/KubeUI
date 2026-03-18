using System.Text.Json;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Humanizer;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.ViewModels;

namespace KubeUI.Avalonia.Resources;

public abstract partial class ResourceConfigBase<T> : ObservableObject, IResourceConfig where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected readonly ILogger<ResourceConfigBase<T>> _logger;
    protected readonly IDialogService _dialogService;
    protected readonly INotificationManager _notificationManager;
    protected readonly IFactory _factory;

    public ResourceConfigBase()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceConfigBase<T>>>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();
        _factory = Application.Current.GetRequiredService<IFactory>();
        _notificationManager = Application.Current.GetRequiredService<INotificationManager>();
    }

    public Type Type { get; } = typeof(T);

    public GroupApiVersionKind Kind { get; } = GroupApiVersionKind.From<T>();

    public ClusterWorkspaceViewModel Cluster { get; private set; }

    public virtual string Name => Kind.Kind.Humanize(LetterCasing.Title).Pluralize();

    public virtual string? Category { get; } = null;

    public virtual bool ShowNewResource { get; } = true;

    public virtual bool IsNamespaced { get; private set; }

    public virtual bool IsCustomResource => false;

    public bool CanListAndWatch { get; private set; }

    public virtual int Order { get; }

    public virtual IStyle ListStyle() => null;

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

    public virtual Control[] Properties(T resource) => [];

    protected ResourceListColumn<T, string> NameColumn(SortDirection sort = SortDirection.None)
    {
        return new ResourceListColumn<T, string>()
        {
            Name = "Name",
            Field = x => x.Metadata.Name,
            Width = "2*",
            Sort = sort,
        };
    }

    protected ResourceListColumn<T, string> NamespaceColumn()
    {
        return new ResourceListColumn<T, string>()
        {
            Name = "Namespace",
            Field = x => x.Metadata.NamespaceProperty,
            Width = "*",
        };
    }

    protected ResourceListColumn<T, DateTime?> AgeColumn()
    {
        return new ResourceListColumn<T, DateTime?>()
        {
            Name = "Age",
            CustomControl = typeof(AgeCell),
            Field = x => x.Metadata.CreationTimestamp,
            Width = "80"
        };
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
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
            Header = "View",
            Command = ViewCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.PanelRight,
        },
        new()
        {
            Header = "View Yaml",
            Command = ViewYamlCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.Code,
        },
        new()
        {
            Header = "Delete",
            Command = DeleteCommand,
            CommandParameter = selectedItems?.ToList(),
            FluentIcon = Icon.Delete,
        }
    ];

    public IList<(Verb verb, string? subResource)> DefaultPermissions() => [
        (Verb.Create, null),
        (Verb.Delete, null),
        //(Verb.Get, null),
        (Verb.List, null),
        (Verb.Patch, null),
        (Verb.Update, null),
        (Verb.Watch, null),
    ];

    public async Task UpdatePermissions()
    {
        CanListAndWatch = false;

        await Cluster.UpdatePermissionsAllNamespaceAsync<T>(Verb.List).ConfigureAwait(false);
        await Cluster.UpdatePermissionsAllNamespaceAsync<T>(Verb.Watch).ConfigureAwait(false);

        try
        {
            CanListAndWatch = Cluster.CanIAnyNamespace<T>(Verb.List)
                && Cluster.CanIAnyNamespace<T>(Verb.Watch);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate cached list/watch permissions for {Type}", typeof(T).FullName);
            CanListAndWatch = false;
        }

        if (!CanListAndWatch)
        {
            return;
        }

        var tasks = new List<Task>();

        foreach (var (verb, subResource) in DefaultPermissions())
        {
            if (verb is Verb.List or Verb.Watch)
            {
                continue;
            }

            tasks.Add(Cluster.UpdatePermissionsAllNamespaceAsync<T>(verb, subResource));
        }

        foreach (var (verb, subResource) in CustomPermissions())
        {
            if (verb is Verb.List or Verb.Watch)
            {
                continue;
            }

            tasks.Add(Cluster.UpdatePermissionsAllNamespaceAsync<T>(verb, subResource));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
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

        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        vm.Initialize(Cluster, resource);
        vm.EditMode = true;

        _factory.AddToBottom(vm);
    }

    public bool CanNewResource()
    {
        return Cluster.CanIAnyNamespace<T>(Verb.Create);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public virtual async Task Delete(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListViewModel_Delete_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_Delete_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_Delete_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_Delete_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            var exceptions = new List<Exception>();

            foreach (var item in items.Cast<T>().ToList())
            {
                try
                {
                    await Cluster.DeleteResource<T>(item);
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
            if (!Cluster.CanI<T>(Verb.Delete, item.Key))
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    public void View(IList items)
    {
        var instance = Application.Current.GetRequiredService<ResourcePropertiesViewModel<T>>();
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
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

        vm.Initialize(Cluster, (T)items[0]!);

        _factory.AddToBottom(vm);
    }

    public bool CanViewYaml(IList? items)
    {
        return items?.Count == 1;
    }

    [RelayCommand(CanExecute = nameof(CanRestart))]
    private async Task Restart(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListViewModel_Restart_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
            DefaultButton = ContentDialogButton.Secondary
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

        if (result == ContentDialogResult.Primary)
        {
            var exceptions = new List<Exception>();

            foreach (var item in items.Cast<T>().ToList())
            {
                try
                {
                    using var genClient = Cluster.Client.GetGenericClient(item);

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
            if (!Cluster.CanI<T>(Verb.Patch, item.Key))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}


public class ResourceListColumn<T, TValue> : IResourceListColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public required string Name { get; set; }

    public required Func<T, TValue> Field { get; set; }

    public Func<T, string>? Display { get; set; }

    public SortDirection Sort { get; set; } = SortDirection.None;

    public Type CustomControl { get; set; } = typeof(ResourceTextCell);

    public string? Width { get; set; }

    public Type ItemType => typeof(T);

    public Type ValueType => typeof(TValue);

    public Func<object, IComparable?> SortKey =>
        o => (IComparable?)(object?)Field((T)o);

    public Func<object, string> DisplayValue =>
        o =>
        {
            var t = (T)o;
            if (Display != null)
                return Display(t);
            var v = Field(t);
            return v?.ToString() ?? "";
        };
}



