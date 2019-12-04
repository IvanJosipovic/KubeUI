using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class PersistentVolumeList
    {
        [Parameter]
        public Expression<Func<V1PersistentVolume, bool>> Filter { get; set; }

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

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        private async Task Delete(V1PersistentVolume item)
        {
            await Client.DeletePersistentVolumeAsync(item.Metadata.Name);

            await Update();
        }
    }
}
