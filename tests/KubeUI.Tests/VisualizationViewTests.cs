using Avalonia;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Tests.Infra;
using KubeUI.ViewModels;
using Shouldly;
using static KubeUI.ViewModels.VisualizationViewModel;

namespace KubeUI.Tests;

public class VisualizationViewTests
{
    [AvaloniaFact]
    public async Task LinkOwners()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    #region ConfigMap

    [AvaloniaFact]
    public async Task LinkConfigMapInPodEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    //todo add test for hidenoise and ReplicaSet = 1/0

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    #endregion

    #region Secret

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetInitEnv()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetInitEnvFrom()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetVolume()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkArgoTrackingAnnotationt()
    {
        var cluster = TestCluster.Get();

        var namespaceResource = new V1Namespace
        {
            Metadata = new()
            {
                Name = "oidc-guard"
            }
        };

        await cluster.AddOrUpdateResource(namespaceResource);
        cluster.SelectedNamespaces.Add(namespaceResource);

        var argoNamespace = new V1Namespace
        {
            Metadata = new()
            {
                Name = "argocd"
            }
        };

        await cluster.AddOrUpdateResource(argoNamespace);
        cluster.SelectedNamespaces.Add(argoNamespace);

        await cluster.AddOrUpdateResource(new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = "applications.argoproj.io"
            },
            Spec = new()
            {
                Group = "argoproj.io",
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = "applications",
                    Singular = "application",
                    Kind = "Application",
                    ShortNames = ["app", "apps"]
                },
                Versions =
                [
                    new()
                    {
                        Name = "v1alpha1",
                        Served = true,
                        Storage = true,
                        Schema = new()
                        {
                            OpenAPIV3Schema = new()
                            {
                                Type = "object"
                            }
                        }
                    }
                ]
            }
        });

        await cluster.AddOrUpdateResource(new ArgoApplication
        {
            ApiVersion = "argoproj.io/v1alpha1",
            Kind = "Application",
            Metadata = new()
            {
                Name = "my-app",
                NamespaceProperty = "argocd"
            }
        });

        await cluster.AddOrUpdateResource(new V1Secret
        {
            Metadata = new()
            {
                Name = "oidc-guard",
                NamespaceProperty = "oidc-guard",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "my-app:/Secret:oidc-guard/oidc-guard"
                }
            },
            StringData = new Dictionary<string, string>
            {
                ["appsettings.Production.json"] = "test"
            },
            Type = "Opaque"
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<ArgoApplication>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    #endregion
}

[KubernetesEntity(Group = "argoproj.io", ApiVersion = "v1alpha1", Kind = "Application", PluralName = "applications")]
public sealed class ArgoApplication : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "argoproj.io/v1alpha1";

    public string Kind { get; set; } = "Application";

    public V1ObjectMeta Metadata { get; set; } = new();
}
