using KubeUI.Services;
using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Pages
{
    public partial class Connect
    {
        [Inject]
        protected IState State { get; set; }

        private string Config { get; set; }

        private void LoadConfig()
        {
            State.SetK8SConfiguration(Config);
        }
    }
}
