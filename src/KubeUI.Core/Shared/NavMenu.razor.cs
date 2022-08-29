using Microsoft.AspNetCore.Components.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

namespace KubeUI.Core.Shared;

public partial class NavMenu : IDisposable
{
    [Inject]
    private ILogger<NavMenu> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ClusterManager.OnChange += ClusterManager_OnChange;
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        StateHasChanged();
    }

    public static string GetIcon(string iconPath)
    {
        return $"<image href=\"_content/KubeUI.Core/svg/{iconPath}\" height=\"24px\" width=\"24px\" />";
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles(500))
        {
            try
            {
                using var stream = new MemoryStream();
                await file.OpenReadStream(104857600).CopyToAsync(stream);
                stream.Position = 0;

                await ClusterManager.GetActiveCluster().ImportYaml(stream);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error parsing file: {file}", file.Name);
            }
        }
    }

    private async Task LoadFolder()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        var result = dialog.ShowDialog();

        if (result == CommonFileDialogResult.Ok)
        {
            await ClusterManager.GetActiveCluster().ImportFolder(dialog.FileName);
        }
    }

    private void SetActiveCluster(ICluster cluster)
    {
        if (ClusterManager.GetActiveCluster() != null)
        {
            ClusterManager.GetActiveCluster().PropertyChanged -= Cluster_PropertyChanged;
        }

        ClusterManager.SetActiveCluster(cluster);

        cluster.PropertyChanged += Cluster_PropertyChanged;

        NavigationManager.NavigateTo("/Connect");
    }

    private void Cluster_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        ClusterManager.OnChange -= ClusterManager_OnChange;

        if (ClusterManager.GetActiveCluster() != null)
        {
            ClusterManager.GetActiveCluster().PropertyChanged -= Cluster_PropertyChanged;
        }
    }
}