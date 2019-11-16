using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    [Route("/{Namespace}/Pod/{Name}")]
    public partial class Pod : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected ILogger<Pod> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private V1Pod Item;

        private PropertyChangedEventHandler handler;

        private int LogLineCount = 50;

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
            Item = await Client.ReadNamespacedPodAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
