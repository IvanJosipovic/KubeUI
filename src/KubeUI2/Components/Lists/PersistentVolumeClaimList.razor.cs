using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2
{
    public partial class PersistentVolumeClaimList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1PersistentVolumeClaim> Items = new List<V1PersistentVolumeClaim>();

        private PropertyChangedEventHandler handler;

        protected override async Task OnInitializedAsync()
        {
            handler = async (xo, e) =>
            {
                if (e.PropertyName == KubeUI.Services.State.UILevelNotification || e.PropertyName == KubeUI.Services.State.NamespaceNotification)
                {
                    await Update();
                }
            };

            State.PropertyChanged += handler;

            await Update();
        }

        public void Dispose()
        {
            State.PropertyChanged -= handler;
        }

        private async Task Update()
        {
            Items = null;

            StateHasChanged();

            if (Namespace == null || Namespace.Equals(KubeUI.Services.State.AllNameSpace))
            {
                Items = (await Client.ListPersistentVolumeClaimForAllNamespacesAsync())?.Items;
            }
            else
            {
                Items = (await Client.ListNamespacedPersistentVolumeClaimAsync(Namespace))?.Items;
            }

            StateHasChanged();
        }

        public async Task Delete(V1PersistentVolumeClaim item)
        {
            await Client.DeleteNamespacedPersistentVolumeClaimAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
