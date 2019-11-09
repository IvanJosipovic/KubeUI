using k8s;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KubeUI2.Components
{
    public partial class PodLogs : IDisposable
    {
        [Parameter] public string Namespace { get; set; }

        [Parameter] public string Name { get; set; }

        [Parameter] public string Container { get; set; }

        [Inject] protected IKubernetes Client { get; set; }

        private string Logs { get; set; }

        private Timer timer;

        protected override void OnInitialized()
        {
            timer = new Timer(async _ => await Update(), null, 0, 60000);
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        private async Task Update()
        {
            var stream = await Client.ReadNamespacedPodLogAsync(Name, Namespace, container: Container);
            using (var reader = new StreamReader(stream))
            {
                Logs = reader.ReadToEnd();
            }

            StateHasChanged();
        }
    }
}
