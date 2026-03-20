using Avalonia.Styling;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.ViewModels;

namespace KubeUI.Avalonia.Resources
{
    public interface IResourceConfig : IInitializeCluster
    {
        bool IsNamespaced { get; }
        bool CanListAndWatch { get; }
        bool ShowNewResource { get; }
        bool IsCustomResource { get; }
        GroupApiVersionKind Kind { get; }
        IList<IResourceListColumn> Columns();
        IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems);
        IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems);
        int Order { get; }
        string Name { get; }
        string? Category { get; }
        IStyle ListStyle();
        Task UpdatePermissions();
        Type Type { get; }
        IRelayCommand NewResourceCommand { get; }
        IRelayCommand<IList> ViewCommand { get; }
    }
}

