using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class PersistentVolumeList
    {
        [Parameter]
        public string OwnerUid { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1PersistentVolume> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1PersistentVolume> items;

            items = (await Client.ListPersistentVolumeAsync())?.Items;

            if (!string.IsNullOrEmpty(OwnerUid))
            {
                items = items.Where(x => x.Spec.ClaimRef.Uid.Equals(OwnerUid)).ToList();
            }

            Items = items;
        }

        public async Task Delete(V1PersistentVolume item)
        {
            await Client.DeletePersistentVolumeAsync(item.Metadata.Name);

            await Update();
        }
    }
}
