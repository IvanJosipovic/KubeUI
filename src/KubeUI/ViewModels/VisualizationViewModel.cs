using System.Collections.Specialized;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Views;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using Cluster = KubeUI.Client.Cluster;

namespace KubeUI.ViewModels;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    [ObservableProperty]
    private ICluster? _cluster;

    [ObservableProperty]
    private DrawingNodeViewModel _drawing = new()
    {
        Nodes = new ObservableCollection<INode>(),
        Connectors = new ObservableCollection<IConnector>(),
        EnableMultiplePinConnections = true,
        EnableSnap = true,
        SnapX = 10.0,
        SnapY = 10.0,
    };

    [ObservableProperty]
    private bool _hideNoise = true;

    [ObservableProperty]
    private bool _hideUnattached = true;

    private readonly int _resourceSize = 60;

    private int _resourceSpacing => _resourceSize * 2;

    public VisualizationViewModel()
    {
        Title = Resources.VisualizationViewModel_Title;
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

        cluster.Seed<V1Node>();

        // Workloads
        cluster.Seed<V1Pod>();
        cluster.Seed<V1ReplicaSet>();
        cluster.Seed<V1Deployment>();
        cluster.Seed<V1StatefulSet>();
        cluster.Seed<V1DaemonSet>();
        cluster.Seed<V1CronJob>();
        cluster.Seed<V1Job>();

        // Configuration
        cluster.Seed<V1Secret>();
        cluster.Seed<V1ConfigMap>();

        // Network
        cluster.Seed<V1Service>();
        cluster.Seed<V1EndpointSlice>();
        cluster.Seed<V1Ingress>();
        cluster.Seed<V1IngressClass>();

        // Storage
        cluster.Seed<V1PersistentVolumeClaim>();
        cluster.Seed<V1PersistentVolume>();

        // Access Control
        cluster.Seed<V1ServiceAccount>();

        Run();
    }

    private void Run()
    {
        if (Cluster.SelectedNamespaces.Count == 0)
        {
            Clear();
            return;
        }

        PopulateAllResources();

        LinkOwners();
        LinkEvent();

        LinkIngress();
        LinkEndpoints();
        LinkEndpointSlice();

        LinkConfigMap();
        LinkSecret();

        LinkServiceAccount();

        //Lo

        OrderResourcesByConnections();
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Run();
    }

    private void Clear()
    {
        Drawing.Nodes.Clear();
        Drawing.Connectors.Clear();
    }

    private void PopulateAllResources()
    {
        Clear();

        foreach (var kind in Cluster.Objects)
        {
            foreach (object item in kind.Value.Items)
            {
                var key = (NamespacedName)item.GetType().GetProperty("Key").GetValue(item);
                var value = (IKubernetesObject<V1ObjectMeta>)item.GetType().GetProperty("Value").GetValue(item);

                if (Cluster.SelectedNamespaces.Count == 0 || !Cluster.SelectedNamespaces.Any(x => x.Metadata.Name == key.Namespace))
                {
                    continue;
                }

                // Remove inactive ReplicaSets
                if (HideNoise && value is V1ReplicaSet replicaSet && replicaSet.Status.Replicas == 0)
                {
                    continue;
                }

                var resource = CreateResource(value, kind.Key);
                resource.Parent = Drawing;
                Drawing.Nodes.Add(resource);
            }
        }
    }

    private void OrderResourcesByConnections()
    {
        var x = 0;
        var y = 0;

        var items = Drawing.Nodes.OfType<ResourceNodeViewModel>();

        var groups = items.GroupBy(x => x.Kind);

        var order = groups.OrderBy(x => x.Sum(y => y.Pins.Count(y => y.Alignment == PinAlignment.Left)));

        foreach (var group in order)
        {
            foreach (var item in group)
            {
                item.X = (_resourceSize + (_resourceSpacing)) * x;
                item.Y = (_resourceSize + (_resourceSpacing)) * y;
                y++;
            }

            y = 0;
            x++;
        }
    }

    #region Links

    private void LinkOwners()
    {
        foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (end.Resource.Metadata.OwnerReferences != null)
            {
                foreach (var ownerReference in end.Resource.Metadata.OwnerReferences)
                {
                    var start = Drawing.Nodes.OfType<ResourceNodeViewModel>().FirstOrDefault(x => x.Resource.Metadata.Uid == ownerReference.Uid);

                    if (start != null)
                    {
                        var connector = new MyConnectorViewModel
                        {
                            Start = start.Pins[1],
                            End = end.Pins[0],
                            Parent = Drawing
                        };

                        Drawing.Connectors.Add(connector);
                    }
                }
            }
        }
    }

    private void LinkIngress()
    {
        foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (start.Resource is V1Ingress ingress)
            {
                if (ingress.Spec.Rules != null)
                {
                    foreach (var serviceBackend in ingress.Spec.Rules
                                                    .SelectMany(x => x.Http.Paths.Where(y => y.Backend != null && y.Backend.Service != null)
                                                    .Select(y => y.Backend.Service)))
                    {
                        foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Metadata.Name == serviceBackend.Name && service.Metadata.NamespaceProperty == ingress.Metadata.NamespaceProperty)
                                {
                                    var connector = new MyConnectorViewModel
                                    {
                                        Start = start.Pins[1],
                                        End = end.Pins[0],
                                        Parent = Drawing
                                    };

                                    Drawing.Connectors.Add(connector);
                                }
                            }
                        }
                    }
                }

                if (ingress.Spec.DefaultBackend != null)
                {
                    if (ingress.Spec.DefaultBackend.Service != null)
                    {
                        foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Metadata.Name == ingress.Spec.DefaultBackend.Service.Name && service.Metadata.NamespaceProperty == ingress.Metadata.NamespaceProperty)
                                {
                                    var connector = new MyConnectorViewModel
                                    {
                                        Start = start.Pins[1],
                                        End = end.Pins[0],
                                        Parent = Drawing
                                    };

                                    Drawing.Connectors.Add(connector);
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
        foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
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

                        foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                        {
                            if (end.Resource is V1Pod pod)
                            {
                                if (pod.Metadata.Uid == address.TargetRef.Uid)
                                {
                                    var connector = new MyConnectorViewModel
                                    {
                                        Start = start.Pins[1],
                                        End = end.Pins[0],
                                        Parent = Drawing
                                    };

                                    Drawing.Connectors.Add(connector);
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
        foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
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

                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                    {
                        if (end.Resource is V1Pod pod)
                        {
                            if (pod.Metadata.Uid == endpoint.TargetRef.Uid)
                            {
                                var connector = new MyConnectorViewModel
                                {
                                    Start = start.Pins[1],
                                    End = end.Pins[0],
                                    Parent = Drawing
                                };

                                Drawing.Connectors.Add(connector);
                            }
                        }
                    }
                }
            }
        }
    }

    private void LinkConfigMap()
    {
        foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Metadata.Name == envFrom.ConfigMapRef.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Metadata.Name == volume.ConfigMap.Name && configMap.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
        foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = end.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1Secret secret)
                                {
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1Secret secret)
                                {
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1Secret secret)
                                {
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == env.ValueFrom.SecretKeyRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                                    {
                                        if (destination.Resource is V1Secret secret)
                                        {
                                            if (secret.Metadata.Name == envFrom.SecretRef.Name && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                            {
                                                var connector = new MyConnectorViewModel
                                                {
                                                    Start = start.Pins[1],
                                                    End = destination.Pins[0],
                                                    Parent = Drawing
                                                };

                                                Drawing.Connectors.Add(connector);
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
                            foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                            {
                                if (destination.Resource is V1Secret secret)
                                {
                                    if (secret.Metadata.Name == volume.Secret.SecretName && secret.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                                    {
                                        var connector = new MyConnectorViewModel
                                        {
                                            Start = start.Pins[1],
                                            End = destination.Pins[0],
                                            Parent = Drawing
                                        };

                                        Drawing.Connectors.Add(connector);
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
                        foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                        {
                            if (end.Resource is V1Secret secret)
                            {
                                if (secret.Metadata.Uid == secretReference.Uid && end.Resource.Metadata.NamespaceProperty == serviceAccount.Metadata.NamespaceProperty)
                                {
                                    var connector = new MyConnectorViewModel
                                    {
                                        Start = start.Pins[1],
                                        End = end.Pins[0],
                                        Parent = Drawing
                                    };

                                    Drawing.Connectors.Add(connector);
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
        foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (end.Resource is Corev1Event @event)
            {
                foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                {
                    if (start.Resource.Metadata.Uid == @event.InvolvedObject.Uid && start.Resource.Metadata.NamespaceProperty == @event.Metadata.NamespaceProperty)
                    {
                        var connector = new MyConnectorViewModel
                        {
                            Start = start.Pins[1],
                            End = end.Pins[0],
                            Parent = Drawing
                        };

                        Drawing.Connectors.Add(connector);
                    }
                }
            }
        }
    }

    private void LinkServiceAccount()
    {
        foreach (var start in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (start.Resource is V1Deployment deployment)
            {
                foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Metadata.Name == deployment.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == deployment.Metadata.NamespaceProperty)
                        {
                            var connector = new MyConnectorViewModel
                            {
                                Start = start.Pins[1],
                                End = end.Pins[0],
                                Parent = Drawing
                            };

                            Drawing.Connectors.Add(connector);
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Metadata.Name == replicaSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == replicaSet.Metadata.NamespaceProperty)
                        {
                            var connector = new MyConnectorViewModel
                            {
                                Start = start.Pins[1],
                                End = end.Pins[0],
                                Parent = Drawing
                            };

                            Drawing.Connectors.Add(connector);
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Metadata.Name == daemonSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == daemonSet.Metadata.NamespaceProperty)
                        {
                            var connector = new MyConnectorViewModel
                            {
                                Start = start.Pins[1],
                                End = end.Pins[0],
                                Parent = Drawing
                            };

                            Drawing.Connectors.Add(connector);
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                foreach (var end in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Metadata.Name == statefulSet.Spec.Template.Spec.ServiceAccountName && end.Resource.Metadata.NamespaceProperty == statefulSet.Metadata.NamespaceProperty)
                        {
                            var connector = new MyConnectorViewModel
                            {
                                Start = start.Pins[1],
                                End = end.Pins[0],
                                Parent = Drawing
                            };

                            Drawing.Connectors.Add(connector);
                        }
                    }
                }
            }
        }
    }

    #endregion

    private INode CreateResource(IKubernetesObject<V1ObjectMeta> resource, GroupApiVersionKind kind, double pinSize = 8)
    {
        var vm = Application.Current.GetRequiredService<VisualizationResourceViewModel>();

        vm.Initialize(_cluster, resource);

        var view = Application.Current.GetRequiredService<VisualizationResourceView>();
        view.DataContext = vm;

        var node = new ResourceNodeViewModel
        {
            Width = _resourceSize,
            Height = _resourceSize,
            Resource = resource,
            Kind = kind,
            Content = view,
            Parent = Drawing
        };

        node.Pins.Add(new MyPin()
        {
            X = 0,
            Y = _resourceSize / 2,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Left,
            Name = "L",
            Parent = node
        });

        node.Pins.Add(new MyPin()
        {
            X = _resourceSize,
            Y = _resourceSize / 2,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Right,
            Name = "R",
            Parent = node
        });

        node.Pins.Add(new MyPin()
        {
            X = _resourceSize / 2,
            Y = 0,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Top,
            Name = "T",
            Parent = node
        });

        node.Pins.Add(new MyPin()
        {
            X = _resourceSize / 2,
            Y = _resourceSize,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Bottom,
            Name = "B",
            Parent = node
        });

        return node;
    }

    public void Dispose()
    {
        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    public sealed partial class ResourceNodeViewModel : NodeViewModel, INode
    {
        [ObservableProperty]
        private IKubernetesObject<V1ObjectMeta> _resource;

        [ObservableProperty]
        private GroupApiVersionKind _kind;

        public ResourceNodeViewModel()
        {
            Pins = new ObservableCollection<IPin>();
        }

        public new bool CanSelect()
        {
            return true;
        }

        public new bool CanRemove()
        {
            return false;
        }

        public new bool CanMove()
        {
            return true;
        }

        public new bool CanResize()
        {
            return false;
        }
    }

    public sealed class MyPin: PinViewModel, IPin
    {
        public new bool CanConnect()
        {
            return false;
        }

        public new bool CanDisconnect()
        {
            return false;
        }

        public new void OnSelected()
        {

        }
    }

    public sealed class MyConnectorViewModel : ConnectorViewModel, IConnector
    {
        public new bool CanSelect()
        {
            return false;
        }

        public new bool CanRemove()
        {
            return false;
        }
    }
}
