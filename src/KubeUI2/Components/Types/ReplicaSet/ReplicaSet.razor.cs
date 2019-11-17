using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    [Route("/{Namespace}/ReplicaSet/{Name}")]
    public partial class ReplicaSet
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState state { get; set; }

        [Inject]
        protected IKubernetes client { get; set; }

        private V1ReplicaSet Item;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }


        private async Task Update()
        {
            Item = await client.ReadNamespacedReplicaSetAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
