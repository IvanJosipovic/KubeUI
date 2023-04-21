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

                ObjectClone?.Metadata?.Annotations?.Remove("kubectl.kubernetes.io/last-applied-configuration");
            }

            if (refresh)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}