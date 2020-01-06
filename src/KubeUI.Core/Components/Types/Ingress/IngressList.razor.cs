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
    public partial class IngressList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<Extensionsv1beta1Ingress, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<IngressList> Logger { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private readonly List<Extensionsv1beta1Ingress> Items = new List<Extensionsv1beta1Ingress>();

        private Watcher<Extensionsv1beta1Ingress> watcher;

        protected override void OnParametersSet()
        {
            Task<HttpOperationResponse<Extensionsv1beta1IngressList>> task;

            if (Namespace?.Equals(State.AllNameSpace) != false)
            {
                task = Client.ListIngressForAllNamespacesWithHttpMessagesAsync(watch: true);
            }
            else
            {
                task = Client.ListNamespacedIngressWithHttpMessagesAsync(Namespace, watch: true);
            }

            watcher = task.Watch<Extensionsv1beta1Ingress, Extensionsv1beta1IngressList>((type, item) =>
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

        private async Task Delete(Extensionsv1beta1Ingress item)
        {
            await Client.DeleteNamespacedIngressAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);
        }

        public void Dispose()
        {
            watcher?.Dispose();
        }
    }
}
