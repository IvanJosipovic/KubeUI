using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/{Namespace}/ReplicaSet/{Name}")]
    public partial class ReplicaSet
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState State { get; set; }

        private V1ReplicaSet Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }


        private async Task Update()
        {
            Item = await State.Client.ReadNamespacedReplicaSetAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
