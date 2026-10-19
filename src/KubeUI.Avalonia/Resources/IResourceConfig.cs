using k8s.Models;
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
        bool SeedOnConnect => false;
        GroupApiVersionKind Kind { get; }

        /// <summary>Gets where this kind's model comes from.</summary>
        ResourceOrigin Origin { get; }

        /// <summary>Gets the defining CRD for a discovered kind, or null for a built-in kind.</summary>
        V1CustomResourceDefinition? CustomResourceDefinition => null;

        /// <summary>Gets the canonical location of this kind in cluster navigation.</summary>
        NavigationPlacement Navigation { get; }
        IList<IResourceListColumn> Columns();
        IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems);
        IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems);
        int Order { get; }
        string Name { get; }
        Style[] ListStyle();
        IEnumerable<(Verb verb, string? subresource)> Permissions();
        IEnumerable<AuthorizationRequest> ListWatchAuthorizationRequests()
        {
            return [
                new AuthorizationRequest(Kind, Verb.List, null),
                new AuthorizationRequest(Kind, Verb.Watch, null),
            ];
        }
        IEnumerable<AuthorizationRequest> AuthorizationRequests()
        {
            return Permissions().Select(permission => new AuthorizationRequest(Kind, permission.verb, permission.subresource));
        }
        Task EvaluateListWatchAccessAsync();
        Task SeedResource(bool waitForReady = false);
        Task SeedResource(bool waitForReady, CancellationToken cancellationToken)
            => SeedResource(waitForReady);
        IRelayCommand NewResourceCommand { get; }
        IRelayCommand<IList> ViewCommand { get; }
        IAsyncRelayCommand<IList> DeleteCommand { get; }
    }
}
