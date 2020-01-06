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
    public partial class SecretList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1Secret, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<SecretList> Logger { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private readonly List<V1Secret> Items = new List<V1Secret>();

        private Watcher<V1Secret> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<V1SecretList>> task;

            if (Namespace?.Equals(State.AllNameSpace) != false)
            {
                task = Client.ListSecretForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = Client.ListNamespacedSecretWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<V1Secret, V1SecretList>((type, item) =>
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

        private async Task Delete(V1Secret item)
        {
            await Client.DeleteNamespacedSecretAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
