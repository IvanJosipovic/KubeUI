using KubeUI.Core;
using KubeUI.Core.Client;
using static MudBlazor.CategoryTypes;

namespace KubeUI.UI.Components
{
    public partial class Edit<TItem> where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        [Parameter] public TItem Object { get; set; }

        [Parameter] public EventCallback<TItem> ObjectChanged { get; set; }

        [Inject] ClusterManager ClusterManager { get; set; }

        [Inject]
        private IDialogService Dialog { get; set; }

        private TItem ObjectClone { get; set; }

        protected override void OnInitialized()
        {
            if (Object != null)
            {
                ObjectClone = Utilities.CloneObject(Object);
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
            var parameters = new DialogParameters()
            {
                { "ContentText", $"Do you want to save?" },
                { "ButtonText", "Save" }, { "Color", Color.Success }
            };
            var dialog = Dialog.Show<Dialog>("Save", parameters, new DialogOptions()
            {
                CloseButton = true
            });

            if (!(await dialog.Result).Cancelled)
            {
                await ClusterManager.GetActiveCluster().AddOrUpdate(ObjectClone);
            }
        }

        private void EditorValueChanged(string yaml)
        {
            try
            {
                ObjectClone = Core.Client.Seralization.KubernetesYaml.Deserialize<TItem>(yaml);
            }
            catch (Exception ex)
            {
                //throw;
            }
        }
    }
}