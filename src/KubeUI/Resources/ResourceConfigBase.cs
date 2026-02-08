using System.Text.Json;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using Dock.Model.Core;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubernetesClient.Informer.Client;
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

    public virtual IList<ResourceMenuItem> MenuItems() => [];

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

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
    }

    public IList<ResourceMenuItem> DefaultMenuItems() => [
        new()
        {
            Header = "View",
            CommandPath = nameof(ViewCommand),
            CommandParameterPath = "SelectedItems",
            FluentIcon = Icon.PanelRight,
        },
        new()
        {
            Header = "View Yaml",
            CommandPath = nameof(ViewYamlCommand),
            CommandParameterPath = "SelectedItems",
            FluentIcon = Icon.Code,
        },
        new()
        {
            Header = "Delete",
            CommandPath = nameof(DeleteCommand),
            CommandParameterPath = "SelectedItems",
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
        var tasks = new List<Task>();

        foreach (var (verb, subResource) in DefaultPermissions())
        {
            tasks.Add(Cluster.UpdatePermissionsAllNamespaceAsync<T>(verb, subResource));
        }

        foreach (var (verb, subResource) in CustomPermissions())
        {
            tasks.Add(Cluster.UpdatePermissionsAllNamespaceAsync<T>(verb, subResource));
        }

        await Task.WhenAll(tasks);
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

    #endregion
}

public interface IResourceListColumn
{
    string Name { get; }
    string? Width { get; }
    SortDirection Sort { get; set; }
    Type CustomControl { get; }

    Type ItemType { get; }
    Type ValueType { get; }

    Func<object, IComparable?> SortKey { get; }
    Func<object, string> DisplayValue { get; }
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

    public Icon? FluentIcon { get; set; }

    public ResourceMenuItem? ItemTemplate { get; set; }

    public IList<ResourceMenuItem> MenuItems { get; set; }
}
