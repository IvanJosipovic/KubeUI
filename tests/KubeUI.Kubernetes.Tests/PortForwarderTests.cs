using System.Net;
using System.Net.Sockets;
using System.Text;
using k8s.Models;
using KubeUI.Testing;
using Moq;
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
    public void AddPodPortForward_returns_existing_instance_for_same_target()
    {
        var cluster = new TestClusterRuntime();

        var left = cluster.AddPodPortForward("default", "pod-1", 8080);
        var right = cluster.AddPodPortForward("default", "pod-1", 8080);

        ReferenceEquals(left, right).ShouldBeTrue();
        cluster.PortForwarders.Count.ShouldBe(1);
    }

    [Fact]
    public void AddServicePortForward_returns_existing_instance_for_same_target()
    {
        var cluster = new TestClusterRuntime();

        var left = cluster.AddServicePortForward("default", "prometheus", 9090);
        var right = cluster.AddServicePortForward("default", "prometheus", 9090);

        ReferenceEquals(left, right).ShouldBeTrue();
        cluster.PortForwarders.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Port_forward_uses_mocked_transport_and_copies_client_bytes()
    {
        var cluster = new TestClusterRuntime();
        var stream = new CaptureStream();
        var session = new Mock<IPortForwardSession>(MockBehavior.Strict);
        session.SetupGet(x => x.Stream).Returns(stream);
        session.Setup(x => x.Dispose());

        var factory = new Mock<IPortForwardSessionFactory>(MockBehavior.Strict);
        factory.Setup(x => x.CreateAsync("pod-1", "default", 8080)).ReturnsAsync(session.Object);

        using var sut = new PortForwarder(cluster, "default", localPort: 0, sessionFactory: factory.Object);
        sut.SetPod("pod-1", 8080);
        sut.Start();

        var payload = Encoding.UTF8.GetBytes("ping");

        using (var client = new TcpClient())
        {
            await client.ConnectAsync(IPAddress.Loopback, sut.LocalPort);
            await client.GetStream().WriteAsync(payload);
            client.Client.Shutdown(SocketShutdown.Send);
        }

        await WaitForAsync(() => stream.WrittenBytes.SequenceEqual(payload));
        await WaitForAsync(() => sut.Connections == 0);

        factory.Verify(x => x.CreateAsync("pod-1", "default", 8080), Times.Once);
        session.VerifyGet(x => x.Stream, Times.AtLeastOnce);
        stream.WrittenBytes.ShouldBe(payload);
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
        await WaitForAsync(() => sut.Connections == 0);
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
        await WaitForAsync(() => sut.Connections == 0);
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
        await WaitForAsync(() => sut.Connections == 0);
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

    private sealed class CaptureStream : Stream
    {
        private readonly MemoryStream _written = new();

        public byte[] WrittenBytes => _written.ToArray();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _written.Write(buffer, offset, count);
        }
    }
}
