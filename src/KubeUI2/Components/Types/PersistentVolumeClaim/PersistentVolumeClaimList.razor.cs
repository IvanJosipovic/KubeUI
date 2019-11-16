using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class PersistentVolumeClaimList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1PersistentVolumeClaim> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            if (Namespace == null || Namespace.Equals(KubeUI.Services.State.AllNameSpace))
            {
                Items = (await Client.ListPersistentVolumeClaimForAllNamespacesAsync())?.Items;
            }
            else
            {
                Items = (await Client.ListNamespacedPersistentVolumeClaimAsync(Namespace))?.Items;
            }
        }

        public async Task Delete(V1PersistentVolumeClaim item)
        {
            await Client.DeleteNamespacedPersistentVolumeClaimAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
