using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KubeUI.Core.Components
{
    public partial class Namespaces : IDisposable
    {
        [Inject] protected IState State { get; set; }

        [Inject] protected NavigationManager navigationManager { get; set; }

        private readonly List<V1Namespace> Items = new List<V1Namespace>();

        private Watcher<V1Namespace> watcher;

        protected override void OnInitialized()
        {
            State.PropertyChanged += (xo, e) =>
            {
                if (e.PropertyName == Services.State.ContextNotification)
                {
                    Init();
                }
            };
            Init();
        }

        private void Init()
        {
            watcher?.Dispose();
            Items.Clear();
            watcher = State.Client?.ListNamespaceWithHttpMessagesAsync(watch: true)
                .Watch<V1Namespace, V1NamespaceList>((type, item) =>
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

        private void OnChange(ChangeEventArgs args)
        {
            var newVal = args.Value.ToString();

            if (newVal == "0000-0000-0000")
            {
                State.Namespace = null;
            } else
            {
                State.Namespace = newVal;
            }
            
            navigationManager.NavigateTo("/");
        }

        public void Dispose()
        {
            watcher?.Dispose();

            State.PropertyChanged -= (xo, e) =>
            {
                if (e.PropertyName == Services.State.ContextNotification)
                {
                    Init();
                }
            };
        }
    }
}