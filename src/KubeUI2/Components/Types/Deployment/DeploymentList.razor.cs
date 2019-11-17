using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class DeploymentList
    {
        [Parameter]
        public string Namespace { get; set; }

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
            if (State.Namespace == null || State.Namespace.Equals(KubeUI.Services.State.AllNameSpace))
            {
                Items = (await Client.ListDeploymentForAllNamespacesAsync())?.Items;
            }
            else
            {
                Items = (await Client.ListNamespacedDeploymentAsync(State.Namespace))?.Items;
            }
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
