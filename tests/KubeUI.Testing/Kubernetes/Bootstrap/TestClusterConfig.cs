using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Testing.Kubernetes.Scenarios;

namespace KubeUI.Testing.Kubernetes.Bootstrap;

public sealed class TestClusterConfig
{
    public KubernetesBackend Type { get; set; } = KubernetesBackend.Fake;

    public string? Name { get; set; }

    public K8SConfiguration? KubeConfig { get; set; }

    public IReadOnlyCollection<IKubernetesObject<V1ObjectMeta>> InitialResources { get; set; } = Array.Empty<IKubernetesObject<V1ObjectMeta>>();

    public IReadOnlyCollection<IKubernetesObject> Resources
    {
        get => InitialResources;
        set => InitialResources = value.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray();
    }

    public string? InitialYaml { get; set; }

    public IReadOnlyCollection<DelegatingHandler> HttpHandlers { get; set; } = Array.Empty<DelegatingHandler>();

    public Action<SocketsHttpHandler>? FirstMessageHandlerSetup { get; set; }

    public TimeSpan ResponseLatency { get; set; } = TimeSpan.FromMilliseconds(50);

    public bool ThrowOnConnect { get; set; }

    public string AuthenticatedUser { get; set; } = "system:admin";

    public static TestClusterConfig Fake(
        IEnumerable<IKubernetesObject>? resources = null,
        IEnumerable<DelegatingHandler>? httpHandlers = null,
        TimeSpan responseLatency = default,
        bool throwOnConnect = false,
        Action<SocketsHttpHandler>? firstMessageHandlerSetup = null)
        => new()
        {
            InitialResources = resources?.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray() ?? Array.Empty<IKubernetesObject<V1ObjectMeta>>(),
            HttpHandlers = httpHandlers?.ToArray() ?? Array.Empty<DelegatingHandler>(),
            ResponseLatency = responseLatency,
            ThrowOnConnect = throwOnConnect,
            FirstMessageHandlerSetup = firstMessageHandlerSetup,
        };

    public static TestClusterConfig Kind(
        string? name = null,
        IEnumerable<IKubernetesObject>? resources = null,
        IEnumerable<DelegatingHandler>? httpHandlers = null,
        TimeSpan responseLatency = default,
        bool throwOnConnect = false,
        Action<SocketsHttpHandler>? firstMessageHandlerSetup = null)
        => new()
        {
            Type = KubernetesBackend.Kind,
            Name = name,
            InitialResources = resources?.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray() ?? Array.Empty<IKubernetesObject<V1ObjectMeta>>(),
            HttpHandlers = httpHandlers?.ToArray() ?? Array.Empty<DelegatingHandler>(),
            ResponseLatency = responseLatency,
            ThrowOnConnect = throwOnConnect,
            FirstMessageHandlerSetup = firstMessageHandlerSetup,
        };

    public static TestClusterConfig Named(
        string name,
        K8SConfiguration kubeConfig,
        IEnumerable<IKubernetesObject>? resources = null,
        IEnumerable<DelegatingHandler>? httpHandlers = null,
        TimeSpan responseLatency = default,
        bool throwOnConnect = false,
        Action<SocketsHttpHandler>? firstMessageHandlerSetup = null)
        => new()
        {
            Type = KubernetesBackend.Kind,
            Name = name,
            KubeConfig = kubeConfig,
            InitialResources = resources?.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray() ?? Array.Empty<IKubernetesObject<V1ObjectMeta>>(),
            HttpHandlers = httpHandlers?.ToArray() ?? Array.Empty<DelegatingHandler>(),
            ResponseLatency = responseLatency,
            ThrowOnConnect = throwOnConnect,
            FirstMessageHandlerSetup = firstMessageHandlerSetup,
        };

    internal TimeSpan EffectiveResponseLatency => ResponseLatency;
}
