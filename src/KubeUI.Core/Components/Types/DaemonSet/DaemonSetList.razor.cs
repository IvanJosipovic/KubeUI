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
    public partial class DaemonSetList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1DaemonSet, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<DaemonSetList> Logger { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private static readonly object Locker = new object();

        private readonly List<V1DaemonSet> Items = new List<V1DaemonSet>();

        private Watcher<V1DaemonSet> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<V1DaemonSetList>> task;

            if (Namespace?.Equals(State.AllNameSpace) != false)
            {
                task = Client.ListDaemonSetForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = Client.ListNamespacedDaemonSetWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1DaemonSet, V1DaemonSetList>((type, item) =>
            {
                lock (Locker)
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
                }

                StateHasChanged();
            });
        }

        private async Task Delete(V1DaemonSet item)
        {
            await Client.DeleteNamespacedDeploymentAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
