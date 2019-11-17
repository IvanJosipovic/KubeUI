﻿using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class PodList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string OwnerUid { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Pod> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1Pod> items;

            if (Namespace?.Equals(KubeUI.Services.State.AllNameSpace) != false)
            {
                items = (await Client.ListPodForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedPodAsync(Namespace))?.Items;
            }

            if (!string.IsNullOrEmpty(OwnerUid))
            {
                items = items.Where(x => x.Metadata.OwnerReferences.Any(y => y.Uid.Equals(OwnerUid))).ToList();
            }

            Items = items;
        }

        public async Task Delete(V1Pod item)
        {
            await Client.DeleteNamespacedPodAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
