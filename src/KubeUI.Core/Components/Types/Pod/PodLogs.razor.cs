using k8s;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class PodLogs : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public string Container { get; set; }

        [Parameter]
        public int Lines { get; set; }

        [Parameter]
        public bool Previous { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private string Logs;

        private Timer timer;

        protected override void OnParametersSet()
        {
            SetTimer();
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        public void SetTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            timer = new Timer(async _ => await Update(), null, 0, 10000);
        }

        private async Task Update()
        {
            var stream = await Client.ReadNamespacedPodLogAsync(Name, Namespace, container: Container, tailLines: Lines, previous: Previous);

            using (var reader = new StreamReader(stream))
            {
                Logs = reader.ReadToEnd();
            }

            StateHasChanged();
        }
    }
}
