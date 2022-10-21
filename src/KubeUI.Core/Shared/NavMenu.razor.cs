using Microsoft.AspNetCore.Components.Forms;

namespace KubeUI.Core.Shared;

public partial class NavMenu : IDisposable
{
    [Inject]
    private ILogger<NavMenu> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private System.Timers.Timer Timer { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ClusterManager.OnChange += ClusterManager_OnChange;

        Timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        Timer.Elapsed += Timer_Elapsed;
        Timer.Enabled = true;
        Timer.AutoReset = true;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            InvokeAsync(StateHasChanged);
        }
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

                if (Path.GetExtension(file.Name) == ".zip")
                {
                    await ClusterManager.GetActiveCluster().ImportZip(stream);
                }
                else if (Path.GetExtension(file.Name) == ".yaml" || Path.GetExtension(file.Name) == ".yml")
                {
                    await ClusterManager.GetActiveCluster().ImportYaml(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error parsing file: {file}", file.Name);
            }
        }
    }

    private async Task LoadFolder()
    {
#if Windows
        var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        var result = dialog.ShowDialog();

        if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
        {
            await ClusterManager.GetActiveCluster().ImportFolder(dialog.FileName);
        }
#endif
    }

    private void SetActiveCluster(ICluster cluster)
    {
        ClusterManager.SetActiveCluster(cluster);

        NavigationManager.NavigateTo("/Connect");
    }

    public void Dispose()
    {
        ClusterManager.OnChange -= ClusterManager_OnChange;
        Timer.Dispose();
    }

    private bool ClustersExpanded { get; set; }
    private bool WorkloadsExpanded { get; set; }
    private bool ConfigurationExpanded { get; set; }
    private bool NetworkExpanded { get; set; }
    private bool StorageExpanded { get; set; }
    private bool AccessControlExpanded { get; set; }
    private bool CRDExpanded { get; set; }
}