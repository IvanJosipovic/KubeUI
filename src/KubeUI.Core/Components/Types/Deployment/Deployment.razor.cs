using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/{Namespace}/Deployment/{Name}")]
    public partial class Deployment : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState State { get; set; }


        private V1Deployment Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        public void Dispose()
        {
        }

        private async Task Update()
        {
            Item = await State.Client.ReadNamespacedDeploymentAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
