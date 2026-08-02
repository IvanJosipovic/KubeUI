using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources
{
    public interface ICustomResourceConfig
    {
        void Generate(V1CustomResourceDefinition crd);
    }

    public interface IResourceConfig : IInitializeCluster
    {
        bool IsNamespaced { get; }
        bool CanListAndWatch { get; }
        bool PermissionsLoaded { get; }
        bool ShowNewResource { get; }
        bool IsCustomResource { get; }
        bool SeedOnConnect => false;
        GroupApiVersionKind Kind { get; }
        IList<IResourceListColumn> Columns();
        IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems);
        IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems);
        int Order { get; }
        string Name { get; }
        string? Category { get; }
        Style[] ListStyle();
        IEnumerable<(Verb verb, string? subresource)> Permissions();
        IEnumerable<AuthorizationRequest> ListWatchAuthorizationRequests()
        {
            return [
                new AuthorizationRequest(Type, Verb.List, null),
                new AuthorizationRequest(Type, Verb.Watch, null),
            ];
        }
        IEnumerable<AuthorizationRequest> AuthorizationRequests()
        {
            return Permissions().Select(permission => new AuthorizationRequest(Type, permission.verb, permission.subresource));
        }
        Task EvaluateListWatchAccessAsync();
        Type Type { get; }
        IRelayCommand NewResourceCommand { get; }
        IRelayCommand<IList> ViewCommand { get; }
        IAsyncRelayCommand<IList> DeleteCommand { get; }
    }
}
