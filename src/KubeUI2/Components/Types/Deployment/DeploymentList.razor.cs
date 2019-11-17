﻿using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class DeploymentList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string OwnerUid { get; set; }

        [Inject]
        protected ILogger<DeploymentList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Deployment> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1Deployment> items;

            if (State.Namespace?.Equals(KubeUI.Services.State.AllNameSpace) != false)
            {
                items = (await Client.ListDeploymentForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedDeploymentAsync(State.Namespace))?.Items;
            }

            if (!string.IsNullOrEmpty(OwnerUid))
            {
                items = items.Where(x => x.Metadata.OwnerReferences.Any(y => y.Uid.Equals(OwnerUid))).ToList();
            }

            Items = items;
        }

        public async Task Delete(V1Deployment item)
        {
            await Client.DeleteNamespacedDeploymentAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }

        private async Task ScaleUp(V1Deployment item)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.GetValueOrDefault() + 1);

            await Client.PatchNamespacedDeploymentScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }

        private async Task ScaleDown(V1Deployment item)
        {
            if (!item.Spec.Replicas.HasValue || item.Spec.Replicas.Value == 0)
            {
                return;
            }

            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.Value - 1);

            await Client.PatchNamespacedDeploymentScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);
            
            await Update();
        }
    }
}
