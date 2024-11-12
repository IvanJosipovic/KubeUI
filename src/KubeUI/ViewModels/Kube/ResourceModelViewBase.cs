using k8s.Models;
using k8s;

namespace KubeUI.ViewModels.Kube
{
    internal abstract partial class ResourceViewModelBase<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        public bool DefaultMenuItems { get; set; } = true;

        public bool ShowNewResource { get; set; } = true;

        public abstract IList<IResourceListViewDefinitionColumn> Columns { get; }

        public abstract IList<ResourceListViewMenuItem> MenuItems { get; }

        public abstract object? Properties(T resource);
    }
}
