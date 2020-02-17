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
    public partial class CustomResourceDefinitionList : IDisposable
    {
        [Inject]
        protected IState State { get; set; }


        private List<V1CustomResourceDefinition> Items = new List<V1CustomResourceDefinition>();

        private Watcher<V1CustomResourceDefinition> watcher;

        protected override void OnParametersSet()
        {
            watcher = State.Client.ListCustomResourceDefinitionWithHttpMessagesAsync(watch: true).Watch<V1CustomResourceDefinition, V1CustomResourceDefinitionList>((type, item) =>
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

        private async Task Delete(V1CustomResourceDefinition crd)
        {
            await State.Client.DeleteCustomResourceDefinitionAsync(crd.Metadata.Name);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
