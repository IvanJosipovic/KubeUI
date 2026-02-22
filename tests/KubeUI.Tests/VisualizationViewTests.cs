using Avalonia;
using Avalonia.Headless.XUnit;
using Shouldly;
using k8s.Models;
using KubeUI.Tests.Infra;
using KubeUI.ViewModels;
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
    public async Task LinkSecretInServiceAccount()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    #endregion

    [AvaloniaFact]
    public async Task LinkEvent()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default"
            },
            InvolvedObject = new()
            {
                Uid = "dep-uid"
            }
        });

        await cluster.AddOrUpdateResource(new V1Deployment
        {
            Metadata = new()
            {
                Name = "end",
                NamespaceProperty = "default",
                Uid = "dep-uid"
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();

        vm.HideNoise = false;
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<Corev1Event>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
    }

    [AvaloniaFact]
    public async Task LinkPodToEndpointSlice()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1EndpointSlice>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public async Task LinkPodToEndpointEndpoints()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Endpoints>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public async Task LinkIngressToService()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Ingress>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Service>();
    }

    [AvaloniaFact]
    public async Task LinkIngressDefaultBackend()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Ingress>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Service>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimToPersistantVolume()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1PersistentVolumeClaim
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default",
            },
            Spec = new()
            {
                VolumeName = "end"
            }
        });

        await cluster.AddOrUpdateResource(new V1PersistentVolume
        {
            Metadata = new()
            {
                Name = "end"
            },
            Spec = new()
            {
                HostPath = new()
                {
                    Path = "/test"
                }
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolume>();
    }

    [AvaloniaFact]
    public async Task LinkRoleBindingToServiceAccount()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1RoleBinding 
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default",
            },
            Subjects =
            [
                new()
                {
                    ApiGroup = "",
                    Kind = V1ServiceAccount.KubeKind, // ServiceAccount, todo User and Group
                    Name = "end",
                    NamespaceProperty = "default"
                }
            ]
        });

        await cluster.AddOrUpdateResource(new V1ServiceAccount
        {
            Metadata = new()
            {
                Name = "end",
                NamespaceProperty = "default",
            },
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkRoleBindingToRole()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1RoleBinding
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default"
            },
            RoleRef = new()
            {
                ApiGroup = V1Role.KubeGroup,
                Kind = V1Role.KubeKind,
                Name = "end"
            }
        });

        await cluster.AddOrUpdateResource(new V1Role
        {
            Metadata = new()
            {
                Name = "end",
                NamespaceProperty = "default"
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Role>();
    }

    [AvaloniaFact]
    public async Task LinkRoleBindingClusterRole()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1RoleBinding
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default"
            },
            RoleRef = new()
            {
                ApiGroup = V1ClusterRole.KubeGroup,
                Kind = V1ClusterRole.KubeKind,
                Name = "end"
            }
        });

        await cluster.AddOrUpdateResource(new V1ClusterRole
        {
            Metadata = new()
            {
                Name = "end"
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRole>();
    }

    [AvaloniaFact]
    public async Task LinkClusterRoleBindingToServiceAccount()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1ClusterRoleBinding
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default",
            },
            Subjects =
            [
                new()
                {
                    ApiGroup = "",
                    Kind = V1ServiceAccount.KubeKind, // ServiceAccount, todo User and Group
                    Name = "end",
                    NamespaceProperty = "default"
                }
            ]
        });

        await cluster.AddOrUpdateResource(new V1ServiceAccount
        {
            Metadata = new()
            {
                Name = "end",
                NamespaceProperty = "default",
            },
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkClusterRoleBindingClusterRole()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1ClusterRoleBinding
        {
            Metadata = new()
            {
                Name = "start",
                NamespaceProperty = "default"
            },
            RoleRef = new()
            {
                ApiGroup = V1ClusterRole.KubeGroup,
                Kind = V1ClusterRole.KubeKind,
                Name = "end"
            }
        });

        await cluster.AddOrUpdateResource(new V1ClusterRole
        {
            Metadata = new()
            {
                Name = "end"
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRole>();
    }

    #region ServiceAccount

    [AvaloniaFact]
    public async Task LinkServiceAccountInPod()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInDeployment()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInStatefulSet()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInDaemonSet()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInReplicaSet()
    {
        var cluster = TestCluster.Get();

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

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    #endregion

    #region PersistentVolumeClaim

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInPod()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1PersistentVolumeClaim
        {
            Metadata = new()
            {
                Name = "pvc",
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
                Volumes = [
                    new()
                    {
                        Name = "vol",
                        PersistentVolumeClaim = new()
                        {
                            ClaimName = "pvc"
                        }
                    }
                ]
            }
        });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInDeployment()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1PersistentVolumeClaim
        {
            Metadata = new()
            {
                Name = "pvc",
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
                        Volumes = [
                            new()
                            {
                                Name = "vol",
                                PersistentVolumeClaim = new()
                                {
                                    ClaimName = "pvc"
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
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInStatefulSet()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1PersistentVolumeClaim
        {
            Metadata = new()
            {
                Name = "pvc",
                NamespaceProperty = "default"
            }
        });

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
                        Volumes = [
                            new()
                            {
                                Name = "vol",
                                PersistentVolumeClaim = new()
                                {
                                    ClaimName = "pvc"
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
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInDaemonSet()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1PersistentVolumeClaim
        {
            Metadata = new()
            {
                Name = "pvc",
                NamespaceProperty = "default"
            }
        });

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
                        Volumes = [
                            new()
                            {
                                Name = "vol",
                                PersistentVolumeClaim = new()
                                {
                                    ClaimName = "pvc"
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
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInReplicaSet()
    {
        var cluster = TestCluster.Get();

        await cluster.AddOrUpdateResource(new V1PersistentVolumeClaim
        {
            Metadata = new()
            {
                Name = "pvc",
                NamespaceProperty = "default"
            }
        });

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
                        Volumes = [
                            new()
                            {
                                Name = "vol",
                                PersistentVolumeClaim = new()
                                {
                                    ClaimName = "pvc"
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
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    #endregion
}
