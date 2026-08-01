using System.Reactive.Disposables;
using System.Reactive.Linq;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

internal static class ClusterResourceChangeFeed
{
    public static IObservable<ResourceChange> Connect(IClusterRuntime runtime)
    {
        return Observable.Create<ResourceChange>(observer =>
        {
            Dictionary<GroupApiVersionKind, IDisposable> subscriptions = [];
            object sync = new();
            var disposed = false;

            void Subscribe(GroupApiVersionKind kind)
            {
                lock (sync)
                {
                    if (disposed || subscriptions.ContainsKey(kind))
                    {
                        return;
                    }
                }

                if (!runtime.Objects.TryGetValue(kind, out var value) || value is not IResourceContainer container)
                {
                    return;
                }

                var subscription = container.ConnectChanges(kind).Subscribe(observer.OnNext, observer.OnError);
                foreach (var resource in container.Snapshot())
                {
                    observer.OnNext(new ResourceChange(WatchEventType.Added, kind, resource));
                }

                lock (sync)
                {
                    if (disposed || subscriptions.ContainsKey(kind))
                    {
                        subscription.Dispose();
                        return;
                    }

                    subscriptions.Add(kind, subscription);
                }
            }

            void ResourceSeeded(IClusterRuntime sender, GroupApiVersionKind kind)
            {
                if (ReferenceEquals(sender, runtime))
                {
                    Subscribe(kind);
                }
            }

            void ResourceUnseeded(IClusterRuntime sender, GroupApiVersionKind kind)
            {
                if (!ReferenceEquals(sender, runtime))
                {
                    return;
                }

                IDisposable? subscription;
                lock (sync)
                {
                    subscriptions.Remove(kind, out subscription);
                }

                subscription?.Dispose();
            }

            runtime.ResourceSeeded += ResourceSeeded;
            runtime.ResourceUnseeded += ResourceUnseeded;

            foreach (var kind in runtime.Objects.Keys)
            {
                Subscribe(kind);
            }

            return Disposable.Create(() =>
            {
                lock (sync)
                {
                    disposed = true;
                }

                runtime.ResourceSeeded -= ResourceSeeded;
                runtime.ResourceUnseeded -= ResourceUnseeded;
                IDisposable[] activeSubscriptions;
                lock (sync)
                {
                    activeSubscriptions = subscriptions.Values.ToArray();
                    subscriptions.Clear();
                }

                foreach (var subscription in activeSubscriptions)
                {
                    subscription.Dispose();
                }
            });
        });
    }
}
