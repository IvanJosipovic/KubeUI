using k8s;
using k8s.Models;
using KubeUI.Core.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace KubeUI.Core.Shared
{
    public partial class NavMenu
    {
        [Inject]
        private ILogger<NavMenu> Logger { get; set; }

        [Inject]
        private ClusterManager ClusterManager { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        public static string GetIcon(string iconPath)
        {
            return $"<image href=\"_content/KubeUI.Core/svg/{iconPath}\" height=\"24px\" width=\"24px\" />";
        }

        private async Task UploadFiles(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles())
            {
                try
                {
                    var objects = await KubernetesYaml.LoadAllFromStreamAsync(file.OpenReadStream());
                    ClusterManager.GetActiveCluster().AddObjects(objects.Cast<IKubernetesObject<V1ObjectMeta>>());
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error parsing file: {file}", file.Name);
                }
            }
        }

        private void SetActiveCluster(ICluster cluster)
        {
            ClusterManager.SetActiveCluster(cluster);
            NavigationManager.NavigateTo("/Connect");
        }
    }
}