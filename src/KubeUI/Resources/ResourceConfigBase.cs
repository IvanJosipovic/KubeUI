using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs;
using Humanizer;
using k8s.Models;
using k8s;
using KubeUI.Client.Informer;
using KubeUI.Client;
using KubeUI.Controls;
using System.Text.Json;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources;

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

    public ICluster Cluster { get; private set; }

    public virtual string Name => Kind.Kind.Humanize(LetterCasing.Title).Pluralize();

    public virtual string? Category { get; } = null;

    public virtual bool ShowNewResource { get; } = true;

    public virtual bool IsNamespaced { get; private set; }

    public virtual int Order { get; }

    public virtual StyleGroup ListStyle() => [];

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

    public virtual IList<ResourceMenuItem> MenuItems()=> [];

    public virtual IList<(Cluster.Verb verb, string? subResource)> CustomPermissions() => [];

    public virtual Control[] Properties(T resource)=> [];

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
            Display = x => x.Metadata.CreationTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            Width = "80"
        };
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        IsNamespaced = Cluster.IsNamespaced<T>();
    }

    public IList<ResourceMenuItem> DefaultMenuItems() => [
        new()
        {
            Header = "View",
            CommandPath = nameof(ViewCommand),
            CommandParameterPath = Utilities.PathBuilder<ResourceListViewModel<T>>(x => x.SelectedItem.Value),
            IconResource = "ic_fluent_panel_right_filled",
        },
        new()
        {
            Header = "View Yaml",
            CommandPath = nameof(ViewYamlCommand),
            CommandParameterPath = Utilities.PathBuilder<ResourceListViewModel<T>>(x => x.SelectedItem.Value),
            IconResource = "code_regular",
        },
        new()
        {
            Header = "Delete",
            CommandPath = nameof(DeleteCommand),
            CommandParameterPath = "SelectedItems",
            IconResource = "delete_regular",
        }
    ];

    public IList<(Verb verb, string? subResource)> DefaultPermissions() => [
        (Verb.Create, null),
        (Verb.Delete, null),
        (Verb.Get, null),
        (Verb.List, null),
        (Verb.Patch, null),
        (Verb.Update, null),
        (Verb.Watch, null),
    ];

    public static readonly string sRestartControllerPatch = $$"""
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

    public async Task UpdatePermissions()
    {
        foreach (var (verb, subResource) in DefaultPermissions())
        {
            await Cluster.UpdateCanIAnyNamespaceAsync<T>(verb, subResource);
        }

        foreach (var (verb, subResource) in CustomPermissions())
        {
            await Cluster.UpdateCanIAnyNamespaceAsync<T>(verb, subResource);
        }
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
            Name = "temp"
        };

        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        vm.Cluster = Cluster;
        vm.Object = resource;
        vm.Id = $"{nameof(ViewYaml)}-{Cluster.Name}-new";
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

            foreach (var item in items.Cast<KeyValuePair<NamespacedName, T>>().ToList())
            {
                try
                {
                    await Cluster.Delete<T>(item.Value);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, $"JsonException occurred while deleting resource {item.Key.Namespace}/{item.Key.Name}");
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Utilities.HandleException(_logger, _notificationManager, ex, $"Error Deleting {item.Key.Namespace}/{item.Key.Name}", sendNotification: true);
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

        foreach (var item in items)
        {
            if (item is KeyValuePair<NamespacedName, T> resource )
            {
                if (!Cluster.CanI<T>(Verb.Delete, resource.Value.Namespace()))
                {
                    return false;
                }
            }
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    public void View(T item)
    {
        var instance = Application.Current.GetRequiredService<ResourcePropertiesViewModel<T>>();
        instance.Initialize(Cluster, item);
        instance.CanFloat = false;

        _factory.AddToRight(instance);
    }

    public bool CanView(T? item)
    {
        return item != null;
    }

    [RelayCommand(CanExecute = nameof(CanViewYaml))]
    public void ViewYaml(T item)
    {
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

        vm.Initialize(Cluster, item);

        _factory.AddToBottom(vm);
    }

    public bool CanViewYaml(T? item)
    {
        return item != null;
    }

    #endregion
}

public interface IResourceListColumn
{
    string Name { get; set; }

    SortDirection Sort { get; set; }

    Type? CustomControl { get; set; }

    string? Width { get; set; }
}

public class ResourceListColumn<T, T2> : IResourceListColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public required string Name { get; set; }

    public required Func<T, T2> Field { get; set; }

    public Func<T, string>? Display { get; set; }

    public SortDirection Sort { get; set; }

    public Type? CustomControl { get; set; }

    public string? Width { get; set; }
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}

public class ResourceMenuItem
{
    public string? Header { get; set; }

    public IBinding? HeaderBinding { get; set; }

    public string? CommandPath { get; set; }

    public string? CommandParameterPath { get; set; }

    public bool? CommandParameterAddSelectedItem { get; set; }

    public IBinding? CommandParameterBinding { get; set; }

    public string? ItemSourcePath { get; set; }

    public string? IconResource { get; set; }

    public ResourceMenuItem? ItemTemplate { get; set; }

    public IList<ResourceMenuItem> MenuItems { get; set; }
}
