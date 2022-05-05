using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using KubeUI.Core.Components;
using KubeUI.Core.Client;
using Microsoft.Extensions.Logging;

namespace KubeUI.Core.Pages
{
    public partial class Cluster
    {
        [Inject]
        private ILogger<Cluster> Logger { get; set; }

        [Inject]
        private IDialogService Dialog { get; set; }

        [Inject]
        private ClusterManager ClusterManager { get; set; }

        private async Task Delete(ICluster cluster)
        {
            var parameters = new DialogParameters()
            {
                {"ContentText", $"Do you want to delete {cluster.Name}?"},
                {"ButtonText", "Delete"},
                {"Color", Color.Error}
            };

            var dialog = Dialog.Show<Dialog>("Delete", parameters, new DialogOptions() { CloseButton = true });

            if (!(await dialog.Result).Cancelled)
            {
                ClusterManager.RemoveCluster(cluster);
                StateHasChanged();
            }
        }

        private void ImportConfig(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles())
            {
                try
                {
                    ClusterManager.LoadFromConfig(file.OpenReadStream());
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error importing Kube Config");
                }
            }
        }
    }
}