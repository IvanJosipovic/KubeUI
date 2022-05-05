using Microsoft.AspNetCore.Components;
using KubeUI.Core.Services;
using static KubeUI.Core.Services.Updater;
using Microsoft.Extensions.Logging;

namespace KubeUI.Core.Pages
{
    public partial class About
    {
        [Inject]
        private ILogger<About> Logger { get; set; }

        [Inject]
        private Updater Updater { get; set; }

        private bool UpdateRequired { get; set; }

        private GithubRelease GithubRelease { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                UpdateRequired = await Updater.UpdateRequired();
                GithubRelease = await Updater.GetRelease();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting update information!");
            }
        }
    }
}