using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class NamespaceList
    {
        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private readonly List<V1Namespace> Items = new List<V1Namespace>();

        private Watcher<V1Namespace> watcher;

        protected override void OnParametersSet()
        {
            watcher = Client.ListNamespaceWithHttpMessagesAsync(watch: true).Watch<V1Namespace, V1NamespaceList>((type, item) =>
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

        private async Task Delete(V1Namespace item)
        {
            await Client.DeleteNamespaceAsync(item.Metadata.Name);
        }
    }
}
