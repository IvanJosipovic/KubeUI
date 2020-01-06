using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/Namespace/{Name}")]
    public partial class Namespace
    {
        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private V1Namespace Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            Item = await Client.ReadNamespaceAsync(Name);

            StateHasChanged();
        }
    }
}
