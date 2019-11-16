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
    public partial class PersistentVolumeList
    {
        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1PersistentVolume> Items = new List<V1PersistentVolume>();

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            Items = null;

            StateHasChanged();

            Items = (await Client.ListPersistentVolumeAsync())?.Items;

            StateHasChanged();
        }

        public async Task Delete(V1PersistentVolume item)
        {
            await Client.DeletePersistentVolumeAsync(item.Metadata.Name);

            await Update();
        }
    }
}
