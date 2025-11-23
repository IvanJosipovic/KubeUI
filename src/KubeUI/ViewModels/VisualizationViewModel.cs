using System.Collections.Specialized;
using AvaloniaGraphControl;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Views;
using OpenTelemetry.Resources;
using Yarp.Kubernetes.Controller.Client;
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
            Clear();
            return;
        }

        Clear();
        PopulateAllResources();

        LinkOwners();
        LinkEvent();

        LinkIngress();
        LinkEndpoints();
        LinkEndpointSlice();

        LinkConfigMap();
        LinkSecret();

        LinkServiceAccount();
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Run();
    }

    private void Clear()
    {
        Resources.Clear();
        Graph.Edges.Clear();
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

                if (Cluster.SelectedNamespaces.Count == 0 || !Cluster.SelectedNamespaces.Any(x => x.Metadata.Name == value.Namespace()))
                {
                    continue;
                }

                // Remove inactive ReplicaSets
                if (HideNoise && value is V1ReplicaSet replicaSet && replicaSet.Status.Replicas == 0)
                {
                    continue;
                }

                var node = new ResourceNodeViewModel
                {
                    Resource = value,
                    Kind = kvp.Key,
                    IconPath = Utilities.GetKubeAssetPath(value.GetType())
                };

                Resources.Add(node);
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
                    var start = Resources.FirstOrDefault(x => x.Resource.Metadata.Uid == ownerReference.Uid);

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
                                                    .SelectMany(x => x.Http.Paths.Where(y => y.Backend != null && y.Backend.Service != null)
                                                    .Select(y => y.Backend.Service)))
                    {
                        foreach (var end in Resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Metadata.Name == serviceBackend.Name && service.Metadata.NamespaceProperty == ingress.Metadata.NamespaceProperty)
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
                                if (service.Metadata.Name == ingress.Spec.DefaultBackend.Service.Name && service.Metadata.NamespaceProperty == ingress.Metadata.NamespaceProperty)
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
                                if (pod.Metadata.Uid == address.TargetRef.Uid)
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
                            if (pod.Metadata.Uid == endpoint.TargetRef.Uid)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                if (env.ValueFrom != null && env.ValueFrom.SecretKeyRef != null)
                                {
                                    foreach (var end in Resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
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
                                if (secret.Metadata.Uid == secretReference.Uid && end.Resource.Metadata.NamespaceProperty == serviceAccount.Metadata.NamespaceProperty)
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
                    if (start.Resource.Metadata.Uid == @event.InvolvedObject.Uid && start.Resource.Metadata.NamespaceProperty == @event.Metadata.NamespaceProperty)
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
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Metadata.Name == deployment.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                        {
                            Graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                foreach (var end in Resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Metadata.Name == replicaSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
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
                        if (serviceAccount.Metadata.Name == daemonSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
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
                        if (serviceAccount.Metadata.Name == statefulSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
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
                        if (serviceAccount.Metadata.Name == pod.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == pod.Metadata.NamespaceProperty)
                        {
                            Graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }
        }
    }

    #endregion

    public ResourceNodeViewModel CreateResource(IKubernetesObject<V1ObjectMeta> resource, GroupApiVersionKind kind)
    {
        var node = new ResourceNodeViewModel
        {
            Resource = resource,
            Kind = kind,
            IconPath = Utilities.GetKubeAssetPath(resource.GetType())
        };

        return node;
    }

    public void Dispose()
    {
        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    public sealed partial class ResourceNodeViewModel: ObservableObject
    {
        [ObservableProperty]
        public partial IKubernetesObject<V1ObjectMeta> Resource { get; set; }

        [ObservableProperty]
        public partial GroupApiVersionKind Kind { get; set; }

        [ObservableProperty]
        public partial string IconPath { get; set; }
    }
}
