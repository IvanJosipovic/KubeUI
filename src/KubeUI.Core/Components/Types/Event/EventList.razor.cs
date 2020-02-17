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
    public partial class EventList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1Event, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<EventList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }


        private readonly List<V1Event> Items = new List<V1Event>();

        private Watcher<V1Event> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<V1EventList>> task;

            if (Namespace == null)
            {
                task = State.Client.ListEventForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = State.Client.ListNamespacedEventWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1Event, V1EventList>((type, item) =>
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

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
