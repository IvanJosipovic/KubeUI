using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace KubeUI2.Pages
{
    [Route("/{Namespace}/Pod/{Name}")]
    public partial class Pod : IDisposable
    {
        [Parameter] public string Namespace { get; set; }

        [Parameter] public string Name { get; set; }

        [Inject] protected IState state { get; set; }

        [Inject] protected IKubernetes client { get; set; }

        private V1Pod Item { get; set; }

        private string Logs { get; set; }

        PropertyChangedEventHandler handler;

        protected override async Task OnInitializedAsync()
        {
            handler = async (xo, e) =>
            {
                if (e.PropertyName == State.UILevelNotification || e.PropertyName == State.NamespaceNotification)
                {
                    await Update();
                }
            };

            state.PropertyChanged += handler;

            await Update();
        }

        public void Dispose()
        {
            state.PropertyChanged -= handler;
        }

        private async Task Update()
        {
            Item = await client.ReadNamespacedPodAsync(Name, Namespace);

            StateHasChanged();
        }

        private async Task GetLogs()
        {
            var stream = await client.ReadNamespacedPodLogAsync(Name, Namespace);

            using (var reader = new StreamReader(stream))
            {
                Logs = reader.ReadToEnd();
            }

            StateHasChanged();
        }
    }
}
