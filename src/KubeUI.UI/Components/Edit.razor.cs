using k8s.Models;
using KubeUI.Core;
using KubeUI.Core.Client;
using Microsoft.AspNetCore.Components;
using static MudBlazor.CategoryTypes;

namespace KubeUI.UI.Components
{
    public partial class Edit<TItem> where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        [Parameter] public TItem Object { get; set; }

        [Parameter] public EventCallback<TItem> ObjectChanged { get; set; }

        [Parameter] public bool ShowTitle { get; set; }

        [Inject] ClusterManager ClusterManager { get; set; }

        [Inject]
        private IDialogService Dialog { get; set; }

        [Inject]
        private ILogger<Edit<TItem>> Logger { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        private TItem ObjectClone { get; set; }

        private GroupApiVersionKind Type { get; set; }

        protected override void OnInitialized()
        {
            Type = GroupApiVersionKind.From<TItem>();
        }

        protected override void OnParametersSet()
        {
            if (Object != null)
            {
                ObjectClone = Utilities.CloneObject(Object);
            }
            else
            {
                ObjectClone = new TItem
                {
                    ApiVersion = Type.GroupApiVersion,
                    Kind = Type.Kind
                };
            }
        }

        public async Task Save()
        {
            var parameters = new DialogParameters()
            {
                { "ContentText", "Do you want to save?" },
                { "ButtonText", "Save" }, { "Color", Color.Success }
            };
            var dialog = Dialog.Show<Dialog>("Save", parameters, new DialogOptions()
            {
                CloseButton = true
            });

            if (!(await dialog.Result).Canceled)
            {
                try
                {
                    await ClusterManager.GetActiveCluster().AddOrUpdate(ObjectClone);
                }
                catch (Exception ex)
                {
                    Snackbar.Add("Failed Save Resource: " + ex.Message, Severity.Error);
                }
            }
        }

        private void EditorValueChanged(string yaml)
        {
            try
            {
                ObjectClone = Core.Client.Serialization.KubernetesYaml.Deserialize<TItem>(yaml);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error converting from yaml to " + typeof(TItem));
            }
        }
    }
}