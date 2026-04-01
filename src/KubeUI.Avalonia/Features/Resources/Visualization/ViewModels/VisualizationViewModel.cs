using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using System.Collections.Specialized;
using System.Reactive.Subjects;
using AvaloniaGraphControl;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Kubernetes;
using static AvaloniaGraphControl.GraphPanel;

namespace KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    [ObservableProperty]
    public partial Graph Graph { get; set; }

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

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        // Detach from any previous cluster to avoid duplicate event subscriptions
        if (Cluster != null && Cluster.SelectedNamespaces != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        }

        Cluster = cluster;

        if (Cluster?.SelectedNamespaces != null)
        {
            // Ensure single subscription
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
            Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
        }

        Id = nameof(VisualizationViewModel) + "-" + cluster;

        _ = cluster.SeedResource<V1Node>();

        _ = cluster.SeedResource<Corev1Event>();

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
        _ = cluster.SeedResource<V1RoleBinding>();
        _ = cluster.SeedResource<V1ClusterRoleBinding>();
        _ = cluster.SeedResource<V1Role>();
        _ = cluster.SeedResource<V1ClusterRole>();

        Run();
    }

    private void Run()
    {
        if (Cluster == null || Cluster.SelectedNamespaces.Count == 0)
        {
            Graph = new Graph();
            Resources.Clear();
            return;
        }

        var graph = new Graph();
        Resources.Clear();
        PopulateAllResources();

        LinkOwners(Resources, graph);
        LinkEvent(Resources, graph);

        LinkIngress(Resources, graph);
        LinkEndpointSlice(Resources, graph);

        LinkConfigMap(Resources, graph);
        LinkSecret(Resources, graph);

        LinkArgoCDTracking(Resources, graph);

        LinkServiceAccount(Resources, graph);

        LinkPersistantVolumeClaim(Resources, graph);
        LinkPersistantVolume(Resources, graph);

        LinkRoleBinding(Resources, graph);
        LinkClusterRoleBinding(Resources, graph);

        RemoveDuplicateConnections(graph);

        RemoveNonRelatedToNamespace(graph);

        Graph = graph;
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Run();
    }

    private void PopulateAllResources()
    {
        if (Cluster?.Objects == null)
        {
            return;
        }

        foreach (var kvp in Cluster.Objects)
        {
            var container = kvp.Value;

            var itemsProperty = container.GetType().GetProperty("Items");
            var items = itemsProperty?.GetValue(container);
            var nestedItemsProperty = items?.GetType().GetProperty("Items");
            var nestedItems = nestedItemsProperty?.GetValue(items) as IList;

            if (nestedItems == null)
            {
                continue;
            }

            foreach (object item in nestedItems)
            {
                var value = (IKubernetesObject<V1ObjectMeta>)item;

                if (!string.IsNullOrEmpty(value.Namespace()) && !Cluster.SelectedNamespaces.Any(x => x.Name() == value.Namespace()))
                {
                    continue;
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
                    Cluster = Cluster,
                    Resource = value,
                    IconPath = Utilities.GetKubeAssetPath(value.GetType())
                };

                Resources.Add(node);
            }
        }
    }

    private static void RemoveDuplicateConnections(Graph graph)
    {
        if (graph?.Edges == null || graph.Edges.Count < 2)
        {
            return;
        }

        var seen = new HashSet<(object? Head, object? Tail)>();

        // Iterate backwards so we can remove in-place
        for (int i = graph.Edges.Count - 1; i >= 0; i--)
        {
            var edge = graph.Edges.ElementAt(i);
            var key = (edge.Head, edge.Tail);

            if (!seen.Add(key))
            {
                // Duplicate found, remove it
                graph.Edges.Remove(edge);
            }
        }
    }

    private void RemoveNonRelatedToNamespace(Graph graph)
    {
        if (graph?.Edges == null || graph.Edges.Count == 0 || Cluster == null)
        {
            return;
        }

        var selectedNamespaces = new HashSet<string>(
            Cluster.SelectedNamespaces
                .Where(n => !string.IsNullOrEmpty(n.Name()))
                .Select(n => n.Name()!));

        if (selectedNamespaces.Count == 0)
        {
            return;
        }

        // Collect all resource nodes participating in edges
        var nodes = new HashSet<ResourceNodeViewModel>();
        foreach (var edge in graph.Edges)
        {
            if (edge.Head is ResourceNodeViewModel head)
            {
                nodes.Add(head);
            }

            if (edge.Tail is ResourceNodeViewModel tail)
            {
                nodes.Add(tail);
            }
        }

        if (nodes.Count == 0)
        {
            return;
        }

        // Determine which nodes are in a selected namespace
        var nodeIsInSelectedNamespace = new Dictionary<ResourceNodeViewModel, bool>();
        foreach (var node in nodes)
        {
            var ns = node.Resource?.Namespace();
            nodeIsInSelectedNamespace[node] = ns != null && selectedNamespaces.Contains(ns);
        }

        // Build undirected adjacency list
        var adjacency = new Dictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>>();
        foreach (var node in nodes)
        {
            adjacency[node] = new List<ResourceNodeViewModel>();
        }

        foreach (var edge in graph.Edges)
        {
            if (edge.Head is ResourceNodeViewModel head && edge.Tail is ResourceNodeViewModel tail)
            {
                adjacency[head].Add(tail);
                adjacency[tail].Add(head);
            }
        }

        var visited = new HashSet<ResourceNodeViewModel>();
        var edgesToRemove = new HashSet<Edge>();

        // Traverse connected components; keep only components that have at least one node in a selected namespace
        foreach (var start in nodes)
        {
            if (!visited.Add(start))
            {
                continue;
            }

            var componentNodes = new List<ResourceNodeViewModel>();
            var queue = new Queue<ResourceNodeViewModel>();
            queue.Enqueue(start);

            var componentHasSelectedNamespaceNode = false;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                componentNodes.Add(current);

                if (nodeIsInSelectedNamespace[current])
                {
                    componentHasSelectedNamespaceNode = true;
                }

                foreach (var neighbor in adjacency[current])
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (!componentHasSelectedNamespaceNode)
            {
                var componentSet = new HashSet<ResourceNodeViewModel>(componentNodes);

                // Mark all edges entirely contained in this component for removal
                foreach (var edge in graph.Edges)
                {
                    if (edge.Head is ResourceNodeViewModel head && edge.Tail is ResourceNodeViewModel tail)
                    {
                        if (componentSet.Contains(head) && componentSet.Contains(tail))
                        {
                            edgesToRemove.Add(edge);
                        }
                    }
                }
            }
        }

        // Remove edges not related to any selected namespace chain
        if (edgesToRemove.Count > 0)
        {
            foreach (var edge in edgesToRemove)
            {
                graph.Edges.Remove(edge);
            }
        }
    }

    #region Links

    private static void LinkOwners(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var end in resources)
        {
            if (end.Resource?.Metadata?.OwnerReferences != null)
            {
                foreach (var ownerReference in end.Resource.Metadata.OwnerReferences)
                {
                    if (ownerReference.Kind == V1Namespace.KubeKind)
                    {
                        continue;
                    }

                    var start = resources.FirstOrDefault(x => x.Resource.Uid() == ownerReference.Uid);

                    if (start != null)
                    {
                        graph.Edges.Add(new Edge(start, end));
                    }
                }
            }
        }
    }

    private static void LinkIngress(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Ingress ingress)
            {
                if (ingress?.Spec?.Rules != null)
                {
                    foreach (var serviceBackend in ingress.Spec.Rules
                                                    .SelectMany(x => x.Http.Paths.Where(y => y.Backend?.Service != null)
                                                    .Select(y => y.Backend.Service)))
                    {
                        foreach (var end in resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Name() == serviceBackend.Name && service.Namespace() == ingress.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }

                if (ingress?.Spec?.DefaultBackend != null)
                {
                    if (ingress.Spec.DefaultBackend.Service != null)
                    {
                        foreach (var end in resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Name() == ingress.Spec.DefaultBackend.Service.Name && service.Namespace() == ingress.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
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

    private static void LinkEndpointSlice(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
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

                    foreach (var end in resources)
                    {
                        if (end.Resource is V1Pod pod)
                        {
                            if (pod.Uid() == endpoint.TargetRef.Uid)
                            {
                                graph.Edges.Add(new Edge(start, end));
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkConfigMap(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == deployment.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == replicaSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == statefulSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom?.ConfigMapKeyRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == daemonSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == pod.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkSecret(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == deployment.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == replicaSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == statefulSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == daemonSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod?.Spec?.InitContainers != null)
                {
                    foreach (var container in pod.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
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
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod?.Spec?.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == pod.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                        foreach (var end in resources)
                        {
                            if (end.Resource is V1Secret secret)
                            {
                                if (secret.Uid() == secretReference.Uid && end.Resource.Namespace() == serviceAccount.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkEvent(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is Corev1Event @event)
            {
                foreach (var end in resources)
                {
                    if (end.Resource.Uid() == @event.InvolvedObject.Uid)
                    {
                        graph.Edges.Add(new Edge(start, end));
                    }
                }
            }
        }
    }

    private static void LinkServiceAccount(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount &&
                        serviceAccount.Name() == deployment.Spec.Template.Spec.ServiceAccountName &&
                        end.Resource.Namespace() == deployment.Namespace())
                    {
                        graph.Edges.Add(new Edge(start, end));
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == replicaSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Namespace() == replicaSet.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == daemonSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Namespace() == daemonSet.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == statefulSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Namespace() == statefulSet.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == pod.Spec.ServiceAccountName && end.Resource.Namespace() == pod.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }
        }
    }

    private static void LinkPersistantVolumeClaim(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == deployment.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == replicaSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == statefulSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == daemonSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
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
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == pod.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkPersistantVolume(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1PersistentVolumeClaim persistentVolumeClaim)
            {
                if (persistentVolumeClaim?.Spec?.VolumeName != null)
                {
                    foreach (var end in resources)
                    {
                        if (end.Resource is V1PersistentVolume persistentVolume)
                        {
                            if (persistentVolume.Name() == persistentVolumeClaim.Spec.VolumeName)
                            {
                                graph.Edges.Add(new Edge(start, end));
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkRoleBinding(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1RoleBinding roleBinding)
            {
                if (roleBinding.Subjects != null)
                {
                    foreach (var subject in roleBinding.Subjects)
                    {
                        foreach (var end in resources)
                        {
                            if (subject.Kind == V1ServiceAccount.KubeKind)
                            {
                                if (end.Resource is V1ServiceAccount serviceAccount)
                                {
                                    if (serviceAccount.Name() == subject.Name
                                        && serviceAccount.Namespace() == subject.NamespaceProperty)
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }

                            //todo User

                            //todo Group
                        }
                    }
                }

                if (roleBinding.RoleRef != null)
                {
                    foreach (var end in resources)
                    {
                        if (roleBinding.RoleRef.Kind == V1Role.KubeKind)
                        {
                            if (end.Resource is V1Role role)
                            {
                                if (role.Name() == roleBinding.RoleRef.Name
                                    && role.Namespace() == roleBinding.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }

                        if (roleBinding.RoleRef.Kind == V1ClusterRole.KubeKind)
                        {
                            if (end.Resource is V1ClusterRole clusterRole)
                            {
                                if (clusterRole.Name() == roleBinding.RoleRef.Name)
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkClusterRoleBinding(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1ClusterRoleBinding roleBinding)
            {
                if (roleBinding.Subjects != null)
                {
                    foreach (var subject in roleBinding.Subjects)
                    {
                        foreach (var end in resources)
                        {
                            if (subject.Kind == V1ServiceAccount.KubeKind)
                            {
                                if (end.Resource is V1ServiceAccount serviceAccount)
                                {
                                    if (serviceAccount.Name() == subject.Name
                                        && serviceAccount.Namespace() == subject.NamespaceProperty)
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }

                            //todo User

                            //todo Group
                        }
                    }
                }

                if (roleBinding.RoleRef != null)
                {
                    foreach (var end in resources)
                    {
                        if (roleBinding.RoleRef.Kind == V1ClusterRole.KubeKind)
                        {
                            if (end.Resource is V1ClusterRole clusterRole)
                            {
                                if (clusterRole.Name() == roleBinding.RoleRef.Name)
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkArgoCDTracking(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var resource in resources)
        {
            var annotations = resource.Resource?.Metadata?.Annotations;
            if (annotations != null && annotations.TryGetValue("argocd.argoproj.io/tracking-id", out var trackingId))
            {
                var firstColon = trackingId.IndexOf(':');
                if (firstColon > 0)
                {
                    var appName = trackingId.Substring(0, firstColon);
                    // Find Argo Application resource with matching appName, ApiVersion, and Kind
                    var argoApp = resources.FirstOrDefault(r =>
                        r.Resource?.Metadata?.Name == appName &&
                        r.Resource?.ApiVersion == "argoproj.io/v1alpha1" &&
                        r.Resource?.Kind == "Application");
                    if (argoApp != null)
                    {
                        graph.Edges.Add(new Edge(argoApp, resource));
                    }
                }
            }
        }
    }

    #endregion

    public void Dispose()
    {
        if (Cluster?.SelectedNamespaces != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        }
        Cluster = null;
        // Clear heavy collections to avoid retaining large object graphs between runs
        try
        {
            Resources?.Clear();
        }
        catch { }

        try
        {
            Graph = new Graph();
        }
        catch { }
    }

    public sealed partial class ResourceNodeViewModel: ViewModelBase
    {
        [ObservableProperty]
        public partial ClusterWorkspaceViewModel Cluster { get; set; }

        [ObservableProperty]
        public partial IKubernetesObject<V1ObjectMeta> Resource { get; set; }

        [ObservableProperty]
        public partial string IconPath { get; set; }

        [RelayCommand]
        private void ViewYaml(IKubernetesObject<V1ObjectMeta> resource)
        {
            var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

            vm.Initialize(Cluster, resource);

            Factory.AddToBottom(vm);
        }

        [RelayCommand]
        private void ViewProperties(IKubernetesObject<V1ObjectMeta> resource)
        {
            var propType = typeof(ResourcePropertiesViewModel<>).MakeGenericType(resource.GetType());

            var instance = Application.Current.GetRequiredService(propType) as IDockable;
            instance.CanFloat = false;

            propType.GetMethod(nameof(ResourcePropertiesViewModel<V1Pod>.Initialize)).Invoke(instance, [Cluster, resource]);

            Factory?.AddToRight(instance);
        }
    }
}



