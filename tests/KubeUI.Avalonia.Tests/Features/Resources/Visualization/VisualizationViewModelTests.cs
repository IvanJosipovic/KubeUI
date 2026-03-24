using Avalonia;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.ViewModels;
using Shouldly;
using static KubeUI.Avalonia.ViewModels.VisualizationViewModel;

namespace KubeUI.Avalonia.Tests;

public class VisualizationViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];

    public override void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    private async Task<ClusterWorkspaceViewModel> CreateClusterAsync()
    {
        var cluster = await TestCluster.GetAsync();
        _disposables.Add(cluster);
        return cluster;
    }

    private VisualizationViewModel CreateViewModel()
    {
        var vm = Application.Current.GetRequiredService<VisualizationViewModel>();
        _disposables.Add(vm);
        return vm;
    }

    [AvaloniaFact]
    public async Task LinkOwners()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        // Simulate selection
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkArgoCDTracking()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<ArgoApplication>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    #region ConfigMap

    [AvaloniaFact]
    public async Task LinkConfigMapInPodEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();

        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInPodVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDeploymentVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInDaemonSetVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInStatefulSetVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    //todo add test for hidenoise and ReplicaSet = 1/0

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkConfigMapInReplicaSetVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
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
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDeploymentVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInDaemonSetVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInStatefulSetVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetInitEnv()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetInitEnvFrom()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInReplicaSetVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkSecretInServiceAccount()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    #endregion

    [AvaloniaFact]
    public async Task LinkEvent()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();

        vm.HideNoise = false;
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<Corev1Event>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
    }

    [AvaloniaFact]
    public async Task LinkPodToEndpointSlice()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1EndpointSlice>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public async Task LinkPodToEndpointEndpoints()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Endpoints>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
    }

    [AvaloniaFact]
    public async Task LinkIngressToService()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Ingress>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Service>();
    }

    [AvaloniaFact]
    public async Task LinkIngressDefaultBackend()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Ingress>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Service>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimToPersistantVolume()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolume>();
    }

    [AvaloniaFact]
    public async Task LinkRoleBindingToServiceAccount()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkRoleBindingToRole()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Role>();
    }

    [AvaloniaFact]
    public async Task LinkRoleBindingClusterRole()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRole>();
    }

    [AvaloniaFact]
    public async Task LinkClusterRoleBindingToServiceAccount()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkClusterRoleBindingClusterRole()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRoleBinding>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRole>();
    }

    #region ServiceAccount

    [AvaloniaFact]
    public async Task LinkServiceAccountInPod()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInDeployment()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInStatefulSet()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInDaemonSet()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
    }

    [AvaloniaFact]
    public async Task LinkServiceAccountInReplicaSet()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
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
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInDeployment()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInStatefulSet()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInDaemonSet()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task LinkPersistentVolumeClaimInReplicaSet()
    {
        var cluster = await CreateClusterAsync();

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

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        vm.Graph.Edges.Count.ShouldBe(1);
        vm.Graph.Edges.First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        vm.Graph.Edges.First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    #endregion

    [KubernetesEntity(Group = "argoproj.io", ApiVersion = "v1alpha1", Kind = "Application", PluralName = "applications")]
    public sealed class ArgoApplication : IKubernetesObject<V1ObjectMeta>
    {
        public string ApiVersion { get; set; } = "argoproj.io/v1alpha1";

        public string Kind { get; set; } = "Application";

        public V1ObjectMeta Metadata { get; set; } = new();
    }
}


