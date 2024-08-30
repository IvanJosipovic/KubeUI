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

public class VisualizationTests
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
    public void LinkOwners()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default",
                        OwnerReferences =
                        [
                            new()
                            {
                                Uid = "test123"
                            }
                        ]
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDeploymentInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDeploymentInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                                        }
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() {  Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDaemonSetInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInDaemonSetInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                                        }
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInStatefulSetInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInStatefulSetInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                                        }
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInReplicaSetInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public void LinkConfigMapInReplicaSetInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                    Status = new()
                    {
                        Replicas = 1
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

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

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ConfigMap>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                                        }
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ConfigMap>();
    }

    #endregion

    #region Secret

    [AvaloniaFact]
    public void LinkSecretInDeploymentEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDeploymentInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ConfigMap>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDeploymentEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDeploymentInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDeploymentVolume()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                                        Secret = new()
                                        {
                                            SecretName = "my-config"
                                        }
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }


    [AvaloniaFact]
    public void LinkSecretInDaemonSetEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDaemonSetInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDaemonSetEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDaemonSetInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInDaemonSetVolume()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                                        Secret = new()
                                        {
                                            SecretName = "my-config"
                                        },
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }


    [AvaloniaFact]
    public void LinkSecretInStatefulSetEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInStatefulSetInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInStatefulSetEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInStatefulSetInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInStatefulSetVolume()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                                        Secret = new()
                                        {
                                            SecretName = "my-config"
                                        },
                                    }
                                ]
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1StatefulSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    //todo add test for hidenoise and ReplicaSet = 1/0

    [AvaloniaFact]
    public void LinkSecretInReplicaSetEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInReplicaSetInitEnv()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInReplicaSetEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInReplicaSetInitEnvFrom()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public void LinkSecretInReplicaSetVolume()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                                        Secret = new()
                                        {
                                            SecretName = "my-config"
                                        },
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }


    [AvaloniaFact]
    public void LinkSecretInServiceAccount()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Secret>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default",
                        Uid = "123"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Secret>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ServiceAccount>
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
                    Secrets =
                    [
                        new ()
                        {
                            Uid = "123"
                        }
                    ]
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ServiceAccount>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Secret>();
    }

    #endregion

    [AvaloniaFact]
    public void LinkEvent()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, Corev1Event>
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
                    InvolvedObject = new()
                    {
                        Uid = "123"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<Corev1Event>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
        {
            {
                new("default", "my-deployment"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-deployment",
                        NamespaceProperty = "default",
                        Uid = "123"
                    },
                    Spec = new()
                    {
                        Template = new()
                        {
                            Spec = new()
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<Corev1Event>();
    }

    [AvaloniaFact]
    public void LinkEndpointSlice()
    {
        var (mock, resources) = GetMock();

        var start = new ConcurrentObservableDictionary<NamespacedName, V1EndpointSlice>
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
                    Endpoints =
                    [
                        new()
                        {
                            TargetRef = new()
                            {
                                Uid = "123"
                            }
                        }
                    ]
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1EndpointSlice>(), new() { Items = start });

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Pod>
        {
            {
                new("default", "my-deployment"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-deployment",
                        NamespaceProperty = "default",
                        Uid = "123"
                    },
                    Spec = new()
                    {
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Pod>(), new() { Items = end });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1EndpointSlice>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public void LinkEndpoints()
    {
        var (mock, resources) = GetMock();

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Endpoints>
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
                                        Uid = "123"
                                    }
                                }
                            ]
                        }
                    ]
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Endpoints>(), new() { Items = start });

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Pod>
        {
            {
                new("default", "my-deployment"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-deployment",
                        NamespaceProperty = "default",
                        Uid = "123"
                    },
                    Spec = new()
                    {
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Pod>(), new() { Items = end });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Endpoints>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public void LinkIngress()
    {
        var (mock, resources) = GetMock();

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Ingress>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Ingress>(), new() { Items = start });

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Service>
        {
            {
                new("default", "my-service"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-service",
                        NamespaceProperty = "default",
                    },
                    Spec = new()
                    {
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Pod>(), new() { Items = end });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Ingress>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Service>();
    }

    [AvaloniaFact]
    public void LinkIngressDefaultBackend()
    {
        var (mock, resources) = GetMock();

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Ingress>
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
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Ingress>(), new() { Items = start });

        var end = new ConcurrentObservableDictionary<NamespacedName, V1Service>
        {
            {
                new("default", "my-service"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-service",
                        NamespaceProperty = "default",
                    },
                    Spec = new()
                    {
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Pod>(), new() { Items = end });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Ingress>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Service>();
    }

    #region ServiceAccount

    [AvaloniaFact]
    public void LinkServiceAccountInDeployment()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ServiceAccount>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ServiceAccount>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1Deployment>
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
                                Containers = [],
                                ServiceAccountName = "my-config"
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1Deployment>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public void LinkServiceAccountInStatefulSet()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ServiceAccount>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ServiceAccount>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1StatefulSet>
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
                                Containers = [],
                                ServiceAccountName = "my-config"
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1Deployment>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1StatefulSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public void LinkServiceAccountInDaemonSet()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ServiceAccount>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ServiceAccount>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1DaemonSet>
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
                                Containers = [],
                                ServiceAccountName = "my-config"
                            }
                        }
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1DaemonSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1DaemonSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public void LinkServiceAccountInReplicaSet()
    {
        var (mock, resources) = GetMock();

        var end = new ConcurrentObservableDictionary<NamespacedName, V1ServiceAccount>
        {
            {
                new("default", "my-config"),
                new()
                {
                    Metadata = new()
                    {
                        Name = "my-config",
                        NamespaceProperty = "default"
                    }
                }
            }
        };

        resources.TryAdd(GroupApiVersionKind.From<V1ServiceAccount>(), new() { Items = end });

        var start = new ConcurrentObservableDictionary<NamespacedName, V1ReplicaSet>
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
                                Containers = [],
                                ServiceAccountName = "my-config"
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

        resources.TryAdd(GroupApiVersionKind.From<V1ReplicaSet>(), new() { Items = start });

        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        vm.Initialize(mock.Object);

        vm.Drawing.Connectors.Count.Should().Be(1);
        vm.Drawing.Connectors[0].Start.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ReplicaSet>();
        vm.Drawing.Connectors[0].End.Parent.As<ResourceNodeViewModel>().Resource.Should().BeOfType<V1ServiceAccount>();
    }

    #endregion
}
