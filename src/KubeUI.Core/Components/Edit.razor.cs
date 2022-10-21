using KubeUI.Core.Client;

namespace KubeUI.Core.Components
{
    public partial class Edit<TItem> where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        [Parameter] public TItem Object { get; set; }

        [Parameter] public EventCallback<TItem> ObjectChanged { get; set; }

        [Inject] ClusterManager ClusterManager { get; set; }

        private TItem ObjectClone { get; set; }

        protected override void OnInitialized()
        {
            if (Object != null)
            {
                ObjectClone = Object;
            }
            else
            {
                ObjectClone = new TItem();

                var attribute = GroupApiVersionKind.From<TItem>();

                ObjectClone.ApiVersion = attribute.ApiVersion;
                ObjectClone.Kind = attribute.Kind;
            }
        }

        public async Task Save()
        {
            await ClusterManager.GetActiveCluster().AddOrUpdate<TItem>(ObjectClone);
        }
    }
}