using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/Node/{Name}")]
    public partial class Node
    {
        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState State { get; set; }


        private V1Node Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            Item = await State.Client.ReadNodeAsync(Name);

            StateHasChanged();
        }
    }
}
