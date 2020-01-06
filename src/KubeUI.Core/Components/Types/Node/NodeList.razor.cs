using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class NodeList : IDisposable
    {
        [Parameter]
        public Expression<Func<V1Node, bool>> Filter { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private readonly List<V1Node> Items = new List<V1Node>();

        private Watcher<V1Node> watcher;

        protected override void OnParametersSet()
        {
            watcher = Client.ListNodeWithHttpMessagesAsync(watch: true)
                .Watch<V1Node, V1NodeList>((type, item) =>
            {
                Console.WriteLine(type.ToString());
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

        private async Task Delete(V1Node item)
        {
            await Client.DeleteNodeAsync(item.Metadata.Name);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
