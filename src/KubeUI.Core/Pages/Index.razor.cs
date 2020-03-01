using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KubeUI.Core.Pages
{
    public partial class Index
    {
        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected Updater Updater { get; set; }

        private bool UpdateRequired { get; set; }

        private Updater.GithubRelease GithubRelease;

        protected override async Task OnInitializedAsync()
        {
            GithubRelease = await Updater.GetRelease();
            UpdateRequired = await Updater.UpdateRequired();

        }
    }
}
