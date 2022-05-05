using Microsoft.AspNetCore.Components;
using KubeUI.Core.Services;
using static KubeUI.Core.Services.Updater;

namespace KubeUI.Core.Pages
{
    public partial class About
    {
        [Inject]
        private Updater Updater { get; set; }

        private bool UpdateRequired { get; set; }

        private GithubRelease GithubRelease { get; set; }

        protected override async Task OnInitializedAsync()
        {
            UpdateRequired = await Updater.UpdateRequired();
            GithubRelease = await Updater.GetRelease();
        }
    }
}