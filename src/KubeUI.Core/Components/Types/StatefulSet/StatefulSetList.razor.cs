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
    public partial class StatefulSetList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1StatefulSet, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<StatefulSetList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        private readonly List<V1StatefulSet> Items = new List<V1StatefulSet>();

        private Watcher<V1StatefulSet> watcher;

        protected override void OnParametersSet()
        {
            watcher?.Dispose();

            Task<HttpOperationResponse<V1StatefulSetList>> task;

            if (Namespace == null)
            {
                task = State.Client.ListStatefulSetForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = State.Client.ListNamespacedStatefulSetWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1StatefulSet, V1StatefulSetList>((type, item) =>
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

        private async Task Delete(V1StatefulSet item)
        {
            await State.Client.DeleteNamespacedDeploymentAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        private async Task ScaleUp(V1StatefulSet item)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.GetValueOrDefault() + 1);

            await State.Client.PatchNamespacedStatefulSetScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        private async Task ScaleDown(V1StatefulSet item)
        {
            if (!item.Spec.Replicas.HasValue || item.Spec.Replicas.Value == 0)
            {
                return;
            }

            var patch = new JsonPatchDocument<V1StatefulSet>();
            patch.Replace(e => e.Spec.Replicas, item.Spec.Replicas.Value - 1);

            await State.Client.PatchNamespacedStatefulSetScaleAsync(new V1Patch(patch), item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
