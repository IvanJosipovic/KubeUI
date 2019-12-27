using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace KubeUI.Core.Components.Types
{
    public partial class PersistentVolumeClaimList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1PersistentVolumeClaim, bool>> Filter { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private readonly List<V1PersistentVolumeClaim> Items = new List<V1PersistentVolumeClaim>();

        private Watcher<V1PersistentVolumeClaim> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<V1PersistentVolumeClaimList>> task;

            if (Namespace?.Equals(State.AllNameSpace) != false)
            {
                task = Client.ListPersistentVolumeClaimForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = Client.ListNamespacedPersistentVolumeClaimWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1PersistentVolumeClaim, V1PersistentVolumeClaimList>((type, item) =>
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

        private async Task Delete(V1PersistentVolumeClaim item)
        {
            await Client.DeleteNamespacedPersistentVolumeClaimAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
