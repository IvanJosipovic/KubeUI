using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using Swordfish.NET.Collections;

namespace KubeUI.ViewModels;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class DemoResourceListViewModel : ResourceListViewModel<V1Pod> { public DemoResourceListViewModel() { } }

public partial class ResourceListViewModel<T> : ViewModelBase where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private Client.Cluster _cluster;

    [ObservableProperty]
    private GroupApiVersionKind _kind;

    [ObservableProperty]
    private ConcurrentObservableDictionary<NamespacedName, T> _objects;

    public string TypeName => Kind.ToString();

    [ObservableProperty]
    private object _selectedItem;

    [ObservableProperty]
    private object _selectedNamespaces;

    [ObservableProperty]
    private DataGridCollectionView _dataGridObjects;

    [ObservableProperty]
    private ConcurrentObservableDictionary<NamespacedName, V1Namespace> _namespaces;

    [ObservableProperty]
    private string _searchQuery;

    public ResourceListViewModel()
    {
        Factory = Application.Current.GetRequiredService<IFactory>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();
    }

    public void Initialize()
    {
        DataGridObjects = new DataGridCollectionView(Objects);
        Namespaces = Cluster.GetObjectDictionary<V1Namespace>();
    }

    private void SetFilter()
    {
        if (SelectedNamespaces != null)
        {
            if (SelectedNamespaces is ICollection)
            {
                throw new NotImplementedException();
            }
            else if(SelectedNamespaces is V1Namespace @namespace)
            {
                DataGridObjects.Filter = item => ((KeyValuePair<NamespacedName, T>)item).Key.Namespace == @namespace.Metadata.Name;
            }
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedNamespaces))
        {
            SetFilter();
        }

        if (e.PropertyName == nameof(SearchQuery))
        {
            //SetFilter();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Content = $"This will delete {items.Count} items.\n\nAre you sure?",
            Title = "Warning",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Secondary
        };
        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, T>>().ToList())
            {
                await Cluster.Delete<T>(item.Value);
            }
        }
    }

    private bool CanDelete(IList items)
    {
        return items.Count != 0;
        //var coll = items as IEnumerable;

        //foreach (var item in coll)
        //{
        //    var i = item as T;
        //    var cani = Cluster.CanDelete<T>(i).Result;

        //    if (!cani)
        //    {
        //        return false;
        //    }
        //}
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    private void View(IList items)
    {
        var root = Factory.GetDockable<IRootDock>("Root");
        var pinnedDoc = root.RightPinnedDockables?.FirstOrDefault(x => x.Id == "Properties");
        if (pinnedDoc != null)
        {
            Factory.PinDockable(pinnedDoc);
        }

        var doc = Factory.GetDockable<IDock>("RightDock");

        var existingDock = doc.VisibleDockables.FirstOrDefault(x => x.Id == "Properties");

        if (existingDock != null)
        {
            Factory.RemoveDockable(existingDock, true);
        }

        var instance = Application.Current.GetRequiredService<ResourcePropertiesViewModel<T>>();
        instance.Title = "Properties";
        instance.Id = "Properties";
        instance.Cluster = Cluster;
        instance.Object = ((KeyValuePair<NamespacedName, T>)(items[0])).Value;
        instance.Kind = Kind;
        instance.CanFloat = false;

        Factory?.InsertDockable(doc, instance, 0);
        Factory?.SetActiveDockable(instance);
        Factory?.SetFocusedDockable(doc, instance);
    }

    private bool CanView(IList items)
    {
        return items.Count == 1;
    }

    [RelayCommand(CanExecute = nameof(CanViewYaml))]
    private void ViewYaml(IList items)
    {
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        vm.Title = "Yaml";
        vm.Id = "Yaml";
        vm.Cluster = Cluster;
        vm.Object = ((KeyValuePair<NamespacedName, T>)SelectedItem).Value;

        (Factory as DockFactory).AddToDocumentBottom(vm);
    }

    private bool CanViewYaml(IList items)
    {
        return items.Count == 1;
    }

    [RelayCommand(CanExecute = nameof(CanViewLogs))]
    private async Task ViewLogs(object containerName)
    {
        var vm = Application.Current.GetRequiredService<PodLogsViewModel>();
        vm.Cluster = Cluster;
        vm.Object = ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Value;
        vm.ContainerName = containerName.ToString();
        vm.Title = "Pod Logs";
        vm.Id = "Pod Logs";

        await vm.Connect();

        (Factory as DockFactory).AddToDocumentBottom(vm);
    }

    private bool CanViewLogs(object containerName)
    {
        return !string.IsNullOrEmpty(containerName as string);
    }

    [RelayCommand(CanExecute = nameof(CanViewConsole))]
    private async Task ViewConsole(object containerName)
    {
        var vm = Application.Current.GetRequiredService<PodConsoleViewModel>();
        vm.Cluster = Cluster;
        vm.Object = ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Value;
        vm.ContainerName = containerName.ToString();
        vm.Title = "Pod Console";
        vm.Id = "Pod Console";

        await vm.Connect();

        (Factory as DockFactory).AddToDocumentBottom(vm);
    }

    private bool CanViewConsole(object containerName)
    {
        return !string.IsNullOrEmpty(containerName as string);
    }
}
