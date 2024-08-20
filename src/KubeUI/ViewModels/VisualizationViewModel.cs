using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Views;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using Cluster = KubeUI.Client.Cluster;

namespace KubeUI.ViewModels;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster
{
    private Cluster? _cluster;

    [ObservableProperty]
    private DrawingNodeViewModel _drawing = CreateDrawing();

    [ObservableProperty]
    private string _selectedNamespace = "sonarr";

    public VisualizationViewModel()
    {
        Title = Resources.VisualizationViewModel_Title;
    }

    public void Initialize(Cluster cluster)
    {
        _cluster = cluster;

        Id = nameof(VisualizationViewModel) + "-" + cluster;

        cluster.Seed<V1Node>();

        cluster.Seed<V1Pod>();
        cluster.Seed<V1ReplicaSet>();
        cluster.Seed<V1Deployment>();
        cluster.Seed<V1Endpoints>();

        cluster.Seed<V1Service>();
        cluster.Seed<V1EndpointSlice>();
        cluster.Seed<V1Ingress>();

        cluster.Seed<V1PersistentVolume>();
        cluster.Seed<V1Secret>();
        cluster.Seed<V1ConfigMap>();

        Run();
    }

    public void Run()
    {
        PopulateAllResources();

        LinkOwners();
        //LinkServices();
        LinkIngress();
        LinkEndpoints();
        LinkEndpointSlice();

        OrderResources();
    }

    private void PopulateAllResources()
    {
        var size = 60;

        foreach (var kind in _cluster.Objects)
        {
            foreach (object item in kind.Value.Items)
            {
                var key = (NamespacedName)item.GetType().GetProperty("Key").GetValue(item);
                var value = (IKubernetesObject<V1ObjectMeta>)item.GetType().GetProperty("Value").GetValue(item);

                if (!string.IsNullOrEmpty(SelectedNamespace) && key.Namespace != SelectedNamespace)
                {
                    continue;
                }

                var rectangle0 = CreateResource(size, size, value, kind.Key);
                rectangle0.Parent = Drawing;
                Drawing.Nodes.Add(rectangle0);
            }
        }
    }

    private void OrderResources()
    {
        var size = 60;

        var items = Drawing.Nodes.OfType<ResourceNodeViewModel>();

        var x = 0;
        var y = 0;

        foreach (var group in items.GroupBy(x => x.Kind))
        {
            foreach (var item in group)
            {
                item.X = (size + (size / 2)) * x;
                item.Y = (size + (size / 2)) * y;
                y++;
            }
            y = 0;
            x++;
        }
    }

