using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class CustomObjectList : IDisposable
    {
        [Inject]
        protected IState State { get; set; }

        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Group { get; set; }

        [Parameter]
        public string Version { get; set; }

        [Parameter]
        public string Plural { get; set; }

        private List<JObject> Items = new List<JObject>();

        private Watcher<JObject> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<object>> task;

            if (Namespace == null)
            {
                task = State.Client.ListClusterCustomObjectWithHttpMessagesAsync(Group, Version, Plural, watch: true);
            }
            else
            {
                task = State.Client.ListNamespacedCustomObjectWithHttpMessagesAsync(Group, Version, Namespace, Plural, watch: true);
            }

            watcher = task.Watch<JObject, object> ((type, item) =>
            {
                switch (type)
                {
                    case WatchEventType.Added:
                        if (!Items.Any(x => x["metadata"]["uid"].Value<string>() == item["metadata"]["uid"].Value<string>()))
                            Items.Add(item);
                        else
                            Items[Items.FindIndex(x => x["metadata"]["uid"].Value<string>() == item["metadata"]["uid"].Value<string>())] = item;
                        break;
                    case WatchEventType.Modified:
                        Items[Items.FindIndex(x => x["metadata"]["uid"].Value<string>() == item["metadata"]["uid"].Value<string>())] = item;
                        break;
                    case WatchEventType.Deleted:
                        Items.RemoveAt(Items.FindIndex(x => x["metadata"]["uid"].Value<string>() == item["metadata"]["uid"].Value<string>()));
                        break;
                    case WatchEventType.Error:
                        break;
                    default:
                        break;
                }
                StateHasChanged();
            });
        }

        private async Task Delete(JObject obj)
        {
            if (Namespace == null)
            {
                await State.Client.DeleteClusterCustomObjectAsync(Group, Version, Plural, obj["metadata"]["name"].Value<string>());
            }
            else
            {
                await State.Client.DeleteNamespacedCustomObjectAsync(Group, Version, Namespace, Plural, obj["metadata"]["name"].Value<string>());
            }
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
