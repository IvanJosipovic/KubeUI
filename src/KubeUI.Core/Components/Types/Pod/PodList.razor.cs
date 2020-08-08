using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class PodList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1Pod, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<PodList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        private readonly List<V1Pod> Items  = new List<V1Pod>();

        private Watcher<V1Pod> watcher;

        protected override void OnParametersSet()
        {
            watcher?.Dispose();

            Task<HttpOperationResponse<V1PodList>> task;

            if (Namespace == null)
            {
                task = State.Client.ListPodForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = State.Client.ListNamespacedPodWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1Pod, V1PodList>((type, item) =>
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

        private async Task Delete(V1Pod item)
        {
            await State.Client.DeleteNamespacedPodAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