    private void LinkOwners()
    {
        foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (destination.Resource.Metadata.OwnerReferences != null)
            {
                foreach (var ownerReference in destination.Resource.Metadata.OwnerReferences)
                {
                    var ownerObject = Drawing.Nodes.OfType<ResourceNodeViewModel>().FirstOrDefault(x => x.Resource.Metadata.Uid == ownerReference.Uid);

                    if (ownerObject != null)
                    {
                        var connector = new ConnectorViewModel
                        {
                            Start = ownerObject.Pins[1],
                            End = destination.Pins[0],
                            Parent = Drawing
                        };

                        Drawing.Connectors.Add(connector);
                    }
                }
            }
        }
    }

    [Obsolete("Not needed")]
    private void LinkServices()
    {
        foreach (var source in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (source.Resource is V1Service service)
            {
                if (service.Spec.Selector == null)
                {
                    continue;
                }

                foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                {
                    if (destination.Resource is V1Pod pod)
                    {
                        if (service.Spec.Selector.All(x => pod.Metadata.Labels.TryGetValue(x.Key, out var val) && val == x.Value))
                        {
                            var connector = new ConnectorViewModel
                            {
                                Start = source.Pins[1],
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

    private void LinkIngress()
    {
        foreach (var source in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (source.Resource is V1Ingress ingress)
            {
                foreach (var serviceBackend in ingress.Spec.Rules.SelectMany(x => x.Http.Paths.Select(y => y.Backend.Service)))
                {
                    foreach (var destination2 in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                    {
                        if (destination2.Resource is V1Service service)
                        {
                            if (service.Metadata.Name == serviceBackend.Name && service.Metadata.NamespaceProperty == ingress.Metadata.NamespaceProperty)
                            {
                                var connector = new ConnectorViewModel
                                {
                                    Start = source.Pins[1],
                                    End = destination2.Pins[0],
                                    Parent = Drawing
                                };

                                Drawing.Connectors.Add(connector);
                            }
                        }
                    }
                }

                //todo Backend.Resource
            }
        }
    }

    private void LinkEndpoints()
    {
        foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (destination.Resource is V1Endpoints endpoint)
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

                        foreach (var source in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                        {
                            if (source.Resource is V1Pod pod)
                            {
                                if (pod.Metadata.Uid == address.TargetRef.Uid)
                                {
                                    var connector = new ConnectorViewModel
                                    {
                                        Start = source.Pins[1],
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

    private void LinkEndpointSlice()
    {
        foreach (var source in Drawing.Nodes.OfType<ResourceNodeViewModel>())
        {
            if (source.Resource is V1EndpointSlice endpointSlice)
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

                    foreach (var destination in Drawing.Nodes.OfType<ResourceNodeViewModel>())
                    {
                        if (destination.Resource is V1Pod pod)
                        {
                            if (pod.Metadata.Uid == endpoint.TargetRef.Uid)
                            {
                                var connector = new ConnectorViewModel
                                {
                                    Start = source.Pins[1],
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

    public static DrawingNodeViewModel CreateDrawing(string? name = null)
    {
        return new DrawingNodeViewModel
        {
            Name = name,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
            EnableMultiplePinConnections = true,
            EnableSnap = true,
            SnapX = 10.0,
            SnapY = 10.0,
            EnableGrid = false,
            GridCellWidth = 10.0,
            GridCellHeight = 10.0,
        };
    }

    private INode CreateResource(double width, double height, IKubernetesObject<V1ObjectMeta> resource, GroupApiVersionKind kind, double pinSize = 8)
    {
        var vm = Application.Current.GetRequiredService<VisualizationResourceViewModel>();

        vm.Initialize(_cluster, resource);

        var view = Application.Current.GetRequiredService<VisualizationResourceView>();
        view.DataContext = vm;

        var node = new ResourceNodeViewModel
        {
            Width = width,
            Height = height,
            Resource = resource,
            Kind = kind,
            Content = view,
        };

        node.Pins.Add(new ReadOnlyPin()
        {
            X = 0,
            Y = height / 2,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Left,
            Name = "L",
            Parent = node
        });

        node.Pins.Add(new ReadOnlyPin()
        {
            X = width,
            Y = height / 2,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Right,
            Name = "R",
            Parent = node
        });

        node.Pins.Add(new ReadOnlyPin()
        {
            X = width /2,
            Y = 0,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Top,
            Name = "T",
            Parent = node
        });

        node.Pins.Add(new ReadOnlyPin()
        {
            X = width / 2,
            Y = height,
            Width = pinSize,
            Height = pinSize,
            Alignment = PinAlignment.Bottom,
            Name = "B",
            Parent = node
        });

        return node;
    }

    public partial class ResourceNodeViewModel : NodeViewModel, INode
    {
        [ObservableProperty]
        private IKubernetesObject<V1ObjectMeta> _resource;

        [ObservableProperty]
        private GroupApiVersionKind _kind;

        public ResourceNodeViewModel()
        {
            Pins = new ObservableCollection<IPin>();
        }

        public new virtual bool CanSelect()
        {
            return true;
        }

        public new virtual bool CanRemove()
        {
            return false;
        }

        public new virtual bool CanMove()
        {
            return true;
        }

        public new virtual bool CanResize()
        {
            return false;
        }
    }

    public class ReadOnlyPin: PinViewModel, IPin
    {
        public new virtual bool CanConnect()
        {
            return false;
        }

        public new virtual bool CanDisconnect()
        {
            return false;
        }
    }
}
