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
using Microsoft.Rest;

namespace KubeUI.Core.Components.Types
{
    public partial class ReplicaSetList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1ReplicaSet, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<ReplicaSetList> Logger { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private readonly List<V1ReplicaSet> Items = new List<V1ReplicaSet>();

        private Watcher<V1ReplicaSet> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<V1ReplicaSetList>> task;

            if (Namespace?.Equals(State.AllNameSpace) != false)
            {
                task = Client.ListReplicaSetForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = Client.ListNamespacedReplicaSetWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1ReplicaSet, V1ReplicaSetList>((type, item) =>
            {
                switch (type)
                {
                    case WatchEventType.Added:
                        if (!Items.Any(x => x.Metadata.Uid == item.Metadata.Uid))
                            Items.Add(item);
                        else
                            Items[Items.FindIndex(x => x.Metadata.Uid == item.Metadata.Uid)] = item;
                        break;
                    case WatchEventType.Modified:
                        Items[Items.FindIndex(x => x.Metadata.Uid == item.Metadata.Uid)] = item;
                        break;
                    case WatchEventType.Deleted:
                        Items.RemoveAt(Items.FindIndex(x => x.Metadata.Uid == item.Metadata.Uid));
                        break;
                    case WatchEventType.Error:
                        break;
                    default:
                        break;
                }
                StateHasChanged();
            });
        }

        private async Task Delete(V1ReplicaSet item)
        {
            await Client.DeleteNamespacedDeploymentAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        private async Task ScaleUp(V1ReplicaSet item)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.GetValueOrDefault() + 1);

            await Client.PatchNamespacedReplicaSetScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);
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
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
