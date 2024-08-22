using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Headless.XUnit;
using FluentAssertions;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.ViewModels;
using Moq;
using Swordfish.NET.Collections;
using static KubeUI.ViewModels.VisualizationViewModel;

namespace KubeUI.Tests;

public class VisualizatonTests
{
    private (Mock<ICluster>, ConcurrentDictionary<GroupApiVersionKind, ContainerClass>) GetMock()
    {
        var resources = new ConcurrentDictionary<GroupApiVersionKind, ContainerClass>();

        var mock = new Mock<ICluster>();
        mock.Setup(p => p.Objects).Returns(resources);
        mock.Setup(p => p.SelectedNamespaces).Returns(
        [
            new V1Namespace()
            {
                Metadata = new()
                {
                    Name = "default"
                }
            }
        ]);

        return (mock, resources);
    }

    [AvaloniaFact]
    public void LinkOwnersTest()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default",
                        OwnerReferences = [
                            new()
                            {
                                Uid = "test123"
                            }
                        ]
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var deployments = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
        {
            {
                new("default", "my-deployment"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-deployment",
                        NamespaceProperty = "default",
                        Uid = "test123"
                    },
                    Spec = new()
                    {
                        Template = new()
                        {
                            Spec = new()
                            {
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = deployments });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    #region ConfigMap

    [AvaloniaFact]
    public void LinkConfigMapInDeploymentEnv()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var deployments = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
                                    new()
                                    {
                                        Env = [
                                            new()
                                            {
                                                Name = "MY_CONFIG",
                                                ValueFrom = new()
                                                {
                                                    ConfigMapKeyRef = new()
                                                    {
                                                        Key = "config.json",
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = deployments });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDeploymentEnvFrom()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var deployments = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = deployments });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDeploymentVolume()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var deployments = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Volumes =
                                [
                                    new()
                                    {
                                        ConfigMap = new()
                                        {
                                            Name = "my-config"
                                        },
                                        Name = "config-volume"
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() {  Items = deployments });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }


    [AvaloniaFact]
    public void LinkConfigMapInDaemonSetEnv()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var daemonsets = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
                                    new()
                                    {
                                        Env = [
                                            new()
                                            {
                                                Name = "MY_CONFIG",
                                                ValueFrom = new()
                                                {
                                                    ConfigMapKeyRef = new()
                                                    {
                                                        Key = "config.json",
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = daemonsets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDaemonSetEnvFrom()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var daemonsets = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = daemonsets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDaemonSetVolume()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var daemonsets = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Volumes =
                                [
                                    new()
                                    {
                                        ConfigMap = new()
                                        {
                                            Name = "my-config"
                                        },
                                        Name = "config-volume"
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = daemonsets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }


    [AvaloniaFact]
    public void LinkConfigMapInStatefulSetEnv()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var statefulsets = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
                                    new()
                                    {
                                        Env = [
                                            new()
                                            {
                                                Name = "MY_CONFIG",
                                                ValueFrom = new()
                                                {
                                                    ConfigMapKeyRef = new()
                                                    {
                                                        Key = "config.json",
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = statefulsets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInStatefulSetEnvFrom()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var statefulsets = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = statefulsets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInStatefulSetVolume()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var statefulsets = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Volumes =
                                [
                                    new()
                                    {
                                        ConfigMap = new()
                                        {
                                            Name = "my-config"
                                        },
                                        Name = "config-volume"
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = statefulsets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    //todo add test for hidenoise and ReplicaSet = 1/0

    [AvaloniaFact]
    public void LinkConfigMapInReplicaSetEnv()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var replicasets = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
                                    new()
                                    {
                                        Env = [
                                            new()
                                            {
                                                Name = "MY_CONFIG",
                                                ValueFrom = new()
                                                {
                                                    ConfigMapKeyRef = new()
                                                    {
                                                        Key = "config.json",
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
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = replicasets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInReplicaSetEnvFrom()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var replicasets = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Containers = [
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
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = replicasets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInReplicaSetVolume()
    {
        var (mock, resources) = GetMock();

        var configMaps = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "config.json", "{ \"key\": \"value\" }" }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = configMaps });

        var replicasets = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
        {
            {
                new("default", "my-deployment"),
                new()
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
                                Volumes =
                                [
                                    new()
                                    {
                                        ConfigMap = new()
                                        {
                                            Name = "my-config"
                                        },
                                        Name = "config-volume"
                                    }
                                ]
                            }
                        }
                    },
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = replicasets });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    #endregion
}
