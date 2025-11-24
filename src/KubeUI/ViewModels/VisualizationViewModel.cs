using System.Collections.Specialized;
using AvaloniaGraphControl;
using k8s;
using k8s.Models;
using KubeUI.Client;
using static AvaloniaGraphControl.GraphPanel;
using Cluster = KubeUI.Client.Cluster;

namespace KubeUI.ViewModels;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    [ObservableProperty]
    public partial ICluster? Cluster { get; set; }

    [ObservableProperty]
    public partial Graph Graph { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<ResourceNodeViewModel> Resources { get; set; } = [];

    [ObservableProperty]
    public partial bool HideNoise { get; set; } = true;

    [ObservableProperty]
    public partial LayoutMethods LayoutMethod { get; set; } = LayoutMethods.SugiyamaScheme;

    public VisualizationViewModel()
    {
        Title = Assets.Resources.VisualizationViewModel_Title;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(HideNoise))
        {
            Run();
        }
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;

        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        Id = nameof(VisualizationViewModel) + "-" + cluster;

        _ = cluster.SeedResource<V1Node>();

        // Workloads
        _ = cluster.SeedResource<V1Pod>();
        _ = cluster.SeedResource<V1ReplicaSet>();
        _ = cluster.SeedResource<V1Deployment>();
        _ = cluster.SeedResource<V1StatefulSet>();
        _ = cluster.SeedResource<V1DaemonSet>();
        _ = cluster.SeedResource<V1CronJob>();
        _ = cluster.SeedResource<V1Job>();

        // Configuration
        _ = cluster.SeedResource<V1Secret>();
        _ = cluster.SeedResource<V1ConfigMap>();

        // Network
        _ = cluster.SeedResource<V1Service>();
        _ = cluster.SeedResource<V1EndpointSlice>();
        _ = cluster.SeedResource<V1Ingress>();
        _ = cluster.SeedResource<V1IngressClass>();

        // Storage
        _ = cluster.SeedResource<V1PersistentVolumeClaim>();
        _ = cluster.SeedResource<V1PersistentVolume>();

        // Access Control
        _ = cluster.SeedResource<V1ServiceAccount>();

        Run();
    }

    public void Run()
    {
        if (Cluster.SelectedNamespaces.Count == 0)
        {
            return;
        }

        Graph.Edges.Clear();
        Resources.Clear();
        PopulateAllResources();

        LinkOwners();
        LinkEvent();

        LinkIngress();
        LinkEndpoints();
        LinkEndpointSlice();

        LinkConfigMap();
        LinkSecret();

        LinkServiceAccount();

        LinkPersistantVolumeClaim();
        LinkPersistantVolume();

        RemoveDuplicateConnections();

        OnPropertyChanged(nameof(Graph));
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Run();
    }

    private void PopulateAllResources()
    {
        foreach (var kvp in Cluster.Objects)
        {
            var container = kvp.Value;

            var items = container.GetType().GetProperty("Items").GetValue(container);

            foreach (object item in (IList)items.GetType().GetProperty("Items").GetValue(items))
            {
                var value = (IKubernetesObject<V1ObjectMeta>)item;

                if (value is not V1PersistentVolume)
                {
                    if (Cluster.SelectedNamespaces.Count == 0 || !Cluster.SelectedNamespaces.Any(x => x.Name() == value.Namespace()))
                    {
                        continue;
                    }
                }

                // HideNoise
                if (HideNoise
                    && (
                           (value is V1ReplicaSet replicaSet && replicaSet.Status.Replicas == 0)
                        || (value is Corev1Event)
                    ))
                {
                    continue;
                }

                var node = new ResourceNodeViewModel
                {
                    Resource = value,
                    IconPath = Utilities.GetKubeAssetPath(value.GetType())
                };

                Resources.Add(node);
            }
        }
    }

    private void RemoveDuplicateConnections()
    {
        if (Graph?.Edges == null || Graph.Edges.Count < 2)
        {
            return;
        }

        var seen = new HashSet<(object? Head, object? Tail)>();

        // Iterate backwards so we can remove in-place
        for (int i = Graph.Edges.Count - 1; i >= 0; i--)
        {
            var edge = Graph.Edges.ElementAt(i);
            var key = (edge.Head, edge.Tail);

            if (!seen.Add(key))
            {
                // Duplicate found, remove it
                Graph.Edges.Remove(edge);
            }
        }
    }

    #region Links

    private void LinkOwners()
    {
        foreach (var end in Resources)
        {
            if (end.Resource.Metadata.OwnerReferences != null)
            {
                foreach (var ownerReference in end.Resource.Metadata.OwnerReferences)
                {
                    var start = Resources.FirstOrDefault(x => x.Resource.Uid() == ownerReference.Uid);

                    if (start != null)
                    {
                        Graph.Edges.Add(new Edge(start, end));
                    }
                }
            }
        }
    }

    private void LinkIngress()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1Ingress ingress)
            {
                if (ingress.Spec.Rules != null)
                {
                    foreach (var serviceBackend in ingress.Spec.Rules
                                                    .SelectMany(x => x.Http.Paths.Where(y => y.Backend?.Service != null)
                                                    .Select(y => y.Backend.Service)))
                    {
                        foreach (var end in Resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Name() == serviceBackend.Name && service.Namespace() == ingress.Namespace())
                                {
                                    Graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }

                if (ingress.Spec.DefaultBackend != null)
                {
                    if (ingress.Spec.DefaultBackend.Service != null)
                    {
                        foreach (var end in Resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Name() == ingress.Spec.DefaultBackend.Service.Name && service.Namespace() == ingress.Namespace())
                                {
                                    Graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }

                    //todo DefaultBackend.Resource
                }

                //todo Backend.Resource
            }
        }
    }

    private void LinkEndpoints()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1Endpoints endpoint)
            {
                if (endpoint.Subsets == null)
                {
                    continue;
                }

                foreach (var subset in endpoint.Subsets)
                {
                    if (subset.Addresses == null)
                    {
                        continue;
                    }

                    foreach (var address in subset.Addresses)
                    {
                        if (address.TargetRef == null)
                        {
                            continue;
                        }

                        foreach (var end in Resources)
                        {
                            if (end.Resource is V1Pod pod)
                            {
                                if (pod.Uid() == address.TargetRef.Uid)
                                {
                                    Graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void LinkEndpointSlice()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1EndpointSlice endpointSlice)
            {
                if (endpointSlice.Endpoints == null)
                {
                    continue;
                }

                foreach (var endpoint in endpointSlice.Endpoints)
                {
                    if (endpoint.TargetRef == null)
                    {
                        continue;
                    }

                    foreach (var end in Resources)
                    {
                        if (end.Resource is V1Pod pod)
                        {
                            if (pod.Uid() == endpoint.TargetRef.Uid)
                            {
                                Graph.Edges.Add(new Edge(start, end));
                            }
                        }
                    }
                }
            }
        }
    }

    private void LinkConfigMap()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == deployment.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == replicaSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == statefulSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            Graph.Edges.Add(new Edge(start, end));
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == daemonSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                if (pod.Spec.Containers != null)
                {
                    foreach (var container in pod.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod.Spec.InitContainers != null)
                {
                    foreach (var container in pod.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod.Spec.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == pod.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void LinkSecret()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == deployment.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == replicaSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == statefulSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet.Spec.Template.Spec.Containers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet.Spec.Template.Spec.InitContainers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet.Spec.Template.Spec.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == daemonSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                if (pod.Spec.Containers != null)
                {
                    foreach (var container in pod.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod.Spec.InitContainers != null)
                {
                    foreach (var container in pod.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                Graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod.Spec.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == pod.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ServiceAccount serviceAccount)
            {
                if (serviceAccount.Secrets != null)
                {
                    foreach (var secretReference in serviceAccount.Secrets)
                    {
                        foreach (var end in Resources)
                        {
                            if (end.Resource is V1Secret secret)
                            {
                                if (secret.Uid() == secretReference.Uid && end.Resource.Namespace() == serviceAccount.Namespace())
                                {
                                    Graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void LinkEvent()
    {
        foreach (var end in Resources)
        {
            if (end.Resource is Corev1Event @event)
            {
                foreach (var start in Resources)
                {
                    if (start.Resource.Uid() == @event.InvolvedObject.Uid)
                    {
                        Graph.Edges.Add(new Edge(start, end));
                    }
                }
            }
        }
    }

    private void LinkServiceAccount()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                foreach (var end in Resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount &&
                        serviceAccount.Name() == deployment.Spec.Template.Spec.ServiceAccountName &&
                        end.Resource.Namespace() == deployment.Namespace())
                    {
                        Graph.Edges.Add(new Edge(start, end));
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                foreach (var end in Resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == replicaSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Namespace() == replicaSet.Namespace())
                        {
                            Graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                foreach (var end in Resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == daemonSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Namespace() == daemonSet.Namespace())
                        {
                            Graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                foreach (var end in Resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == statefulSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Namespace() == statefulSet.Namespace())
                        {
                            Graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                foreach (var end in Resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == pod.Spec.ServiceAccountName && end.Resource.Namespace() == pod.Namespace())
                        {
                            Graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }
        }
    }

    private void LinkPersistantVolumeClaim()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == deployment.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == replicaSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == statefulSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == daemonSet.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                if (pod.Spec.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in Resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == pod.Namespace())
                                    {
                                        Graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void LinkPersistantVolume()
    {
        foreach (var start in Resources)
        {
            if (start.Resource is V1PersistentVolumeClaim persistentVolumeClaim)
            {
                if (persistentVolumeClaim?.Spec?.VolumeName != null)
                {
                    foreach (var end in Resources)
                    {
                        if (end.Resource is V1PersistentVolume persistentVolume)
                        {
                            if (persistentVolume.Name() == persistentVolumeClaim.Spec.VolumeName)
                            {
                                Graph.Edges.Add(new Edge(start, end));
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    public void Dispose()
    {
        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    public sealed partial class ResourceNodeViewModel: ObservableObject
    {
        [ObservableProperty]
        public partial IKubernetesObject<V1ObjectMeta> Resource { get; set; }

        [ObservableProperty]
        public partial string IconPath { get; set; }
    }
}
