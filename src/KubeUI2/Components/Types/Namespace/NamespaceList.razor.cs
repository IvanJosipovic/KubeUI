using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class NamespaceList
    {
        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Namespace> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            Items = (await Client.ListNamespaceAsync())?.Items;
        }

        private async Task Delete(V1Namespace item)
        {
            await Client.DeleteNamespaceAsync(item.Metadata.Name);
            await Update();
        }
    }
}
