using System.Net;
using System.Net.Sockets;
using k8s.Models;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class PortForwarderTests
{
    [Fact]
    public void Start_activates_listener_and_assigns_local_port()
    {
        var cluster = new TestClusterRuntime();
        using var sut = new PortForwarder(cluster, "default");
        sut.SetPod("pod-1", 8080);

        sut.Start();

        sut.Status.ShouldBe("Active");
        sut.LocalPort.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Start_with_busy_port_sets_busy_status()
    {
        var cluster = new TestClusterRuntime();
        using var busyListener = new TcpListener(IPAddress.Loopback, 0);
        busyListener.Start();
        var busyPort = ((IPEndPoint)busyListener.LocalEndpoint).Port;

        using var sut = new PortForwarder(cluster, "default", busyPort);
        sut.SetPod("pod-1", 8080);

        sut.Start();

        sut.Status.ShouldBe("Local port is busy");
        sut.LocalPort.ShouldBe(busyPort);
    }

    [Fact]
    public async Task Stop_marks_inactive_and_stops_accepting_connections()
    {
        var cluster = new TestClusterRuntime();
        using var sut = new PortForwarder(cluster, "default");
        sut.SetPod("pod-1", 8080);
        sut.Start();
        var localPort = sut.LocalPort;

        sut.Stop();

        sut.Status.ShouldBe("Inactive");
        await Should.ThrowAsync<SocketException>(async () =>
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, localPort);
        });
    }

    [Fact]
    public async Task Dispose_marks_inactive_and_stops_accepting_connections()
    {
        var cluster = new TestClusterRuntime();
        var sut = new PortForwarder(cluster, "default");
        sut.SetPod("pod-1", 8080);
        sut.Start();
        var localPort = sut.LocalPort;

        sut.Dispose();

        sut.Status.ShouldBe("Inactive");
        await Should.ThrowAsync<SocketException>(async () =>
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, localPort);
        });
    }

    [Fact]
    public void Equals_returns_true_for_same_target()
    {
        var cluster = new TestClusterRuntime();
        using var left = new PortForwarder(cluster, "default");
        using var right = new PortForwarder(cluster, "default");
        left.SetService("prometheus", 9090);
        right.SetService("prometheus", 9090);

        left.Equals(right).ShouldBeTrue();
    }

    [Fact]
    public void Equals_returns_false_for_different_target()
    {
        var cluster = new TestClusterRuntime();
        using var left = new PortForwarder(cluster, "default");
        using var right = new PortForwarder(cluster, "default");
        left.SetService("prometheus", 9090);
        right.SetService("grafana", 9090);

        left.Equals(right).ShouldBeFalse();
    }

    [Fact]
    public async Task Service_forward_without_endpoint_slice_sets_expected_status()
    {
        var cluster = new TestClusterRuntime();
        await cluster.AddOrUpdateResource(CreateService());

        using var sut = new PortForwarder(cluster, "default");
        sut.SetService("prometheus", 9090);
        sut.Start();

        await ConnectAsync(sut.LocalPort);

        await WaitForAsync(() => sut.Status == "No endpoint slices found for Service");
        sut.Connections.ShouldBe(0);
    }

    [Fact]
    public async Task Service_forward_without_matching_port_sets_expected_status()
    {
        var cluster = new TestClusterRuntime();
        await cluster.AddOrUpdateResource(CreateService());
        await cluster.AddOrUpdateResource(new V1EndpointSlice
        {
            Metadata = new V1ObjectMeta
            {
                Name = "prometheus-slice",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string>
                {
                    ["kubernetes.io/service-name"] = "prometheus",
                },
            },
            Ports =
            [
                new Discoveryv1EndpointPort
                {
                    Name = "metrics",
                    Port = 9090,
                },
            ],
        });

        using var sut = new PortForwarder(cluster, "default");
        sut.SetService("prometheus", 9090);
        sut.Start();

        await ConnectAsync(sut.LocalPort);

        await WaitForAsync(() => sut.Status == "No port found for Service");
        sut.Connections.ShouldBe(0);
    }

    [Fact]
    public async Task Service_forward_without_ready_pod_sets_expected_status()
    {
        var cluster = new TestClusterRuntime();
        await cluster.AddOrUpdateResource(CreateService());
        await cluster.AddOrUpdateResource(new V1EndpointSlice
        {
            Metadata = new V1ObjectMeta
            {
                Name = "prometheus-slice",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string>
                {
                    ["kubernetes.io/service-name"] = "prometheus",
                },
            },
            Ports =
            [
                new Discoveryv1EndpointPort
                {
                    Name = "http",
                    Port = 9090,
                },
            ],
            Endpoints =
            [
                new V1Endpoint
                {
                    Conditions = new V1EndpointConditions
                    {
                        Ready = false,
                        Serving = false,
                    },
                    TargetRef = new V1ObjectReference
                    {
                        Kind = "Pod",
                        Name = "prometheus-0",
                    },
                },
            ],
        });

        using var sut = new PortForwarder(cluster, "default");
        sut.SetService("prometheus", 9090);
        sut.Start();

        await ConnectAsync(sut.LocalPort);

        await WaitForAsync(() => sut.Status == "No pods found for Service");
        sut.Connections.ShouldBe(0);
    }

    private static V1Service CreateService()
    {
        return new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = "prometheus",
                NamespaceProperty = "default",
            },
            Spec = new V1ServiceSpec
            {
                Ports =
                [
                    new V1ServicePort
                    {
                        Name = "http",
                        Port = 9090,
                    },
                ],
            },
        };
    }

    private static async Task ConnectAsync(int port)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        predicate().ShouldBeTrue();
    }
}
