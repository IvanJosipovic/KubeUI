using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class PodList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Pod> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            if (Namespace == null || Namespace.Equals(KubeUI.Services.State.AllNameSpace))
            {
                Items = (await Client.ListPodForAllNamespacesAsync())?.Items;
            }
            else
            {
                Items = (await Client.ListNamespacedPodAsync(Namespace))?.Items;
            }
        }
    }
}
