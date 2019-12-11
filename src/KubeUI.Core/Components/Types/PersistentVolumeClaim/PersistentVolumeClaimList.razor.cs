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
    public partial class PersistentVolumeClaimList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1PersistentVolumeClaim, bool>> Filter { get; set; }

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
            IList<V1PersistentVolumeClaim> items;

            if (Namespace == null || Namespace.Equals(KubeUI.Services.State.AllNameSpace))
            {
                items = (await Client.ListPersistentVolumeClaimForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedPersistentVolumeClaimAsync(Namespace))?.Items;
            }

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        private async Task Delete(V1PersistentVolumeClaim item)
        {
            await Client.DeleteNamespacedPersistentVolumeClaimAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
