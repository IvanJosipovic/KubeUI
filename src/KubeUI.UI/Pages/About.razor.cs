using KubeUI.Core.Services;
using static KubeUI.Core.Services.Updater;

namespace KubeUI.UI.Pages;

public partial class About
{
    [Inject]
    private ILogger<About> Logger { get; set; }

    [Inject]
    private Updater Updater { get; set; }

    private bool UpdateRequired { get; set; }

    private GithubRelease[] GithubReleases { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            UpdateRequired = await Updater.UpdateRequired();
            GithubReleases = await Updater.GetReleases();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting update information!");
        }
    }
}