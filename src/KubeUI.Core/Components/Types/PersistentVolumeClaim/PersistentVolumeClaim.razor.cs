using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/{Namespace}/PersistentVolumeClaim/{Name}")]
    public partial class PersistentVolumeClaim : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected ILogger<PersistentVolumeClaim> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }


        private V1PersistentVolumeClaim Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        public void Dispose()
        {
        }

        private async Task Update()
        {
            Item = await State.Client.ReadNamespacedPersistentVolumeClaimAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
