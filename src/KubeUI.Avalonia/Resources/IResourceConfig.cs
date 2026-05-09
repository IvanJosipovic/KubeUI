using Avalonia.Styling;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources
{
    public interface IResourceConfig : IInitializeCluster
    {
        bool IsNamespaced { get; }
        bool CanListAndWatch { get; }
        bool PermissionsLoaded { get; }
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
        IEnumerable<(Verb verb, string? subresource)> Permissions();
        IEnumerable<AuthorizationRequest> AuthorizationRequests()
        {
            return Permissions().Select(permission => new AuthorizationRequest(Type, permission.verb, permission.subresource));
        }
        Task UpdatePermissions();
        Type Type { get; }
        IRelayCommand NewResourceCommand { get; }
        IRelayCommand<IList> ViewCommand { get; }
    }
}
