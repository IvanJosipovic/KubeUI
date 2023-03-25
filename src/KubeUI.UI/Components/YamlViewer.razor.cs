using KubeUI.Core;
using KubeUI.Core.Client;

namespace KubeUI.UI.Components
{
    public partial class YamlViewer<TItem> where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        [Parameter]
        public TItem Object { get; set; }

        [Parameter]
        public bool HideNosiyFields { get; set; } = true;

        [Inject]
        ClusterManager ClusterManager { get; set; }

        [Inject]
        private ILogger<Edit<TItem>> Logger { get; set; }

        private TItem ObjectClone { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update(bool refresh = false)
        {
            ObjectClone = Utilities.CloneObject(Object);

            if (HideNosiyFields)
            {
                ObjectClone.Metadata.ManagedFields = null;
            }

            if (refresh)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}