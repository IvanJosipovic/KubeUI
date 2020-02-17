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
    [Route("/{Namespace}/Service/{Name}")]
    public partial class Service : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState State { get; set; }

        private V1Service Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        public void Dispose()
        {
        }

        private async Task Update()
        {
            Item = await State.Client.ReadNamespacedServiceAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
