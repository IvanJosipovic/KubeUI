using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class ReplicaSetList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1ReplicaSet, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<ReplicaSetList> Logger { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1ReplicaSet> Items;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1ReplicaSet> items;

            if (Namespace?.Equals(State.AllNameSpace) != false)
            {
                items = (await Client.ListReplicaSetForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedReplicaSetAsync(Namespace))?.Items;
            }

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        public async Task Delete(V1ReplicaSet item)
        {
            await Client.DeleteNamespacedDeploymentAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }

        private async Task ScaleUp(V1ReplicaSet item)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.GetValueOrDefault() + 1);

            await Client.PatchNamespacedReplicaSetScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }

        private async Task ScaleDown(V1ReplicaSet item)
        {
            if (!item.Spec.Replicas.HasValue || item.Spec.Replicas.Value == 0)
            {
                return;
            }

            var patch = new JsonPatchDocument<V1ReplicaSet>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.Value - 1);

            await Client.PatchNamespacedReplicaSetScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);
            
            await Update();
        }
    }
}
