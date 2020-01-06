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
        protected IKubernetes Client { get; set; }

        private V1Pod Item;

        private int LogLineCount { get; set; } = 50;

        private bool PreviousLog { get; set; } = false;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        public void Dispose()
        {
        }

        private async Task Update()
        {
            Item = await Client.ReadNamespacedPodAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
