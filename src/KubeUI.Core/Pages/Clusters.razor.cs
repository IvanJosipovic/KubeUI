using Microsoft.AspNetCore.Components.Forms;

namespace KubeUI.Core.Pages;

public partial class Clusters
{
    [Inject]
    private ILogger<Clusters> Logger { get; set; }

    [Inject]
    private IDialogService Dialog { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    private HashSet<ICluster> SelectedItems;

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
        foreach (var file in e.GetMultipleFiles(500))
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

    private void AddGitOpsCluster()
    {
        ClusterManager.AddGitOpsCluster();
    }

    private void Compare()
    {
        var left = SelectedItems.ElementAt(0);
        var right = SelectedItems.ElementAt(1);
        var parameters = new DialogParameters()
        {
            { "Left", left },
            { "Right", right },
        };

        var dialog = Dialog.Show<CompareCluster>($"Compare {left.Name} and {right.Name}", parameters, new DialogOptions()
        {
            CloseButton = true,
            FullScreen = true
        });
    }
}