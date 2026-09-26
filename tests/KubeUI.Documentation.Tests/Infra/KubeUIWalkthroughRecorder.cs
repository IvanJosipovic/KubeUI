using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Rendering;
using KokoroSharp;
using k8s;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Kubernetes.Scenarios;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Documentation.Tests.Infra;

internal static class KubeUIWalkthroughRecorder
{
    private const string PodName = WalkthroughDemoResources.FeaturedPodName;
    private const int Width = KubeUIWalkthroughIntroView.VideoWidth;
    private const int Height = KubeUIWalkthroughIntroView.VideoHeight;
    private const int FramesPerSecond = 30;
    private const int CursorFrames = 23;
    private const double FrameDuration = 1.0 / FramesPerSecond;
    private const double OpeningFrameDuration = 0.4;

    public static bool IsRecordingEnabled =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KUBEUI_DOCS_VIDEO_DIR"));

    public static async Task RecordAsync(
        string theme,
        string clipName,
        string clusterName,
        Action<TestClusterConfig>? configureFakeCluster,
        WalkthroughStart start,
        WalkthroughIntro? intro,
        IReadOnlyList<WalkthroughStep> steps)
    {
        SetRecordingTheme(theme);
        var videoDirectory = Environment.GetEnvironmentVariable("KUBEUI_DOCS_VIDEO_DIR")
            ?? throw new InvalidOperationException("Set KUBEUI_DOCS_VIDEO_DIR to record feature videos.");
        if (steps.Count == 0)
        {
            throw new ArgumentException("A walkthrough needs at least one narration step.", nameof(steps));
        }

        using var window = new RecordingWindow { Width = Width, Height = Height, CanResize = false };
        var cluster = await CreateWorkspaceAsync(
            clusterName,
            connect: start.ConnectToCluster,
            configureFakeCluster);
        using var serviceScope = KubeUIWalkthroughServices.GetRequiredServices().CreateScope();
        try
        {
            var root = start.CreateView(
                cluster.Workspace,
                cluster.DemoResources,
                serviceScope.ServiceProvider);
            var cursor = new CursorOverlay { IsHitTestVisible = false };
            window.Content = new Grid { Children = { root, cursor } };
            window.Show();
            await WaitForUiAsync();
            await RecordClipAsync(
                clipName,
                theme,
                intro,
                root,
                cluster.Workspace,
                window,
                cursor,
                steps,
                videoDirectory);
        }
        finally
        {
            await cluster.TestCluster.DisposeAsync();
        }
    }

    private static async Task<(
        TestCluster TestCluster,
        ClusterWorkspace Workspace,
        WalkthroughDemoResources DemoResources)> CreateWorkspaceAsync(
        string name,
        bool connect,
        Action<TestClusterConfig>? configure)
    {
        var services = KubeUIWalkthroughServices.GetRequiredServices();
        var config = services.GetRequiredService<TestClusterConfig>();
        config.Type = KubernetesBackend.Fake;
        config.Name = name;
        var demoResources = CreateDemoResources();
        config.InitialResources = demoResources.Resources;
        configure?.Invoke(config);

        var testCluster = await services.GetRequiredService<TestClusterGenerator>()
            .CreateAsync(config, TestContext.Current.CancellationToken);
        try
        {
            testCluster.RegisterWith(services.GetRequiredService<ClusterManager>());
            var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().GetCluster(testCluster.Cluster.Name)
                ?? throw new InvalidOperationException("Test cluster workspace was not created.");
            if (connect)
            {
                var expectedPodMetrics = demoResources.Resources.OfType<V1Pod>().Count();
                await workspace.Connect();
                await workspace.Runtime.EnsureOpenApiSchemasAsync();
                await WaitForConditionAsync(() =>
                    workspace.Runtime.IsMetricsAvailable
                    && workspace.Runtime.NodeMetrics.Count == 3
                    && workspace.Runtime.PodMetrics.Count == expectedPodMetrics
                    && workspace.Runtime.NodeMetrics.All(metric =>
                        metric.Usage.ContainsKey("cpu") && metric.Usage.ContainsKey("memory"))
                    && workspace.Runtime.PodMetrics.All(metric =>
                        metric.Containers.Count > 0
                        && metric.Containers.All(container =>
                            container.Usage.ContainsKey("cpu") && container.Usage.ContainsKey("memory"))));
            }

            return (testCluster, workspace, demoResources);
        }
        catch
        {
            await testCluster.DisposeAsync();
            throw;
        }
    }

    internal static WalkthroughDemoResources CreateDemoResources()
    {
        V1Namespace defaultNamespace = new()
        {
            ApiVersion = "v1",
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "default" },
        };
        List<IKubernetesObject<V1ObjectMeta>> resources =
        [
            defaultNamespace,
        ];
        for (var index = 1; index <= 3; index++)
        {
            resources.Add(CreateNode(index));
            resources.Add(CreateNodeMetrics(index));
        }

        var replicaCountRandom = new Random(0x4B554955);
        var replicaSetHashRandom = new Random(0x5253504C);
        var podSuffixRandom = new Random(0x504F4453);
        var podNames = new HashSet<string>(StringComparer.Ordinal);
        var nodePodCounts = new int[3];
        var podIndex = 0;
        for (var index = 1; index <= 10; index++)
        {
            var appName = index == 1 ? "web" : $"web-{index:D2}";
            var labels = new Dictionary<string, string>
            {
                ["app"] = appName,
                ["app.kubernetes.io/name"] = appName,
                ["app.kubernetes.io/part-of"] = "kubeui-demo",
            };
            var replicaCount = replicaCountRandom.Next(2, 6);
            var claimName = $"data-{appName}";
            var replicaSetHash = index == 1
                ? "7c9f8d6f54"
                : replicaSetHashRandom.NextInt64(1L << 40).ToString("x10", CultureInfo.InvariantCulture);
            var replicaSetName = $"{appName}-{replicaSetHash}";
            var deploymentUid = $"walkthrough-deployment-{index:D2}";
            var replicaSetUid = $"walkthrough-replicaset-{index:D2}";
            var nodeIndex = ((index - 1) % 3) + 1;
            var nodeName = $"node-{nodeIndex}";
            var podLabels = new Dictionary<string, string>(labels)
            {
                ["pod-template-hash"] = replicaSetHash,
            };
            var failedPodCount = index == 10 ? 1 : 0;
            var readyReplicas = replicaCount - failedPodCount;

            V1PodTemplateSpec CreatePodTemplate(IDictionary<string, string> templateLabels) => new()
            {
                Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string>(templateLabels) },
                Spec = new V1PodSpec
                {
                    Containers = [CreateContainer(appName, index)],
                    NodeSelector = new Dictionary<string, string>
                    {
                        ["kubernetes.io/hostname"] = nodeName,
                    },
                    Volumes =
                    [
                        new V1Volume
                        {
                            Name = "data",
                            PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource { ClaimName = claimName },
                        },
                    ],
                },
            };

            var deploymentMetadata = CreateMetadata(appName, labels);
            deploymentMetadata.Uid = deploymentUid;
            var replicaSetMetadata = CreateMetadata(replicaSetName, podLabels);
            replicaSetMetadata.Uid = replicaSetUid;
            replicaSetMetadata.OwnerReferences =
            [
                CreateOwnerReference(V1Deployment.KubeKind, appName, deploymentUid),
            ];

