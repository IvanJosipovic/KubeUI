using Avalonia;
using Avalonia.Headless.XUnit;
using AvaloniaGraphControl;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;
using static KubeUI.Avalonia.Features.Resources.Visualization.ViewModels.VisualizationViewModel;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

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
        var vm = TestApp.CurrentServices?.GetRequiredService<VisualizationViewModel>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        _disposables.Add(vm);
        return vm;
    }

    private static IEnumerable<Edge> ResourceEdges(Graph graph)
        => graph.Edges.Where(edge => edge is not GroupEdge);

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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkOwners_CronJobJobPodChain()
    {
        var cluster = await CreateClusterAsync();

        const string namespaceName = "viz-ownership-chain";

        var backupNamespace = new V1Namespace
        {
            Metadata = new()
            {
                Name = namespaceName,
            },
        };

        await cluster.AddOrUpdateResource(backupNamespace);
        cluster.SelectedNamespaces.Add(backupNamespace);

        await cluster.AddOrUpdateResource(new V1CronJob
        {
            ApiVersion = "batch/v1",
            Kind = V1CronJob.KubeKind,
            Metadata = new()
            {
                Name = "backup-backup",
                NamespaceProperty = namespaceName,
                Uid = "cronjob-uid",
            },
        });

        await cluster.AddOrUpdateResource(new V1Job
        {
            ApiVersion = "batch/v1",
            Kind = V1Job.KubeKind,
            Metadata = new()
            {
                Name = "backup-backup-29651025",
                NamespaceProperty = namespaceName,
                Uid = "job-uid",
                OwnerReferences =
                [
                    new()
                    {
                        ApiVersion = "batch/v1",
                        Kind = V1CronJob.KubeKind,
                        Name = "backup-backup",
                        Uid = "cronjob-uid",
                        Controller = true,
                        BlockOwnerDeletion = true,
                    }
                ],
            },
        });

        await cluster.AddOrUpdateResource(new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "backup-backup-29651025-ddzf6",
                NamespaceProperty = namespaceName,
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new()
                    {
                        ApiVersion = "batch/v1",
                        Kind = V1Job.KubeKind,
                        Name = "backup-backup-29651025",
                        Uid = "job-uid",
                        Controller = true,
                        BlockOwnerDeletion = true,
                    }
                ],
            },
            Spec = new V1PodSpec(),
        });

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        var edges = ResourceEdges(vm.Graph)
            .Select(edge => (
                Tail: ((ResourceNodeViewModel)edge.Tail).Resource.GetType(),
                Head: ((ResourceNodeViewModel)edge.Head).Resource.GetType()))
            .ToHashSet();

        edges.Count.ShouldBe(2);
        edges.ShouldContain((typeof(V1CronJob), typeof(V1Job)));
        edges.ShouldContain((typeof(V1Job), typeof(V1Pod)));
    }

    [AvaloniaFact]
    public async Task LinkOwners_CronJobJobPodChain_WhenVisualizingPod_IncludesParents()
    {
        var cluster = await CreateClusterAsync();

        const string namespaceName = "viz-ownership-root";

        var namespaceResource = new V1Namespace
        {
            Metadata = new()
            {
                Name = namespaceName,
            },
        };

        await cluster.AddOrUpdateResource(namespaceResource);

        var cronJob = new V1CronJob
        {
            ApiVersion = "batch/v1",
            Kind = V1CronJob.KubeKind,
            Metadata = new()
            {
                Name = "backup-backup",
                NamespaceProperty = namespaceName,
                Uid = "cronjob-uid",
            },
        };

        await cluster.AddOrUpdateResource(cronJob);

        var job = new V1Job
        {
            ApiVersion = "batch/v1",
            Kind = V1Job.KubeKind,
            Metadata = new()
            {
                Name = "backup-backup-29651025",
                NamespaceProperty = namespaceName,
                Uid = "job-uid",
                OwnerReferences =
                [
                    new()
                    {
                        ApiVersion = "batch/v1",
                        Kind = V1CronJob.KubeKind,
                        Name = "backup-backup",
                        Uid = "cronjob-uid",
                        Controller = true,
                        BlockOwnerDeletion = true,
                    }
                ],
            },
        };

        await cluster.AddOrUpdateResource(job);

        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "backup-backup-29651025-ddzf6",
                NamespaceProperty = namespaceName,
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new()
                    {
                        ApiVersion = "batch/v1",
                        Kind = V1Job.KubeKind,
                        Name = "backup-backup-29651025",
                        Uid = "job-uid",
                        Controller = true,
                        BlockOwnerDeletion = true,
                    }
                ],
            },
            Spec = new V1PodSpec(),
        };

        await cluster.AddOrUpdateResource(pod);

        var vm = CreateViewModel();
        vm.Initialize(cluster, pod);

        var resourceTypes = vm.Resources.Select(x => x.Resource.GetType()).ToHashSet();
        resourceTypes.ShouldContain(typeof(V1Pod));
        resourceTypes.ShouldContain(typeof(V1Job));
        resourceTypes.ShouldContain(typeof(V1CronJob));

        var edges = ResourceEdges(vm.Graph)
            .Select(edge => (
                Tail: ((ResourceNodeViewModel)edge.Tail).Resource.GetType(),
                Head: ((ResourceNodeViewModel)edge.Head).Resource.GetType()))
            .ToHashSet();

        edges.Count.ShouldBe(2);
        edges.ShouldContain((typeof(V1CronJob), typeof(V1Job)));
        edges.ShouldContain((typeof(V1Job), typeof(V1Pod)));
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<ArgoApplication>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
    }

    [AvaloniaFact]
    public async Task LinkFluxCDKustomization()
    {
        var cluster = await CreateClusterAsync();

        var namespaceResource = new V1Namespace
        {
            Metadata = new()
            {
                Name = "flux-system"
            }
        };

        await cluster.AddOrUpdateResource(namespaceResource);
        cluster.SelectedNamespaces.Add(namespaceResource);

        await cluster.AddOrUpdateResource(new FluxKustomization
        {
            Metadata = new()
            {
                Name = "app",
                NamespaceProperty = "flux-system"
            }
        });

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "app-config",
                NamespaceProperty = "flux-system",
                Labels = new Dictionary<string, string>
                {
                    ["kustomize.toolkit.fluxcd.io/name"] = "app",
                    ["kustomize.toolkit.fluxcd.io/namespace"] = "flux-system"
                }
            }
        });

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<FluxKustomization>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task LinkFluxCDHelmRelease()
    {
        var cluster = await CreateClusterAsync();

        var namespaceResource = new V1Namespace
        {
            Metadata = new()
            {
                Name = "plex"
            }
        };

        await cluster.AddOrUpdateResource(namespaceResource);
        cluster.SelectedNamespaces.Add(namespaceResource);

        await cluster.AddOrUpdateResource(new FluxHelmRelease
        {
            Metadata = new()
            {
                Name = "plex",
                NamespaceProperty = "plex"
            }
        });

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "plex-config",
                NamespaceProperty = "plex",
                Labels = new Dictionary<string, string>
                {
                    ["helm.toolkit.fluxcd.io/name"] = "plex",
                    ["helm.toolkit.fluxcd.io/namespace"] = "plex"
                }
            }
        });

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<FluxHelmRelease>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
    }

    [AvaloniaFact]
    public async Task Initialize_with_root_secret_filters_to_parents_only()
    {
        var cluster = await CreateClusterAsync();

        var deployment = new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-app",
                NamespaceProperty = "default",
                Uid = "deployment-uid"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox",
                            }
                        ]
                    }
                }
            }
        };

        await cluster.AddOrUpdateResource(deployment);

        var replicaSet = new V1ReplicaSet
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b",
                NamespaceProperty = "default",
                Uid = "replicaset-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "deployment-uid",
                        Kind = "Deployment",
                        ApiVersion = V1Deployment.KubeApiVersion,
                        Controller = true,
                    }
                ],
            },
        };

        await cluster.AddOrUpdateResource(replicaSet);

        var pod = new V1Pod
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b-abcde",
                NamespaceProperty = "default",
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "replicaset-uid",
                        Kind = "ReplicaSet",
                        ApiVersion = V1ReplicaSet.KubeApiVersion,
                        Controller = true,
                    }
                ]
            },
            Spec = new()
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox",
                    }
                ],
                ServiceAccountName = "workload-sa",
                Volumes =
                [
                    new()
                    {
                        Name = "config",
                        ConfigMap = new()
                        {
                            Name = "workload-config"
                        }
                    }
                ]
            },
        };

        var secret = new V1Secret
        {
            Metadata = new()
            {
                Name = "workload-secret",
                NamespaceProperty = "default",
                Uid = "secret-uid"
            }
        };

        await cluster.AddOrUpdateResource(secret);

        pod.Spec = new()
        {
            Containers =
            [
                new V1Container
                {
                    Name = "app",
                    Image = "busybox",
                    Env =
                    [
                        new()
                        {
                            ValueFrom = new()
                            {
                                SecretKeyRef = new()
                                {
                                    Name = "workload-secret"
                                }
                            }
                        }
                    ]
                }
            ],
            ServiceAccountName = "workload-sa",
            Volumes =
            [
                new()
                {
                    Name = "config",
                    ConfigMap = new()
                    {
                        Name = "workload-config"
                    }
                }
            ]
        };

        await cluster.AddOrUpdateResource(pod);

        await cluster.AddOrUpdateResource(new V1ServiceAccount
        {
            Metadata = new()
            {
                Name = "workload-sa",
                NamespaceProperty = "default"
            }
        });

        await cluster.AddOrUpdateResource(new V1Secret
        {
            Metadata = new()
            {
                Name = "other-secret",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>()
        });

        await cluster.AddOrUpdateResource(new V1ConfigMap
        {
            Metadata = new()
            {
                Name = "unrelated-config",
                NamespaceProperty = "default",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "other-owner"
                    }
                ]
            },
            Data = new Dictionary<string, string>()
        });

        var vm = CreateViewModel();
        vm.Initialize(cluster, secret);

        vm.Resources.Count.ShouldBe(4);
        vm.Resources.Select(x => x.Resource).Any(x => ReferenceEquals(x, deployment)).ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1ReplicaSet linkedReplicaSet && linkedReplicaSet.Name() == "my-app-7c4f5d8f7b").ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1Pod linkedPod && linkedPod.Name() == "my-app-7c4f5d8f7b-abcde").ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1Secret linkedSecret && linkedSecret.Name() == "workload-secret").ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1ServiceAccount linkedServiceAccount && linkedServiceAccount.Name() == "workload-sa").ShouldBeFalse();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1ConfigMap linkedConfigMap && linkedConfigMap.Name() == "workload-config").ShouldBeFalse();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1ConfigMap configMap && configMap.Name() == "unrelated-config").ShouldBeFalse();
        vm.Resources.Single(x => x.Resource is V1Secret linkedSecret && linkedSecret.Name() == "workload-secret").IsRoot.ShouldBeTrue();
        vm.Resources.Where(x => !ReferenceEquals(x.Resource, secret)).All(x => !x.IsRoot).ShouldBeTrue();
        ResourceEdges(vm.Graph).Count().ShouldBe(3);
        ResourceEdges(vm.Graph).Select(x => x.Tail).OfType<ResourceNodeViewModel>().Any(x => ReferenceEquals(x.Resource, deployment)).ShouldBeTrue();
        ResourceEdges(vm.Graph).Select(x => x.Tail).OfType<ResourceNodeViewModel>().Any(x => x.Resource is V1ReplicaSet linkedReplicaSet && linkedReplicaSet.Name() == "my-app-7c4f5d8f7b").ShouldBeTrue();
        ResourceEdges(vm.Graph).Select(x => x.Tail).OfType<ResourceNodeViewModel>().Any(x => x.Resource is V1Pod linkedPod && linkedPod.Name() == "my-app-7c4f5d8f7b-abcde").ShouldBeTrue();
        ResourceEdges(vm.Graph).Select(x => x.Head).OfType<ResourceNodeViewModel>().Any(x => x.Resource is V1Secret linkedSecret && linkedSecret.Name() == "workload-secret").ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Initialize_with_root_deployment_includes_dependents()
    {
        var cluster = await CreateClusterAsync();

        var deployment = new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-app",
                NamespaceProperty = "default",
                Uid = "deployment-uid"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox"
                            }
                        ]
                    }
                }
            }
        };

        await cluster.AddOrUpdateResource(deployment);

        var replicaSet = new V1ReplicaSet
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b",
                NamespaceProperty = "default",
                Uid = "replicaset-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "deployment-uid",
                        Kind = "Deployment",
                        ApiVersion = V1Deployment.KubeApiVersion,
                        Controller = true,
                    }
                ],
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox",
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "workload-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        };

        await cluster.AddOrUpdateResource(replicaSet);

        var pod = new V1Pod
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b-abcde",
                NamespaceProperty = "default",
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "replicaset-uid",
                        Kind = "ReplicaSet",
                        ApiVersion = V1ReplicaSet.KubeApiVersion,
                        Controller = true,
                    }
                ]
            },
            Spec = new()
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox",
                        Env =
                        [
                            new()
                            {
                                ValueFrom = new()
                                {
                                    SecretKeyRef = new()
                                    {
                                        Name = "workload-secret"
                                    }
                                }
                            }
                        ]
                    }
                ]
            }
        };

        await cluster.AddOrUpdateResource(pod);

        await cluster.AddOrUpdateResource(new V1Secret
        {
            Metadata = new()
            {
                Name = "workload-secret",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>()
        });

        var vm = CreateViewModel();
        vm.Initialize(cluster, deployment);

        vm.Resources.Select(x => x.Resource).Any(x => ReferenceEquals(x, deployment)).ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1ReplicaSet linkedReplicaSet && linkedReplicaSet.Name() == "my-app-7c4f5d8f7b").ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1Pod linkedPod && linkedPod.Name() == "my-app-7c4f5d8f7b-abcde").ShouldBeTrue();
        vm.Resources.Select(x => x.Resource).Any(x => x is V1Secret linkedSecret && linkedSecret.Name() == "workload-secret").ShouldBeFalse();
        ResourceEdges(vm.Graph).Count().ShouldBe(2);
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, deployment) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, replicaSet)).ShouldBeTrue();
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, replicaSet) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, pod)).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Initialize_with_root_pod_includes_secret_dependent()
    {
        var cluster = await CreateClusterAsync();

        var pod = new V1Pod
        {
            Metadata = new()
            {
                Name = "my-pod",
                NamespaceProperty = "default",
                Uid = "pod-uid"
            },
            Spec = new()
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox",
                        Env =
                        [
                            new()
                            {
                                ValueFrom = new()
                                {
                                    SecretKeyRef = new()
                                    {
                                        Name = "workload-secret"
                                    }
                                }
                            }
                        ]
                    }
                ]
            }
        };

        await cluster.AddOrUpdateResource(pod);

        await cluster.AddOrUpdateResource(new V1Secret
        {
            Metadata = new()
            {
                Name = "workload-secret",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>()
        });

        var vm = CreateViewModel();
        vm.Initialize(cluster, pod);

        vm.Resources.Select(x => x.Resource).Any(x => x is V1Secret linkedSecret && linkedSecret.Name() == "workload-secret").ShouldBeTrue();
        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, pod) && x.Head is ResourceNodeViewModel head && head.Resource is V1Secret linkedSecret && linkedSecret.Name() == "workload-secret").ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Initialize_with_selected_namespace_keeps_nearest_dependency_only()
    {
        var cluster = await CreateClusterAsync();

        var selectedNamespace = new V1Namespace
        {
            Metadata = new()
            {
                Name = "default"
            }
        };

        await cluster.AddOrUpdateResource(selectedNamespace);
        cluster.SelectedNamespaces.Add(selectedNamespace);

        var deployment = new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-app",
                NamespaceProperty = "default",
                Uid = "deployment-uid"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox",
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "shared-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        };

        var replicaSet = new V1ReplicaSet
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b",
                NamespaceProperty = "default",
                Uid = "replicaset-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "deployment-uid",
                        Kind = "Deployment",
                        ApiVersion = V1Deployment.KubeApiVersion,
                        Controller = true,
                    }
                ],
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox",
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "shared-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        };

        var pod = new V1Pod
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b-abcde",
                NamespaceProperty = "default",
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "replicaset-uid",
                        Kind = "ReplicaSet",
                        ApiVersion = V1ReplicaSet.KubeApiVersion,
                        Controller = true,
                    }
                ]
            },
            Spec = new()
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox",
                        Env =
                        [
                            new()
                            {
                                ValueFrom = new()
                                {
                                    SecretKeyRef = new()
                                    {
                                        Name = "shared-secret"
                                    }
                                }
                            }
                        ]
                    }
                ]
            }
        };

        var secret = new V1Secret
        {
            Metadata = new()
            {
                Name = "shared-secret",
                NamespaceProperty = "default",
                Uid = "secret-uid"
            }
        };

        await cluster.AddOrUpdateResource(deployment);
        await cluster.AddOrUpdateResource(replicaSet);
        await cluster.AddOrUpdateResource(pod);
        await cluster.AddOrUpdateResource(secret);

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        ResourceEdges(vm.Graph).Count().ShouldBe(3);
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, deployment) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, secret)).ShouldBeFalse();
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, replicaSet) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, secret)).ShouldBeFalse();
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, pod) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, secret)).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Initialize_with_selected_namespace_keeps_nearest_dependency_only_when_replicaset_is_hidden()
    {
        var cluster = await CreateClusterAsync();

        var selectedNamespace = new V1Namespace
        {
            Metadata = new()
            {
                Name = "default"
            }
        };

        await cluster.AddOrUpdateResource(selectedNamespace);
        cluster.SelectedNamespaces.Add(selectedNamespace);

        var deployment = new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-app",
                NamespaceProperty = "default",
                Uid = "deployment-uid"
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox",
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "shared-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        };

        var replicaSet = new V1ReplicaSet
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b",
                NamespaceProperty = "default",
                Uid = "replicaset-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "deployment-uid",
                        Kind = "Deployment",
                        ApiVersion = V1Deployment.KubeApiVersion,
                        Controller = true,
                    }
                ],
            },
            Status = new()
            {
                Replicas = 0,
            },
            Spec = new()
            {
                Template = new()
                {
                    Spec = new()
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox",
                                Env =
                                [
                                    new()
                                    {
                                        ValueFrom = new()
                                        {
                                            SecretKeyRef = new()
                                            {
                                                Name = "shared-secret"
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
        };

        var pod = new V1Pod
        {
            Metadata = new()
            {
                Name = "my-app-7c4f5d8f7b-abcde",
                NamespaceProperty = "default",
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new()
                    {
                        Uid = "replicaset-uid",
                        Kind = "ReplicaSet",
                        ApiVersion = V1ReplicaSet.KubeApiVersion,
                        Controller = true,
                    }
                ]
            },
            Spec = new()
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox",
                        Env =
                        [
                            new()
                            {
                                ValueFrom = new()
                                {
                                    SecretKeyRef = new()
                                    {
                                        Name = "shared-secret"
                                    }
                                }
                            }
                        ]
                    }
                ]
            }
        };

        var secret = new V1Secret
        {
            Metadata = new()
            {
                Name = "shared-secret",
                NamespaceProperty = "default",
                Uid = "secret-uid"
            }
        };

        await cluster.AddOrUpdateResource(deployment);
        await cluster.AddOrUpdateResource(replicaSet);
        await cluster.AddOrUpdateResource(pod);
        await cluster.AddOrUpdateResource(secret);

        var vm = CreateViewModel();
        vm.Initialize(cluster);

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, deployment) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, secret)).ShouldBeFalse();
        ResourceEdges(vm.Graph).Any(x => x.Tail is ResourceNodeViewModel tail && ReferenceEquals(tail.Resource, pod) && x.Head is ResourceNodeViewModel head && ReferenceEquals(head.Resource, secret)).ShouldBeTrue();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ConfigMap>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Secret>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<Corev1Event>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1EndpointSlice>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Ingress>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Service>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Ingress>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Service>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolume>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Role>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRole>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1RoleBinding>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRoleBinding>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRole>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ClusterRoleBinding>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ServiceAccount>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Pod>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1Deployment>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1StatefulSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1DaemonSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
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

        ResourceEdges(vm.Graph).Count().ShouldBe(1);
        ResourceEdges(vm.Graph).First().Tail.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1ReplicaSet>();
        ResourceEdges(vm.Graph).First().Head.ShouldBeOfType<ResourceNodeViewModel>().Resource.ShouldBeOfType<V1PersistentVolumeClaim>();
    }

    [AvaloniaFact]
    public async Task GroupsResourcesByNamespaceAndComponentLabel()
    {
        var cluster = await CreateClusterAsync();

        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "worker-pod",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string>
                {
                    ["app.kubernetes.io/component"] = "worker",
                },
            },
            Spec = new V1PodSpec(),
        };

        await cluster.AddOrUpdateResource(pod);

        var vm = CreateViewModel();
        vm.Initialize(cluster, pod);

        var podNode = vm.Resources.Single(x => ReferenceEquals(x.Resource, pod));
        var componentGroup = vm.Graph.Parent[podNode];
        componentGroup.ShouldNotBeNull();
        componentGroup.ShouldBeOfType<GroupNodeViewModel>().DisplayText.ShouldBe("Component worker");

        var namespaceGroup = vm.Graph.Parent[componentGroup];
        namespaceGroup.ShouldNotBeNull();
        namespaceGroup.ShouldBeOfType<GroupNodeViewModel>().DisplayText.ShouldBe("Namespace default");

        vm.Graph.Edges.OfType<GroupEdge>().Count().ShouldBe(2);
    }

    #endregion

    [KubernetesEntity(Group = "argoproj.io", ApiVersion = "v1alpha1", Kind = "Application", PluralName = "applications")]
    public sealed class ArgoApplication : IKubernetesObject<V1ObjectMeta>
    {
        public string ApiVersion { get; set; } = "argoproj.io/v1alpha1";

        public string Kind { get; set; } = "Application";

        public V1ObjectMeta Metadata { get; set; } = new();
    }

    [KubernetesEntity(Group = "kustomize.toolkit.fluxcd.io", ApiVersion = "v1", Kind = "Kustomization", PluralName = "kustomizations")]
    public sealed class FluxKustomization : IKubernetesObject<V1ObjectMeta>
    {
        public string ApiVersion { get; set; } = "kustomize.toolkit.fluxcd.io/v1";

        public string Kind { get; set; } = "Kustomization";

        public V1ObjectMeta Metadata { get; set; } = new();
    }

    [KubernetesEntity(Group = "helm.toolkit.fluxcd.io", ApiVersion = "v2", Kind = "HelmRelease", PluralName = "helmreleases")]
    public sealed class FluxHelmRelease : IKubernetesObject<V1ObjectMeta>
    {
        public string ApiVersion { get; set; } = "helm.toolkit.fluxcd.io/v2";

        public string Kind { get; set; } = "HelmRelease";

        public V1ObjectMeta Metadata { get; set; } = new();
    }
}


