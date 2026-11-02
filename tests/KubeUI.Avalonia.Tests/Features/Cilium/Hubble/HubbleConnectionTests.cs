using KubeUI.Cilium.Hubble;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class HubbleConnectionTests
{
    [Fact]
    public async Task Observe_flows_rejects_an_empty_namespace_scope()
    {
        var cluster = new Mock<IClusterRuntime>(MockBehavior.Strict);
        await using var connection = new HubbleConnection(cluster.Object, NullLogger<HubbleConnection>.Instance);

        Should.Throw<ArgumentException>(() => connection.ObserveFlows([]));
        cluster.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Start_rejects_empty_relay_namespace_before_cluster_access()
    {
        var cluster = new Mock<IClusterRuntime>(MockBehavior.Strict);
        await using var connection = new HubbleConnection(
            cluster.Object,
            NullLogger<HubbleConnection>.Instance,
            new HubbleConnectionOptions { Namespace = " " });
        var statuses = new List<ConnectionStatus>();
        connection.StatusChanged += statuses.Add;

        await Should.ThrowAsync<InvalidOperationException>(() => connection.StartAsync());

        statuses.ShouldBe([ConnectionStatus.Connecting, ConnectionStatus.Error]);
        cluster.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Start_rejects_incomplete_mutual_tls_credentials()
    {
        var cluster = new Mock<IClusterRuntime>(MockBehavior.Strict);
        await using var connection = new HubbleConnection(
            cluster.Object,
            NullLogger<HubbleConnection>.Instance,
            new HubbleConnectionOptions { ClientCertificatePem = "certificate" });

        await Should.ThrowAsync<InvalidOperationException>(() => connection.StartAsync());

        cluster.VerifyNoOtherCalls();
    }
}