            resources.Add(new V1Deployment
            {
                ApiVersion = "apps/v1",
                Kind = V1Deployment.KubeKind,
                Metadata = deploymentMetadata,
                Spec = new V1DeploymentSpec
                {
                    Replicas = replicaCount,
                    Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string>(labels) },
                    Template = CreatePodTemplate(labels),
                    RevisionHistoryLimit = 5,
                    ProgressDeadlineSeconds = 600,
                },
                Status = new V1DeploymentStatus
                {
                    ObservedGeneration = 1,
                    Replicas = replicaCount,
                    UpdatedReplicas = replicaCount,
                    ReadyReplicas = readyReplicas,
                    AvailableReplicas = readyReplicas,
                    UnavailableReplicas = failedPodCount,
                    Conditions = readyReplicas > 0 ?
                    [
                        new V1DeploymentCondition { Type = "Available", Status = "True", Reason = "MinimumReplicasAvailable", Message = "Deployment has minimum availability." },
                        new V1DeploymentCondition { Type = "Progressing", Status = "True", Reason = "NewReplicaSetAvailable", Message = "ReplicaSet has successfully progressed." },
                    ] :
                    [
                        new V1DeploymentCondition { Type = "Available", Status = "False", Reason = "MinimumReplicasUnavailable", Message = "Deployment has no available replicas." },
                        new V1DeploymentCondition { Type = "Progressing", Status = "False", Reason = "ProgressDeadlineExceeded", Message = "Deployment has not made progress." },
                    ],
                },
            });

            resources.Add(new V1ReplicaSet
            {
                ApiVersion = "apps/v1",
                Kind = V1ReplicaSet.KubeKind,
                Metadata = replicaSetMetadata,
                Spec = new V1ReplicaSetSpec
                {
                    Replicas = replicaCount,
                    Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string>(podLabels) },
                    Template = CreatePodTemplate(podLabels),
                },
                Status = new V1ReplicaSetStatus
                {
                    ObservedGeneration = 1,
                    Replicas = replicaCount,
                    FullyLabeledReplicas = replicaCount,
                    ReadyReplicas = readyReplicas,
                    AvailableReplicas = readyReplicas,
                },
            });

            for (var replicaIndex = 0; replicaIndex < replicaCount; replicaIndex++)
            {
                podIndex++;
                var nodePodIndex = ++nodePodCounts[nodeIndex - 1];
                var podName = CreateDemoPodName(
                    index,
                    appName,
                    replicaSetHash,
                    replicaIndex,
                    podSuffixRandom,
                    podNames);
                var qosClass = GetPodQosClass(index);
                var podIP = $"10.244.{nodeIndex}.{nodePodIndex + 10}";
                resources.Add(new V1Pod
                {
                    ApiVersion = "v1",
                    Kind = V1Pod.KubeKind,
                    Metadata = new V1ObjectMeta
                    {
                        Name = podName,
                        NamespaceProperty = "default",
                        Uid = $"walkthrough-pod-{podIndex:D3}",
                        Labels = new Dictionary<string, string>(podLabels),
                        OwnerReferences = [CreateOwnerReference(V1ReplicaSet.KubeKind, replicaSetName, replicaSetUid)],
                    },
                    Spec = new V1PodSpec
                    {
                        NodeName = nodeName,
                        Containers = [CreateContainer(appName, index)],
                        Volumes =
                        [
                            new V1Volume
                            {
                                Name = "data",
                                PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource { ClaimName = claimName },
                            },
                        ],
                    },
                    Status = CreatePodStatus(
                        index,
                        replicaIndex,
                        podIndex,
                        nodeIndex,
                        nodeName,
                        podIP,
                        qosClass,
                        podName),
                });
                resources.Add(CreatePodMetrics(podName, appName, podIndex));
            }

            resources.Add(new V1Service
            {
                ApiVersion = "v1",
                Kind = V1Service.KubeKind,
                Metadata = CreateMetadata($"svc-{appName}", labels),
                Spec = new V1ServiceSpec
                {
                    Type = "ClusterIP",
                    Selector = labels,
                    Ports = [new V1ServicePort { Name = "http", Port = 80, TargetPort = 80 }],
                },
            });

            resources.Add(new V1Ingress
            {
                ApiVersion = "networking.k8s.io/v1",
                Kind = V1Ingress.KubeKind,
                Metadata = CreateMetadata($"ing-{appName}", labels),
                Spec = new V1IngressSpec
                {
                    Rules =
                    [
                        new V1IngressRule
                        {
                            Host = $"{appName}.demo.kubeui.local",
                            Http = new V1HTTPIngressRuleValue
                            {
                                Paths =
                                [
                                    new V1HTTPIngressPath
                                    {
                                        Path = "/",
                                        PathType = "Prefix",
                                        Backend = new V1IngressBackend
                                        {
                                            Service = new V1IngressServiceBackend
                                            {
                                                Name = $"svc-{appName}",
                                                Port = new V1ServiceBackendPort { Number = 80 },
                                            },
                                        },
                                    },
                                ],
                            },
                        },
                    ],
                },
                Status = new V1IngressStatus
                {
                    LoadBalancer = new V1IngressLoadBalancerStatus
                    {
                        Ingress = [new V1IngressLoadBalancerIngress { Hostname = $"{appName}.demo.kubeui.local" }],
                    },
                },
            });

            var persistentVolumeName = $"pv-{appName}";
            resources.Add(new V1PersistentVolumeClaim
            {
                ApiVersion = "v1",
                Kind = V1PersistentVolumeClaim.KubeKind,
                Metadata = CreateMetadata(claimName, labels),
                Spec = CreateClaimSpec(persistentVolumeName),
                Status = new V1PersistentVolumeClaimStatus
                {
                    Phase = "Bound",
                    AccessModes = ["ReadWriteOnce"],
                    Capacity = new Dictionary<string, ResourceQuantity> { ["storage"] = new("5Gi") },
                },
            });

            resources.Add(new V1PersistentVolume
            {
                ApiVersion = "v1",
                Kind = V1PersistentVolume.KubeKind,
                Metadata = CreateMetadata(persistentVolumeName, labels),
                Spec = new V1PersistentVolumeSpec
                {
                    Capacity = new Dictionary<string, ResourceQuantity> { ["storage"] = new("5Gi") },
                    AccessModes = ["ReadWriteOnce"],
                    PersistentVolumeReclaimPolicy = "Retain",
                    StorageClassName = "demo-ssd",
                    ClaimRef = new V1ObjectReference
                    {
                        ApiVersion = "v1",
                        Kind = V1PersistentVolumeClaim.KubeKind,
                        NamespaceProperty = "default",
                        Name = claimName,
                    },
                    HostPath = new V1HostPathVolumeSource { Path = $"/mnt/kubeui-demo/{appName}" },
                },
                Status = new V1PersistentVolumeStatus { Phase = "Bound" },
            });
        }

        resources.Add(new V1StorageClass
        {
            ApiVersion = "storage.k8s.io/v1",
            Kind = V1StorageClass.KubeKind,
            Metadata = new V1ObjectMeta { Name = "demo-ssd" },
            Provisioner = "kubernetes.io/no-provisioner",
            VolumeBindingMode = "WaitForFirstConsumer",
        });

        return new WalkthroughDemoResources(resources, defaultNamespace);
    }

    private static V1Node CreateNode(int index)
    {
        var nodeName = $"node-{index}";
        var internalIP = $"192.168.65.{10 + index}";
        var nodeResources = new Dictionary<string, ResourceQuantity>
        {
            ["cpu"] = new("4"),
            ["ephemeral-storage"] = new("40Gi"),
            ["memory"] = new("8Gi"),
            ["pods"] = new("110"),
        };

        return new V1Node
        {
            ApiVersion = "v1",
            Kind = V1Node.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = nodeName,
                Labels = new Dictionary<string, string>
                {
                    ["kubernetes.io/arch"] = "amd64",
                    ["kubernetes.io/hostname"] = nodeName,
                    ["kubernetes.io/os"] = "linux",
                    ["node-role.kubernetes.io/worker"] = string.Empty,
                },
            },
            Spec = new V1NodeSpec
            {
                PodCIDR = $"10.244.{index}.0/24",
                ProviderID = $"fake://kubeui/{nodeName}",
            },
            Status = new V1NodeStatus
            {
                Addresses =
                [
                    new V1NodeAddress { Type = "InternalIP", Address = internalIP },
                    new V1NodeAddress { Type = "Hostname", Address = nodeName },
                ],
                Capacity = new Dictionary<string, ResourceQuantity>(nodeResources),
                Allocatable = new Dictionary<string, ResourceQuantity>(nodeResources),
                Conditions =
                [
                    new V1NodeCondition { Type = "Ready", Status = "True", Reason = "KubeletReady", Message = "kubelet is posting ready status." },
                    new V1NodeCondition { Type = "MemoryPressure", Status = "False", Reason = "KubeletHasSufficientMemory", Message = "node has sufficient memory." },
                    new V1NodeCondition { Type = "DiskPressure", Status = "False", Reason = "KubeletHasNoDiskPressure", Message = "node has no disk pressure." },
                    new V1NodeCondition { Type = "PIDPressure", Status = "False", Reason = "KubeletHasSufficientPID", Message = "node has sufficient PID available." },
                ],
                NodeInfo = new V1NodeSystemInfo
                {
                    Architecture = "amd64",
                    ContainerRuntimeVersion = "containerd://1.7.24",
                    KernelVersion = "6.8.0-kubeui-demo",
                    KubeletVersion = "v1.31.2",
                    KubeProxyVersion = "v1.31.2",
                    OperatingSystem = "linux",
                    OsImage = "Ubuntu 24.04 LTS",
                },
                Phase = "Running",
            },
        };
    }

    private static GenericKubernetesObject CreateNodeMetrics(int index)
    {
        var nodeName = $"node-{index}";
        return KubernetesJson.Deserialize<GenericKubernetesObject>(JsonSerializer.Serialize(new
        {
            apiVersion = "metrics.k8s.io/v1beta1",
            kind = "NodeMetrics",
            metadata = new { name = nodeName },
            timestamp = DateTime.UtcNow,
            window = "30s",
            usage = new Dictionary<string, string>
            {
                ["cpu"] = $"{420 + index * 135}m",
                ["memory"] = $"{1800 + index * 375}Mi",
            },
        }))!;
    }

    private static GenericKubernetesObject CreatePodMetrics(string podName, string containerName, int index)
    {
        return KubernetesJson.Deserialize<GenericKubernetesObject>(JsonSerializer.Serialize(new
        {
            apiVersion = "metrics.k8s.io/v1beta1",
            kind = "PodMetrics",
            metadata = new { name = podName, @namespace = "default" },
            timestamp = DateTime.UtcNow,
            window = "30s",
            containers = new[]
            {
                new
                {
                    name = containerName,
                    usage = new Dictionary<string, string>
                    {
                        ["cpu"] = $"{25 + index * 17}m",
                        ["memory"] = $"{42 + index * 19}Mi",
                    },
                },
            },
        }))!;
    }

    private static string GetPodQosClass(int index) => (index % 5) switch
    {
        0 => "Guaranteed",
        1 => "BestEffort",
        _ => "Burstable",
    };

    private static V1PodStatus CreatePodStatus(
        int deploymentIndex,
        int replicaIndex,
        int podIndex,
        int nodeIndex,
        string nodeName,
        string podIP,
        string qosClass,
        string podName)
    {
        var crashLooping = deploymentIndex == 10 && replicaIndex == 0;
        var appName = deploymentIndex == 1 ? "web" : $"web-{deploymentIndex:D2}";
        return new V1PodStatus
        {
            Phase = "Running",
            HostIP = $"192.168.65.{10 + nodeIndex}",
            PodIP = podIP,
            QosClass = qosClass,
            ContainerStatuses =
            [
                new V1ContainerStatus
                {
                    Name = appName,
                    Image = "nginx:1.27",
                    ImageID = "docker-pullable://nginx@sha256:4a2b2a2e2f8d",
                    ContainerID = $"containerd://kubeui-demo-{podIndex:D3}",
                    Ready = !crashLooping,
                    Started = !crashLooping,
                    RestartCount = crashLooping ? 4 : 0,
                    State = crashLooping
                        ? new V1ContainerState { Waiting = new V1ContainerStateWaiting { Reason = "CrashLoopBackOff", Message = "Back-off restarting failed container." } }
                        : new V1ContainerState { Running = new V1ContainerStateRunning() },
                    LastState = crashLooping
                        ? new V1ContainerState { Terminated = new V1ContainerStateTerminated { ExitCode = 1, Reason = "Error" } }
                        : new V1ContainerState(),
                },
            ],
            Conditions =
            [
                new V1PodCondition { Type = "PodScheduled", Status = "True", Reason = "Scheduled", Message = $"Successfully assigned default/{podName} to {nodeName}." },
                new V1PodCondition { Type = "Initialized", Status = "True", Reason = "PodInitialized" },
                new V1PodCondition { Type = "ContainersReady", Status = crashLooping ? "False" : "True", Reason = crashLooping ? "ContainersNotReady" : "ContainersReady" },
                new V1PodCondition { Type = "Ready", Status = crashLooping ? "False" : "True", Reason = crashLooping ? "ContainersNotReady" : "PodReady" },
            ],
        };
    }

    private static V1ObjectMeta CreateMetadata(string name, IDictionary<string, string> labels) => new()
    {
        Name = name,
        NamespaceProperty = "default",
        Labels = new Dictionary<string, string>(labels),
    };

    private static V1OwnerReference CreateOwnerReference(string kind, string name, string uid) => new()
    {
        ApiVersion = "apps/v1",
        Kind = kind,
        Name = name,
        Uid = uid,
        Controller = true,
        BlockOwnerDeletion = true,
    };

    private static string CreateDemoPodName(
        int deploymentIndex,
        string appName,
        string replicaSetHash,
        int replicaIndex,
        Random suffixRandom,
        HashSet<string> existingPodNames)
    {
        if (deploymentIndex == 1 && replicaIndex == 0)
        {
            existingPodNames.Add(PodName);
            return PodName;
        }

        var suffix = deploymentIndex == 1
            ? replicaIndex switch
            {
                1 => "2k4m7",
                2 => "2k4m9",
                _ => CreateRandomPodSuffix(suffixRandom),
            }
            : CreateRandomPodSuffix(suffixRandom);
        var name = $"{appName}-{replicaSetHash}-{suffix}";
        while (!existingPodNames.Add(name))
        {
            suffix = CreateRandomPodSuffix(suffixRandom);
            name = $"{appName}-{replicaSetHash}-{suffix}";
        }

        return name;
    }

    private static string CreateRandomPodSuffix(Random random)
    {
        const string characters = "abcdefghijklmnopqrstuvwxyz0123456789";
        Span<char> suffix = stackalloc char[5];
        for (var index = 0; index < suffix.Length; index++)
        {
            suffix[index] = characters[random.Next(characters.Length)];
        }

        return new string(suffix);
    }

    private static V1Container CreateContainer(string name, int index) => new()
    {
        Name = name,
        Image = "nginx:1.27",
        ImagePullPolicy = index % 2 == 0 ? "IfNotPresent" : "Always",
        Ports = [new V1ContainerPort { Name = "http", ContainerPort = 80 }],
        VolumeMounts = [new V1VolumeMount { Name = "data", MountPath = "/var/lib/kubeui" }],
        ReadinessProbe = new V1Probe
        {
            HttpGet = new V1HTTPGetAction { Path = "/", Port = 80 },
            InitialDelaySeconds = 3,
            PeriodSeconds = 10,
        },
        LivenessProbe = index % 3 == 0
            ? new V1Probe
            {
                HttpGet = new V1HTTPGetAction { Path = "/", Port = 80 },
                InitialDelaySeconds = 10,
                PeriodSeconds = 20,
            }
            : null,
        Resources = CreateResourceRequirements(index),
    };

    private static V1ResourceRequirements? CreateResourceRequirements(int index)
    {
        var qosClass = GetPodQosClass(index);
        if (qosClass == "BestEffort")
        {
            return null;
        }

        var requests = qosClass == "Guaranteed"
            ? new Dictionary<string, ResourceQuantity> { ["cpu"] = new("250m"), ["memory"] = new("128Mi") }
            : new Dictionary<string, ResourceQuantity> { ["cpu"] = new("100m"), ["memory"] = new("64Mi") };
        var limits = qosClass == "Guaranteed"
            ? new Dictionary<string, ResourceQuantity>(requests)
            : new Dictionary<string, ResourceQuantity> { ["cpu"] = new("500m"), ["memory"] = new("256Mi") };

        return new V1ResourceRequirements { Requests = requests, Limits = limits };
    }

    private static V1PersistentVolumeClaimSpec CreateClaimSpec(string volumeName) => new()
    {
        AccessModes = ["ReadWriteOnce"],
        Resources = new V1VolumeResourceRequirements
        {
            Requests = new Dictionary<string, ResourceQuantity> { ["storage"] = new("5Gi") },
        },
        StorageClassName = "demo-ssd",
        VolumeName = volumeName,
    };

    private static async Task RecordClipAsync(
        string clipName,
        string theme,
        WalkthroughIntro? intro,
        Control root,
        ClusterWorkspace workspace,
        Window window,
        CursorOverlay cursor,
        IReadOnlyList<WalkthroughStep> steps,
        string videoDirectory)
    {
        Directory.CreateDirectory(videoDirectory);
        var ffmpegPath = GetFfmpegPath();
        var temporaryDirectory = Path.Combine(videoDirectory, $".{clipName}-{theme}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var recordings = new List<StepRecording>(steps.Count + (intro is null ? 0 : 1));
            var origin = new Point(Width * 0.12, Height * 0.78);
            var synthesizer = KokoroWavSynthesizer.LoadModel();
            if (intro is not null)
            {
                var icon = ApplicationIcons.CreateControlPlaneImage();
                using (var introWindow = new RecordingWindow
                {
                    Width = Width,
                    Height = Height,
                    CanResize = false,
                    Content = new KubeUIWalkthroughIntroView(intro.Title, icon),
                })
                {
                    introWindow.Show();
                    await WaitForUiAsync();
                    var introFrame = CaptureFrame(introWindow, temporaryDirectory, -1, -1);
                    var introAudio = synthesizer.Synthesize(
                        intro.Narration,
                        KokoroVoiceManager.GetVoice("af_heart"));
                    var introAudioPath = Path.Combine(temporaryDirectory, "narration-intro.wav");
                    KokoroWavSynthesizer.SaveAudioToFile(introAudio, introAudioPath);
                    var introAudioInfo = ReadWaveInfo(introAudioPath);
                    recordings.Add(new StepRecording(
                        introFrame,
                        [],
                        introFrame,
                        introAudioPath,
                        introAudioInfo.Duration,
                        introAudioInfo));
                }
            }

            for (var stepIndex = 0; stepIndex < steps.Count; stepIndex++)
            {
                var step = steps[stepIndex];
                if (step.ReadyWhen is { } readyWhen)
                {
                    try
                    {
                        await WaitForConditionAsync(() => readyWhen(root));
                    }
                    catch (TimeoutException error)
                    {
                        var description = step.ReadyDescription?.Invoke(root);
                        throw new TimeoutException(
                            $"Walkthrough step {stepIndex + 1} was not ready before narration: '{step.Narration}'. {description}",
                            error);
                    }
                }

                var openingFrame = CaptureFrame(window, temporaryDirectory, stepIndex, -1);
                var audio = synthesizer.Synthesize(step.Narration, KokoroVoiceManager.GetVoice("af_heart"));
                var audioPath = Path.Combine(temporaryDirectory, $"narration-{stepIndex:D3}.wav");
                KokoroWavSynthesizer.SaveAudioToFile(audio, audioPath);
                var audioInfo = ReadWaveInfo(audioPath);

                var movementFrames = new List<string>(step.Actions.Count * CursorFrames);
                foreach (var action in step.Actions)
                {
                    if (action.DelayBefore > TimeSpan.Zero)
                    {
                        var delayedFrames = checked((int)Math.Ceiling(
                            action.DelayBefore.TotalSeconds / FrameDuration));
                        AddHeldFrame(
                            window,
                            temporaryDirectory,
                            stepIndex,
                            movementFrames,
                            delayedFrames);
                    }

                    if (action.Target == "yaml-editor"
                        && action.Kind is WalkthroughActionKind.TypeText or WalkthroughActionKind.BackspaceText)
                    {
                        origin = await ParkYamlPointerAsync(window, cursor, origin, temporaryDirectory, stepIndex, movementFrames);
                        origin = await TypeYamlTextAsync(
                            action,
                            root,
                            window,
                            origin,
                            temporaryDirectory,
                            stepIndex,
                            movementFrames);
                        continue;
                    }

                    if (action.Kind == WalkthroughActionKind.ExploreCompletion)
                    {
                        origin = await ParkYamlPointerAsync(window, cursor, origin, temporaryDirectory, stepIndex, movementFrames);
                        origin = await ExploreYamlCompletionAsync(
                            action.Value!, root, window, origin,
                            temporaryDirectory, stepIndex, movementFrames);
                        continue;
                    }

                    if (action.Kind == WalkthroughActionKind.MovePointer && action.Target == "yaml-below-metadata")
                    {
                        var editor = GetYamlEditor(root);
                        var metadataPoint = GetYamlHeaderPoint(editor, "namespace", window);
                        var metadataDestination = new Point(
                            metadataPoint.X,
                            metadataPoint.Y + editor.TextArea.TextView.DefaultLineHeight);
                        await MoveCursorAsync(
                            window,
                            cursor,
                            origin,
                            metadataDestination,
                            temporaryDirectory,
                            stepIndex,
                            movementFrames,
                            moveNativePointerDuringMotion: false);
                        origin = metadataDestination;
                        continue;
                    }

                    if (action.Kind == WalkthroughActionKind.Hover)
                    {
                        await workspace.Runtime.EnsureOpenApiSchemasAsync();
                        var editor = root.FindControl<TextEditor>("Editor")
                            ?? throw new InvalidOperationException("The YAML editor was not found.");
                        var hoverDestination = GetYamlHeaderPoint(editor, action.Value!, window);
                        var headerOffset = editor.Document!.Text.IndexOf($"{action.Value}:", StringComparison.Ordinal);
                        var hoverReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                        EventHandler<PointerEventArgs> onPointerHover = (_, args) =>
                        {
                            var textView = editor.TextArea.TextView;
                            var position = textView.GetPosition(args.GetPosition(textView) + textView.ScrollOffset);
                            if (position.HasValue)
                            {
                                var offset = editor.Document.GetOffset(position.Value.Location);
                                if (offset >= headerOffset && offset <= headerOffset + action.Value!.Length)
                                {
                                    hoverReceived.TrySetResult();
                                }
                            }
                        };
                        editor.TextArea.TextView.PointerHover += onPointerHover;
                        try
                        {
                            await MoveCursorAsync(window, cursor, origin, hoverDestination, temporaryDirectory, stepIndex, movementFrames, moveNativePointerDuringMotion: false);
                            await hoverReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
                            await WaitForUiAsync();
                        }
                        finally
                        {
                            editor.TextArea.TextView.PointerHover -= onPointerHover;
                        }

                        var hoverFrame = CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count);
                        for (var frame = 0; frame < 15; frame++)
                        {
                            movementFrames.Add(hoverFrame);
                        }
                        origin = hoverDestination;
                        continue;
                    }

                    if (action.Kind == WalkthroughActionKind.Click && action.Target == "navigation")
                    {
                        origin = await OpenNavigationSectionsAsync(
                            action.Value!, root, window, cursor, origin, temporaryDirectory, stepIndex, movementFrames);
                    }

                    var (target, afterClick) = await ResolveActionAsync(action, root, workspace);
                    var destination = target.TranslatePoint(
                        new Point(target.Bounds.Width / 2, target.Bounds.Height / 2),
                        window)
                        ?? throw new InvalidOperationException($"Could not locate action '{action.Kind}' target in the window.");
                    if (action.Target == "yaml-editor" && action.Kind == WalkthroughActionKind.InsertText)
                    {
                        destination = GetYamlCaretPoint((TextEditor)target, window);
                    }

                    await MoveCursorAsync(window, cursor, origin, destination, temporaryDirectory, stepIndex, movementFrames);
                    if (action.Kind is WalkthroughActionKind.Click or WalkthroughActionKind.RightClick or WalkthroughActionKind.SetText)
                    {
                        await ClickAsync(
                            window,
                            cursor,
                            destination,
                            temporaryDirectory,
                            stepIndex,
                            movementFrames,
                            action.Kind == WalkthroughActionKind.RightClick
                                ? MouseButton.Right
                                : MouseButton.Left);
                    }

                    if (afterClick is not null)
                    {
                        try
                        {
                            await afterClick();
                        }
                        catch (TimeoutException error)
                        {
                            throw new TimeoutException(
                                $"Walkthrough action '{action.Target}' ('{action.Value}') did not complete.",
                                error);
                        }
                    }

                    movementFrames.Add(CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count));
                    origin = destination;
                }

                var finalFrame = CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count);
                recordings.Add(new StepRecording(openingFrame, movementFrames, finalFrame, audioPath, audioInfo.Duration, audioInfo));
            }

            var combinedAudioPath = Path.Combine(temporaryDirectory, "narration.wav");
            await CombineAudioAsync(recordings, combinedAudioPath, temporaryDirectory, ffmpegPath);
            await EncodeClipAsync(
                Path.Combine(videoDirectory, $"{clipName}-{theme}.mp4"),
                temporaryDirectory,
                recordings,
                combinedAudioPath,
                ffmpegPath);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static async Task<Point> ExploreYamlCompletionAsync(
        string field,
        Control root,
        Window window,
        Point origin,
        string temporaryDirectory,
        int stepIndex,
        List<string> movementFrames)
    {
        var editor = GetYamlEditor(root);
        editor.CaretOffset = editor.Document!.TextLength;
        editor.TextArea.Focus();
        window.KeyPress(Key.Space, RawInputModifiers.Control, PhysicalKey.Space, null);
        try
        {
            await WaitForConditionAsync(() => GetCompletionWindow(editor) is { IsOpen: true } completion
                && completion.CompletionList.CompletionData.Count > 0);
        }
        catch (TimeoutException error)
        {
            throw new InvalidOperationException($"YAML IntelliSense did not open for '{field}'. Document: {editor.Text.ReplaceLineEndings("\\n")}", error);
        }

        var completionWindow = GetCompletionWindow(editor)!;
        var candidates = completionWindow.CompletionList.CompletionData.OfType<ICompletionData>().ToArray();
        var targetIndex = Array.FindIndex(candidates, item => item.Text == field);
        if (targetIndex < 0)
        {
            throw new InvalidOperationException($"YAML IntelliSense did not offer '{field}'. Offered: {string.Join(", ", candidates.Select(item => item.Text))}. Document: {editor.Text.ReplaceLineEndings("\\n")}");
        }

        AddHeldFrame(window, temporaryDirectory, stepIndex, movementFrames, 12);
        var selectedIndex = Array.FindIndex(candidates, item => ReferenceEquals(item, completionWindow.CompletionList.SelectedItem));
        if (selectedIndex < 0)
        {
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            await WaitForUiAsync();
            AddHeldFrame(window, temporaryDirectory, stepIndex, movementFrames, 6);
            selectedIndex = Array.FindIndex(candidates, item => ReferenceEquals(item, completionWindow.CompletionList.SelectedItem));
            if (selectedIndex < 0)
            {
                throw new InvalidOperationException("The first YAML completion was not selected with Down Arrow.");
            }
        }

        var downSteps = (targetIndex - selectedIndex + candidates.Length) % candidates.Length;
        var upSteps = (selectedIndex - targetIndex + candidates.Length) % candidates.Length;
        var direction = downSteps <= upSteps ? Key.Down : Key.Up;
        var physicalKey = direction == Key.Down ? PhysicalKey.ArrowDown : PhysicalKey.ArrowUp;
        var steps = Math.Min(downSteps, upSteps);
        for (var index = 0; index < steps; index++)
        {
            window.KeyPress(direction, RawInputModifiers.None, physicalKey, null);
            await WaitForUiAsync();
            AddHeldFrame(window, temporaryDirectory, stepIndex, movementFrames, 6);
        }

        if (completionWindow.CompletionList.SelectedItem?.Text != field)
        {
            throw new InvalidOperationException($"Arrow navigation selected '{completionWindow.CompletionList.SelectedItem?.Text}' instead of '{field}'.");
        }

        AddHeldFrame(window, temporaryDirectory, stepIndex, movementFrames, 12);
        var previousText = editor.Text;
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        await WaitForUiAsync();
        if (editor.Text == previousText)
        {
            throw new InvalidOperationException($"Enter did not insert the selected YAML completion '{field}'.");
        }

        movementFrames.Add(CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count));
        return origin;
    }

    private static async Task<Point> TypeYamlTextAsync(
        WalkthroughAction action,
        Control root,
        Window window,
        Point origin,
        string temporaryDirectory,
        int stepIndex,
        List<string> movementFrames)
    {
        var editor = GetYamlEditor(root);
        editor.CaretOffset = editor.Document!.TextLength;
        editor.TextArea.Focus();

        if (action.Kind == WalkthroughActionKind.BackspaceText
            && !editor.Text.EndsWith(action.Value!, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected YAML to end with '{action.Value}' before pressing Backspace.");
        }

        for (var index = 0; index < action.Value!.Length; index++)
        {
            if (action.Kind == WalkthroughActionKind.TypeText)
            {
                if (action.Value[index] == '\n')
                {
                    window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
                    await WaitForUiAsync();

                    var expectedIndent = 0;
                    while (index + 1 + expectedIndent < action.Value.Length
                        && action.Value[index + 1 + expectedIndent] == ' ')
                    {
                        expectedIndent++;
                    }

                    var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                    var actualIndent = editor.CaretOffset - line.Offset;
                    var lineCount = editor.Document.LineCount;
                    while (actualIndent > expectedIndent)
                    {
                        window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);
                        await WaitForUiAsync();
                        if (editor.Document.LineCount != lineCount)
                        {
                            throw new InvalidOperationException("Backspace removed the YAML line while adjusting auto-indent.");
                        }
                        line = editor.Document.GetLineByOffset(editor.CaretOffset);
                        actualIndent = editor.CaretOffset - line.Offset;
                    }
                    while (actualIndent < expectedIndent)
                    {
                        window.KeyTextInput(" ");
                        await WaitForUiAsync();
                        actualIndent++;
                    }

                    index += expectedIndent;
                }
                else
                {
                    window.KeyTextInput(action.Value[index].ToString());
                }
            }
            else
            {
                window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);
            }

            await WaitForUiAsync();
            var frame = CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count);
            movementFrames.Add(frame);
            movementFrames.Add(frame);
        }

        if (action.Kind == WalkthroughActionKind.BackspaceText
            && editor.Text.EndsWith(action.Value!, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Backspace did not remove YAML suffix '{action.Value}'.");
        }

        return origin;
    }

    private static async Task<(Control Target, Func<Task>? AfterClick)> ResolveActionAsync(
        WalkthroughAction action,
        Control root,
        ClusterWorkspace workspace)
    {
        if (action.Kind == WalkthroughActionKind.Hover)
        {
            if (action.Target == "yaml-header")
            {
                return (GetYamlEditor(root), null);
            }

            throw new InvalidOperationException($"Unknown walkthrough hover target '{action.Target}'.");
        }

        if (action.Target == "yaml-editor")
        {
            var view = GetYamlView(root);
            var editor = GetYamlEditor(root);
            return action.Kind switch
            {
                WalkthroughActionKind.Click => (editor, null),
                WalkthroughActionKind.SetText => (editor, async () =>
                {
                    view.ViewModel.YamlDocument.Text = action.Value!;
                    editor.CaretOffset = editor.Document!.TextLength;
                    editor.TextArea.Focus();
                    await WaitForUiAsync();
                }),

                WalkthroughActionKind.InsertText => (editor, async () =>
                {
                    editor.Document!.Insert(editor.CaretOffset, action.Value!);
                    await WaitForUiAsync();
                }),
                _ => throw new InvalidOperationException($"Action '{action.Kind}' is not supported for the YAML editor."),
            };
        }

        if (action.Kind is not (WalkthroughActionKind.Click or WalkthroughActionKind.RightClick))
        {
            throw new InvalidOperationException($"Action '{action.Kind}' requires a supported walkthrough target.");
        }

        switch (action.Target)
        {
            case "cluster":
                var cluster = await WaitForControlAsync<TextBlock>(root, text => text.Text == action.Value);
                return (cluster, () => WaitForConnectionAsync(workspace));
            case "navigation":
                var navigationView = await WaitForControlAsync<NavigationView>(root, _ => true);
                return (await WaitForControlAsync<TextBlock>(navigationView, text => text.Text == action.Value), null);
            case "pod":
                var podRow = await WaitForControlAsync<DataGridRow>(
                    root,
                    row => row.IsVisible && (row.DataContext as V1Pod)?.Metadata?.Name == action.Value);
                if (action.Kind == WalkthroughActionKind.RightClick)
                {
                    var contextMenu = GetResourceListContextMenu(root);
                    return (podRow, () => WaitForConditionAsync(
                        () => contextMenu.IsOpen
                            && contextMenu.ItemsSource is IEnumerable<MenuItemViewModel> menuItems
                            && menuItems.Any()));
                }

                return (podRow, () => WaitForConditionAsync(() => podRow.IsSelected));
            case "context-menu-item":
                var menu = GetResourceListContextMenu(root);
                MenuItem menuItem;
                try
                {
                    menuItem = await WaitForMenuItemAsync(menu, action.Value!);
                }
                catch (TimeoutException error)
                {
                    throw new InvalidOperationException(
                        $"Context menu item '{action.Value}' did not appear.",
                        error);
                }

                return (menuItem, action.ReadyWhen is { } contextMenuReady
                    ? () => WaitForConditionAsync(() => contextMenuReady(root))
                    : null);
            case "logs-controller":
                var logsView = await WaitForControlAsync<PodLogsView>(
                    root,
                    view => view.IsVisible && view.ViewModel.CanJumpToController);
                var logsViewModel = logsView.ViewModel;
                return (
                    FindCommandButton(root, logsViewModel.JumpToControlledByLogsCommand),
                    () => WaitForConditionAsync(() => logsViewModel.Object?.Kind == action.Value));
            case "relationship-surface":
                var graph = root.GetVisualDescendants().OfType<Control>()
                    .FirstOrDefault(control => control.Bounds.Width > 300 && control.Bounds.Height > 300)
                    ?? root;
                return (graph, null);
            case "yaml-edit-mode":
                var editView = GetYamlView(root);
                return (FindCommandButton(editView, editView.ViewModel.SetEditModeCommand),
                    () => WaitForConditionAsync(() => editView.ViewModel.EditMode));
            case "yaml-dry-run":
                var dryRunView = GetYamlView(root);
                return (FindCommandButton(dryRunView, dryRunView.ViewModel.DryRunCommand),
                    () => WaitForConditionAsync(() =>
                        dryRunView.ViewModel.HasActionFailureResult || dryRunView.ViewModel.HasActionSuccessResult));
            case "yaml-save":
                var saveView = GetYamlView(root);
                return (FindCommandButton(saveView, saveView.ViewModel.SaveCommand), async () =>
                {
                    await WaitForConditionAsync(() => saveView.ViewModel.HasActionSuccessResult);
                });
            default:
                throw new InvalidOperationException($"Unknown walkthrough click target '{action.Target}'.");
        }
    }

    private static ResourceYamlView GetYamlView(Control root) => root as ResourceYamlView
        ?? throw new InvalidOperationException("The YAML view was not available.");

    private static ContextMenu GetResourceListContextMenu(Control root)
    {
        var grid = root.GetVisualDescendants().OfType<DataGrid>()
            .FirstOrDefault(candidate => candidate.Name == "PART_Grid")
            ?? throw new InvalidOperationException("The resource list grid was not found.");
        return grid.ContextMenu
            ?? throw new InvalidOperationException("The resource list context menu was not found.");
    }

    internal static bool HasContextMenuItem(Control root, string title)
    {
        return GetMenuItemDescendants(GetResourceListContextMenu(root))
            .Any(item => item.IsVisible && item.Header?.ToString() == title);
    }

    private static async Task<MenuItem> WaitForMenuItemAsync(ContextMenu menu, string title)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            await WaitForUiAsync();
            var match = GetMenuItemDescendants(menu)
                .FirstOrDefault(item => item.IsVisible && item.Header?.ToString() == title);
            if (match is not null)
            {
                return match;
            }
        }

        throw new TimeoutException($"Context menu item '{title}' did not appear.");
    }

    private static IEnumerable<MenuItem> GetMenuItemDescendants(Control root)
    {
        return root.GetVisualDescendants().OfType<MenuItem>()
            .Concat(root.GetLogicalDescendants().OfType<MenuItem>())
            .Distinct();
    }

    private static TextEditor GetYamlEditor(Control root) => root.FindControl<TextEditor>("Editor")
        ?? throw new InvalidOperationException("The YAML editor was not found.");

    private static CompletionWindow? GetCompletionWindow(TextEditor editor)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var field = typeof(YamlEditorBehavior).GetField("_completionWindow", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(behavior) as CompletionWindow;
    }

    private static Button FindCommandButton(Control root, System.Windows.Input.ICommand command)
    {
        return root.GetVisualDescendants().OfType<Button>().FirstOrDefault(button => button.Command == command)
            ?? throw new InvalidOperationException("The requested command button was not found.");
    }

    private static Point GetYamlHeaderPoint(TextEditor editor, string header, Window window)
    {
        editor.UpdateLayout();
        editor.TextArea.TextView.UpdateLayout();
        var offset = editor.Document!.Text.IndexOf($"{header}:", StringComparison.Ordinal);
        if (offset < 0)
        {
            throw new InvalidOperationException($"YAML header '{header}' was not found.");
        }

        var location = editor.Document.GetLocation(offset + 1);
        var point = editor.TextArea.TextView.GetVisualPosition(
            new TextViewPosition(location.Line, location.Column),
            VisualYPosition.TextTop);
        return editor.TextArea.TextView.TranslatePoint(new Point(point.X + 2, point.Y + 2), window)
            ?? throw new InvalidOperationException($"Could not locate YAML header '{header}' in the window.");
    }

    private static Point GetYamlCaretPoint(TextEditor editor, Window window)
    {
        editor.UpdateLayout();
        editor.TextArea.TextView.UpdateLayout();
        var location = editor.Document!.GetLocation(editor.CaretOffset);
        var point = editor.TextArea.TextView.GetVisualPosition(
            new TextViewPosition(location.Line, location.Column),
            VisualYPosition.LineMiddle);
        return editor.TextArea.TextView.TranslatePoint(new Point(point.X + 2, point.Y), window)
            ?? throw new InvalidOperationException("Could not locate the YAML editor caret in the window.");
    }

    private static async Task<Point> OpenNavigationSectionsAsync(
        string name,
        Control root,
        Window window,
        CursorOverlay cursor,
        Point origin,
        string temporaryDirectory,
        int stepIndex,
        List<string> movementFrames)
    {
        var navigationView = await WaitForControlAsync<NavigationView>(root, _ => true);
        var viewModel = navigationView.DataContext as NavigationViewModel
            ?? throw new InvalidOperationException("The navigation view model was not available.");
        var parents = new List<NavigationItem>();
        if (!FindNavigationParents(viewModel.Clusters, name, parents))
        {
            throw new InvalidOperationException($"Navigation item '{name}' was not found.");
        }

        foreach (var parent in parents)
        {
            if (parent.IsExpanded)
            {
                continue;
            }

            var section = await WaitForControlAsync<TextBlock>(navigationView, text => text.Text == parent.Name);
            var destination = section.TranslatePoint(new Point(section.Bounds.Width / 2, section.Bounds.Height / 2), window)
                ?? throw new InvalidOperationException($"Could not locate navigation section '{parent.Name}' in the window.");
            await MoveCursorAsync(window, cursor, origin, destination, temporaryDirectory, stepIndex, movementFrames);
            await ClickAsync(window, cursor, destination, temporaryDirectory, stepIndex, movementFrames);
            await WaitForConditionAsync(() => parent.IsExpanded);
            origin = destination;
        }

        return origin;
    }

    private static bool FindNavigationParents(IEnumerable<NavigationItem> items, string name, List<NavigationItem> parents)
    {
        foreach (var item in items)
        {
            if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            parents.Add(item);
            if (FindNavigationParents(item.NavigationItems, name, parents))
            {
                return true;
            }

            parents.RemoveAt(parents.Count - 1);
        }

        return false;
    }

    internal static bool HasNavigationItem(Control root, string name)
    {
        var viewModel = root.GetVisualDescendants()
            .OfType<NavigationView>()
            .Select(static view => view.DataContext)
            .OfType<NavigationViewModel>()
            .FirstOrDefault();
        return viewModel is not null
            && FindNavigationParents(viewModel.Clusters, name, []);
    }

    private static async Task MoveCursorAsync(
        Window window,
        CursorOverlay cursor,
        Point origin,
        Point destination,
        string temporaryDirectory,
        int stepIndex,
        List<string> movementFrames,
        bool moveNativePointerDuringMotion = true)
    {
        for (var frame = 0; frame < CursorFrames; frame++)
        {
            var progress = frame / (double)(CursorFrames - 1);
            cursor.Position = new Point(
                origin.X + (destination.X - origin.X) * progress,
                origin.Y + (destination.Y - origin.Y) * progress);
            if (moveNativePointerDuringMotion || frame == CursorFrames - 1)
            {
                window.MouseMove(cursor.Position);
            }
            await WaitForUiAsync();
            movementFrames.Add(CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count));
        }
    }

    private static async Task<Point> ParkYamlPointerAsync(
        Window window,
        CursorOverlay cursor,
        Point origin,
        string temporaryDirectory,
        int stepIndex,
        List<string> movementFrames)
    {
        var destination = new Point(Width - 40, 14);
        if (!origin.Equals(destination))
        {
            await MoveCursorAsync(window, cursor, origin, destination, temporaryDirectory, stepIndex, movementFrames);
        }

        return destination;
    }

    private static async Task ClickAsync(
        Window window,
        CursorOverlay cursor,
        Point destination,
        string temporaryDirectory,
        int stepIndex,
        List<string> movementFrames,
        MouseButton button = MouseButton.Left)
    {
        cursor.IsClicking = true;
        window.MouseDown(destination, button);
        await WaitForUiAsync();
        movementFrames.Add(CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count));
        window.MouseUp(destination, button);
        await WaitForUiAsync();
        movementFrames.Add(CaptureFrame(window, temporaryDirectory, stepIndex, movementFrames.Count));
        cursor.IsClicking = false;
    }

    private static string CaptureFrame(Window window, string directory, int stepIndex, int frameIndex)
    {
        Dispatcher.UIThread.RunJobs();
        var pixelSize = new PixelSize(
            KubeUIWalkthroughIntroView.VideoWidth,
            KubeUIWalkthroughIntroView.VideoHeight);
        using var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        bitmap.Render(window);
        var path = Path.Combine(directory, $"step-{stepIndex:D3}-frame-{frameIndex:D4}.png");
        bitmap.Save(path);
        return path;
    }

    private static void AddHeldFrame(
        Window window,
        string directory,
        int stepIndex,
        List<string> frames,
        int count)
    {
        var frame = CaptureFrame(window, directory, stepIndex, frames.Count);
        for (var index = 0; index < count; index++)
        {
            frames.Add(frame);
        }
    }

    private static async Task CombineAudioAsync(
        IReadOnlyList<StepRecording> recordings,
        string outputPath,
        string directory,
        string ffmpegPath)
    {
        var audioFiles = new List<string>(recordings.Count * 2);
        for (var index = 0; index < recordings.Count; index++)
        {
            var recording = recordings[index];
            audioFiles.Add(recording.AudioPath);
            var openingDuration = Math.Min(OpeningFrameDuration, recording.NarrationDuration);
            var videoDuration = openingDuration + recording.MovementFrames.Count * FrameDuration;
            var silenceDuration = videoDuration - recording.NarrationDuration;
            if (silenceDuration > 0)
            {
                var silencePath = Path.Combine(directory, $"silence-{index:D3}.wav");
                WriteSilence(silencePath, recording.WaveInfo, silenceDuration);
                audioFiles.Add(silencePath);
            }
        }

        var manifest = Path.Combine(directory, "audio-files.txt");
        var lines = audioFiles.Select(path => $"file '{path.Replace("'", "'\\''", StringComparison.Ordinal)}'");
        await File.WriteAllLinesAsync(manifest, lines, TestContext.Current.CancellationToken);
        await RunFfmpegAsync(ffmpegPath,
            ["-y", "-f", "concat", "-safe", "0", "-i", manifest, "-c:a", "pcm_s16le", outputPath]);
    }

    private static void WriteSilence(string path, WaveInfo source, double duration)
    {
        var dataLength = checked((uint)(Math.Floor(duration * source.ByteRate / source.BlockAlign) * source.BlockAlign));
        var formatPadding = source.FormatChunk.Length & 1;
        var dataPadding = dataLength & 1;
        var riffSize = checked((uint)(4 + 8 + source.FormatChunk.Length + formatPadding + 8 + dataLength + dataPadding));
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(riffSize);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write((uint)source.FormatChunk.Length);
        writer.Write(source.FormatChunk);
        if (formatPadding != 0)
        {
            writer.Write((byte)0);
        }

        writer.Write("data"u8);
        writer.Write(dataLength);
        var originalDataLength = dataLength;
        var zeros = new byte[8192];
        while (dataLength > 0)
        {
            var count = (int)Math.Min((uint)zeros.Length, dataLength);
            writer.Write(zeros, 0, count);
            dataLength -= (uint)count;
        }

        if ((originalDataLength & 1) != 0)
        {
            writer.Write((byte)0);
        }
    }

    private static WaveInfo ReadWaveInfo(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (new string(reader.ReadChars(4)) != "RIFF")
        {
            throw new InvalidDataException($"Narration file '{path}' is not a RIFF WAV file.");
        }

        reader.ReadUInt32();
        if (new string(reader.ReadChars(4)) != "WAVE")
        {
            throw new InvalidDataException($"Narration file '{path}' is not a WAVE file.");
        }

        byte[]? formatChunk = null;
        uint byteRate = 0;
        ushort blockAlign = 0;
        uint dataSize = 0;
        while (stream.Position + 8 <= stream.Length)
        {
            var name = new string(reader.ReadChars(4));
            var size = reader.ReadUInt32();
            var chunkStart = stream.Position;
            if (size > int.MaxValue || chunkStart + size > stream.Length)
            {
                throw new InvalidDataException($"Narration file '{path}' contains an invalid WAV chunk.");
            }

            if (name == "fmt " && size >= 16)
            {
                formatChunk = reader.ReadBytes((int)size);
                stream.Position = chunkStart + 8;
                byteRate = reader.ReadUInt32();
                blockAlign = reader.ReadUInt16();
            }
            else if (name == "data")
            {
                dataSize = size;
            }

            stream.Position = chunkStart + size + (size & 1);
        }

        if (formatChunk is null || byteRate == 0 || blockAlign == 0 || dataSize == 0)
        {
            throw new InvalidDataException($"Narration file '{path}' has no valid audio data.");
        }

        return new WaveInfo(formatChunk, byteRate, blockAlign, (double)dataSize / byteRate);
    }

    private static async Task EncodeClipAsync(
        string outputPath,
        string directory,
        IReadOnlyList<StepRecording> recordings,
        string audioPath,
        string ffmpegPath)
    {
        var manifest = Path.Combine(directory, "frames.txt");
        var lines = new List<string>();
        foreach (var recording in recordings)
        {
            var openingDuration = Math.Min(OpeningFrameDuration, recording.NarrationDuration);
            lines.Add($"file '{recording.OpeningFrame.Replace("'", "'\\''", StringComparison.Ordinal)}'");
            lines.Add($"duration {openingDuration.ToString("F3", CultureInfo.InvariantCulture)}");

            foreach (var frame in recording.MovementFrames)
            {
                lines.Add($"file '{frame.Replace("'", "'\\''", StringComparison.Ordinal)}'");
                lines.Add($"duration {FrameDuration.ToString("F6", CultureInfo.InvariantCulture)}");
            }

            var remainingDuration = recording.NarrationDuration
                - openingDuration
                - recording.MovementFrames.Count * FrameDuration;
            if (remainingDuration > 0)
            {
                lines.Add($"file '{recording.FinalFrame.Replace("'", "'\\''", StringComparison.Ordinal)}'");
                lines.Add($"duration {remainingDuration.ToString("F3", CultureInfo.InvariantCulture)}");
            }
        }

        var finalFrame = recordings[^1].FinalFrame;
        lines.Add($"file '{finalFrame.Replace("'", "'\\''", StringComparison.Ordinal)}'");

        await File.WriteAllLinesAsync(manifest, lines, TestContext.Current.CancellationToken);
        var videoArguments = new[]
        {
            "-vf", $"scale={Width}:{Height}:flags=lanczos",
            "-c:v", "libx264", "-preset", "slow", "-crf", "16", "-profile:v", "high", "-level:v", "4.0", "-pix_fmt", "yuv420p",
        };
        var arguments = new List<string>
        {
            "-y", "-f", "concat", "-safe", "0", "-i", manifest, "-i", audioPath,
            "-fps_mode", "cfr", "-r", FramesPerSecond.ToString(CultureInfo.InvariantCulture),
        };
        arguments.AddRange(videoArguments);
        arguments.AddRange(["-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart", "-shortest", outputPath]);
        await RunFfmpegAsync(ffmpegPath,
            arguments);
    }

    private static async Task RunFfmpegAsync(string ffmpegPath, IEnumerable<string> arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        var standardError = await process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg failed: {standardError}");
        }
    }

    private static async Task WaitForUiAsync()
    {
        Dispatcher.UIThread.RunJobs();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.UIThread.Post(static state => ((TaskCompletionSource)state!).TrySetResult(), completion);
        await completion.Task.WaitAsync(TestContext.Current.CancellationToken);
    }

    private static async Task<TControl> WaitForControlAsync<TControl>(Control root, Func<TControl, bool> predicate)
        where TControl : Control
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            await WaitForUiAsync();
            var match = root.GetVisualDescendants().OfType<TControl>().FirstOrDefault(predicate);
            if (match is not null)
            {
                return match;
            }
        }

        throw new TimeoutException($"A {typeof(TControl).Name} matching the requested state did not appear.");
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            await WaitForUiAsync();
            if (condition())
            {
                return;
            }
        }

        throw new TimeoutException("The expected UI action did not complete.");
    }

    private static async Task WaitForConnectionAsync(ClusterWorkspace workspace)
    {
        if (workspace.Runtime.Connected)
        {
            return;
        }

        var connected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var runtime = (System.ComponentModel.INotifyPropertyChanged)workspace.Runtime;
        void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(workspace.Runtime.Connected) && workspace.Runtime.Connected)
            {
                connected.TrySetResult();
            }
        }

        runtime.PropertyChanged += OnPropertyChanged;
        try
        {
            await connected.Task.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
        }
        finally
        {
            runtime.PropertyChanged -= OnPropertyChanged;
        }
    }

    private static void SetRecordingTheme(string theme)
    {
        Application.Current!.RequestedThemeVariant = theme switch
        {
            "light" => ThemeVariant.Light,
            "dark" => ThemeVariant.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, "Supported themes are light and dark."),
        };
    }

    private static string GetFfmpegPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("KUBEUI_FFMPEG_PATH");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            if (!File.Exists(configuredPath))
            {
                throw new FileNotFoundException("The configured FFmpeg executable was not found.", configuredPath);
            }

            return configuredPath;
        }

        var executableName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        var executablePath = Environment.GetEnvironmentVariable("PATH")?
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => Path.Combine(path, executableName))
            .FirstOrDefault(File.Exists);
        return executablePath ?? throw new FileNotFoundException("FFmpeg was not found on PATH. Install it or set KUBEUI_FFMPEG_PATH.");
    }

    private sealed record StepRecording(
        string OpeningFrame,
        IReadOnlyList<string> MovementFrames,
        string FinalFrame,
        string AudioPath,
        double NarrationDuration,
        WaveInfo WaveInfo);

    private sealed record WaveInfo(byte[] FormatChunk, uint ByteRate, ushort BlockAlign, double Duration);

    private sealed class CursorOverlay : Control
    {
        private static readonly StreamGeometry PointerGeometry = CreatePointerGeometry();
        private static readonly IBrush PointerFill = new SolidColorBrush(Color.Parse("#FFD43B"));
        private static readonly IPen PointerOutline = new Pen(new SolidColorBrush(Color.Parse("#151515")), 2);
        private Point _position = new(80, 700);
        private bool _isClicking;

        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                InvalidateVisual();
            }
        }

        public bool IsClicking
        {
            get => _isClicking;
            set
            {
                _isClicking = value;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (IsClicking)
            {
                context.DrawEllipse(null, new Pen(new SolidColorBrush(Color.Parse("#FFFFFF")), 3), Position, 16, 16);
                context.DrawEllipse(new SolidColorBrush(Color.Parse("#FFB000")), null, Position, 9, 9);
            }
            using (context.PushTransform(Matrix.CreateTranslation(Position.X, Position.Y)))
            {
                context.DrawGeometry(PointerFill, PointerOutline, PointerGeometry);
            }
        }

        private static StreamGeometry CreatePointerGeometry()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(1, 1), isFilled: true);
                context.LineTo(new Point(2, 25));
                context.LineTo(new Point(8, 19));
                context.LineTo(new Point(14, 30));
                context.LineTo(new Point(18, 28));
                context.LineTo(new Point(12, 17));
                context.LineTo(new Point(23, 17));
                context.EndFigure(isClosed: true);
            }

            return geometry;
        }
    }

    private sealed class RecordingWindow : Window, IDisposable
    {
        public void Dispose()
        {
            Content = null;
            Close();
        }
    }
}
