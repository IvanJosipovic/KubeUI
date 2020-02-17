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
    public partial class PersistentVolumeList : IDisposable
    {
        [Parameter]
        public Expression<Func<V1PersistentVolume, bool>> Filter { get; set; }

        [Inject]
        protected IState State { get; set; }


        private readonly List<V1PersistentVolume> Items = new List<V1PersistentVolume>();

        private Watcher<V1PersistentVolume> watcher;

        protected override void OnParametersSet()
        {
            watcher = State.Client.ListPersistentVolumeWithHttpMessagesAsync(watch: true).Watch<V1PersistentVolume, V1PersistentVolumeList>((type, item) =>
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

        private async Task Delete(V1PersistentVolume item)
        {
            await State.Client.DeletePersistentVolumeAsync(item.Metadata.Name);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
