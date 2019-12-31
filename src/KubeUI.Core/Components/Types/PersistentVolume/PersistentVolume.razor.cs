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
    [Route("/PersistentVolume/{Name}")]
    public partial class PersistentVolume : IDisposable
    {
        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected ILogger<PersistentVolume> Logger { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private V1PersistentVolume Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        public void Dispose()
        {
        }

        private async Task Update()
        {
            Item = await Client.ReadPersistentVolumeAsync(Name);

            StateHasChanged();
        }
    }
}
