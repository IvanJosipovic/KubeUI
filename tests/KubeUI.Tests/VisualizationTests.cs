using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using DynamicData;
using FluentAssertions;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using KubeUI.ViewModels;
using Yarp.Kubernetes.Controller;
using Yarp.Kubernetes.Controller.Client;
using static KubeUI.ViewModels.VisualizationViewModel;

namespace KubeUI.Tests;

public class TestCluster : ICluster
{
    public AvaloniaDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsMetricsAvailable => throw new NotImplementedException();

    public bool ListNamespaces { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ClusterStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IKubernetes? Client { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IReadOnlyList<V1Namespace> Namespaces { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public K8SConfiguration KubeConfig { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ModelCache ModelCache { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<NavigationItem> NavigationItems { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<NodeMetrics> NodeMetrics { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<PodMetrics> PodMetrics { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<PortForwarder> PortForwarders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<V1Namespace> SelectedNamespaces { get; } = [];
    public string KubeConfigPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        throw new NotImplementedException();
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort)
    {
        throw new NotImplementedException();
    }

    public bool CanI(Type type, Client.Cluster.Verb verb, string? @namespace = null, string? subresource = null)
    {
        throw new NotImplementedException();
    }

    public bool CanIAnyNamespace(Type type, Client.Cluster.Verb verb, string? subresource = null)
    {
        throw new NotImplementedException();
    }

    public Task Connect()
    {
        throw new NotImplementedException();
    }

    public IResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        throw new NotImplementedException();
    }

    public IObservable<int> GetResourceCount(Type type)
    {
        throw new NotImplementedException();
    }

    public Task ImportFolder(string path)
    {
        throw new NotImplementedException();
    }

    public Task ImportYaml(Stream stream)
    {
        throw new NotImplementedException();
    }

    public bool IsResourceNamespaced(Type type)
    {
        throw new NotImplementedException();
    }

    public bool IsResourceNamespaced<T>()
    {
        throw new NotImplementedException();
    }

    public void RemovePortForward(PortForwarder pf)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateCanI(Type type, Client.Cluster.Verb verb, string? @namespace = null, string? subresource = null)
    {
        throw new NotImplementedException();
    }

    public async Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await SeedResource<T>();

        var kind = GroupApiVersionKind.From<T>();

        var container = (ContainerClass<T>)Objects[kind];

        container.Items.AddOrUpdate(item);
    }

    public bool CanI<T>(Client.Cluster.Verb verb, string? @namespace, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public bool CanIAnyNamespace<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsResourceReady<T>(CancellationToken? token) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task SeedResource<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(kind, out _))
        {
            var container = new ContainerClass<T>();

            if (!Objects.TryAdd(kind, container))
            {
                throw new Exception($"Duplicate Object Set detected for: {kind}");
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateCanI<T>(Client.Cluster.Verb verb, string? @namespace, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task UpdatePermissionsAllNamespaceAsync<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }
}


public class VisualizationTests
{
    private ICluster GetTestCluster()
    {
        var cluster = new TestCluster();

        var ns = new V1Namespace()
        {
            Metadata = new() { Name = "default" }
        };

        cluster.AddOrUpdateResource(ns).GetAwaiter().GetResult();

        cluster.SelectedNamespaces.Add(ns);

        return cluster;
    }

    [AvaloniaFact]
    public async Task LinkOwners()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-deployment",
                NamespaceProperty = "default",
                Uid = "owner-uid"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                }
            }
        });

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "my-config",
                NamespaceProperty = "default",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "owner-uid"
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        // Simulate selection
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    #region ConfigMap

    [AvaloniaFact]
    public async Task LinkConfigMapInPodEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "my-config",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Containers =
                [
                    new()
                    {
                        Env =
                        [
                            new()
                            {
                                ValueFrom = new()
                                {
                                    ConfigMapKeyRef = new()
                                    {
                                        Name = "my-config"
                                    }
                                }
                            }
                        ]
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();

        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "my-config",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                InitContainers =
                [
                    new()
                    {
                        Env =
                        [
                            new()
                            {
                                ValueFrom = new()
                                {
                                    ConfigMapKeyRef = new()
                                    {
                                        Name = "my-config"
                                    }
                                }
                            }
                        ]
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "my-config",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Containers =
                [
                    new()
                    {
                        EnvFrom =
                        [
                            new()
                            {
                                ConfigMapRef = new()
                                {
                                    Name = "my-config"
                                }
                            }
                        ]
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "my-config",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                InitContainers =
                [
                    new()
                    {
                        EnvFrom =
                        [
                            new()
                            {
                                ConfigMapRef = new()
                                {
                                    Name = "my-config"
                                }
                            }
                        ]
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "my-config",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Volumes =
                [
                    new()
                    {
                        ConfigMap = new()
                        {
                            Name = "my-config"
                        }
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new() { Name = "my-config", NamespaceProperty = "default" }
        });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                ConfigMap = new()
                                {
                                    Name = "my-config"
                                }
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                ConfigMap = new()
                                {
                                    Name = "my-config"
                                }
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                ConfigMap = new()
                                {
                                    Name = "my-config"
                                }
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    //todo add test for hidenoise and ReplicaSet = 1/0

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            ConfigMapKeyRef = new()
                                            {
                                                Name = "my-config"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        ConfigMapRef = new()
                                        {
                                            Name = "my-config"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ConfigMap { Metadata = new() { Name = "my-config", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                ConfigMap = new()
                                {
                                    Name = "my-config"
                                }
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    #endregion

    #region Secret

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new() { Name = "my-deployment", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                Secret = new()
                                {
                                    SecretName = "my-secret"
                                }
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new() { Name = "my-ds", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                Secret = new()
                                {
                                    SecretName = "my-secret"
                                }
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new() { Name = "my-sts", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                Secret = new()
                                {
                                    SecretName = "my-secret"
                                }
                            }
                        ]
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetInitEnv()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "my-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetInitEnvFrom()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        InitContainers =
                        [
                            new()
                            {
                                EnvFrom =
                                [
                                    new()
                                    {
                                        SecretRef = new()
                                        {
                                            Name = "my-secret"
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetVolume()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret { Metadata = new() { Name = "my-secret", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new() { Name = "my-rs", NamespaceProperty = "default" },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Volumes =
                        [
                            new()
                            {
                                Secret = new()
                                {
                                    SecretName = "my-secret"
                                }
                            }
                        ]
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInServiceAccount()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Secret
        {
            Metadata = new()
            {
                Name = "my-secret",
                NamespaceProperty = "default",
                Uid = "sec-uid"
            }
        });

        await cluster.AddOrUpdateResource(new V1ServiceAccount
        {
            Metadata = new()
            {
                Name = "my-sa",
                NamespaceProperty = "default"
            },
            Secrets =
            [
                new()
                {
                    Uid = "sec-uid"
                }
            ]
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    #endregion

    [AvaloniaFact]
    public async Task LinkEvent()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-deployment",
                NamespaceProperty = "default",
                Uid = "dep-uid"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                }
            }
        });

        await cluster.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "my-event",
                NamespaceProperty = "default"
            },
            InvolvedObject = new()
            {
                Uid = "dep-uid"
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<Corev1Event>();
    }

    [AvaloniaFact]
    public async Task LinkEndpointSlice()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default",
                Uid = "pod-uid"
            },
            Spec = new()
        });

        await cluster.AddOrUpdateResource(new V1EndpointSlice
        {
            Metadata = new()
            {
                Name = "my-es",
                NamespaceProperty = "default"
            },
            Endpoints =
            [
                new()
                {
                    TargetRef = new()
                    {
                        Uid = "pod-uid"
                    }
                }
            ]
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1EndpointSlice>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public async Task LinkEndpoints()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default",
                Uid = "pod-uid"
            },
            Spec = new()
        });

        await cluster.AddOrUpdateResource(new V1Endpoints
        {
            Metadata = new()
            {
                Name = "my-endpoints",
                NamespaceProperty = "default"
            },
            Subsets =
            [
                new()
                {
                    Addresses =
                    [
                        new()
                        {
                            TargetRef = new()
                            {
                                Uid = "pod-uid"
                            }
                        }
                    ]
                }
            ]
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Endpoints>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public async Task LinkIngress()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Service
        {
            Metadata = new()
            {
                Name = "my-service",
                NamespaceProperty = "default"
            },
            Spec = new()
        });

        await cluster.AddOrUpdateResource(new V1Ingress
        {
            Metadata = new()
            {
                Name = "my-ingress",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Rules =
                [
                    new()
                    {
                        Http = new()
                        {
                            Paths =
                            [
                                new()
                                {
                                    Backend = new()
                                    {
                                        Service = new()
                                        {
                                            Name = "my-service"
                                        }
                                    }
                                }
                            ]
                        }
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Ingress>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Service>();
    }

    [AvaloniaFact]
    public async Task LinkIngressDefaultBackend()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1Service
        {
            Metadata = new()
            {
                Name = "my-service",
                NamespaceProperty = "default"
            },
            Spec = new()
        });

        await cluster.AddOrUpdateResource(new V1Ingress
        {
            Metadata = new()
            {
                Name = "my-ingress",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                DefaultBackend = new()
                {
                    Service = new()
                    {
                        Name = "my-service"
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Ingress>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Service>();
    }

    #region ServiceAccount

    [AvaloniaFact]
    public async Task LinkServiceAccountInPod()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ServiceAccount
        {
            Metadata = new()
            {
                Name = "my-sa",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Containers = [],
                ServiceAccountName = "my-sa"
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInDeployment()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ServiceAccount
        {
            Metadata = new()
            {
                Name = "my-sa",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-deployment",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers = [],
                        ServiceAccountName = "my-sa"
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInStatefulSet()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ServiceAccount { Metadata = new() { Name = "my-sa", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1StatefulSet
        {
            Metadata = new()
            {
                Name = "my-sts",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers = [],
                        ServiceAccountName = "my-sa"
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInDaemonSet()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ServiceAccount { Metadata = new() { Name = "my-sa", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1DaemonSet
        {
            Metadata = new()
            {
                Name = "my-ds",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers = [],
                        ServiceAccountName = "my-sa"
                    }
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInReplicaSet()
    {
        var cluster = GetTestCluster();

        await cluster.AddOrUpdateResource(new V1ServiceAccount { Metadata = new() { Name = "my-sa", NamespaceProperty = "default" } });

        await cluster.AddOrUpdateResource(new V1ReplicaSet
        {
            Metadata = new()
            {
                Name = "my-rs",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers = [],
                        ServiceAccountName = "my-sa"
                    }
                }
            },
            Status = new() { Replicas = 1 }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.Should().Be(1);
        vm.Graph.Edges.First().Tail.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    #endregion
}
