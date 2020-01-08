using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/{Namespace}/DaemonSet/{Name}")]
    public partial class DaemonSet
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private V1DaemonSet Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            Item = await Client.ReadNamespacedDaemonSetAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
