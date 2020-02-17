using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/CustomResourceDefinition/{Name}")]
    public partial class CustomResourceDefinition
    {
        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState State { get; set; }


        private V1CustomResourceDefinition Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            Item = await State.Client.ReadCustomResourceDefinitionAsync(Name);

            StateHasChanged();
        }
    }
}
