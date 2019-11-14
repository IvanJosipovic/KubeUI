using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2.Pages
{
    [Route("/PersistentVolume/{Name}")]
    public partial class PersistentVolume : IDisposable
    {
        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected ILogger<PersistentVolume> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private V1PersistentVolume Item;

        private PropertyChangedEventHandler handler;

        protected override async Task OnInitializedAsync()
        {
            handler = async (xo, e) =>
            {
                if (e.PropertyName == KubeUI.Services.State.UILevelNotification || e.PropertyName == KubeUI.Services.State.NamespaceNotification)
                {
                    await Update();
                }
            };

            State.PropertyChanged += handler;

            await Update();
        }

        public void Dispose()
        {
            State.PropertyChanged -= handler;
        }

        private async Task Update()
        {
            Item = await Client.ReadPersistentVolumeAsync(Name);

            StateHasChanged();
        }
    }
}
