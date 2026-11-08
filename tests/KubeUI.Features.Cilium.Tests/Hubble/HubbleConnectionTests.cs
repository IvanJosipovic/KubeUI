using KubeUI.Features.Cilium.Hubble;
using KubeUI.Kubernetes;
using KubeUI.Testing.Kubernetes.Bootstrap;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Features.Cilium.Tests.Hubble;

public sealed class HubbleConnectionTests
{
    [Fact]
    [Obsolete]
    public async Task Observe_flows_rejects_an_empty_namespace_scope()
    {
        await using var cluster = await CreateNamedRuntimeAsync();

        await using var connection = new HubbleConnection(cluster.Runtime, NullLogger<HubbleConnection>.Instance);

        Should.Throw<ArgumentException>(() => connection.ObserveFlows([]));
        cluster.Runtime.Connected.ShouldBeFalse();
    }

    [Fact]
    [Obsolete]
    public async Task Start_rejects_empty_relay_namespace_before_cluster_access()
    {
        await using var cluster = await CreateNamedRuntimeAsync();
        await using var connection = new HubbleConnection(
            cluster.Runtime,
            NullLogger<HubbleConnection>.Instance,
            new HubbleConnectionOptions { Namespace = " " });
        var statuses = new List<ConnectionStatus>();
        connection.StatusChanged += statuses.Add;

        await Should.ThrowAsync<InvalidOperationException>(() => connection.StartAsync());

        statuses.ShouldBe([ConnectionStatus.Connecting, ConnectionStatus.Error]);
        cluster.Runtime.Connected.ShouldBeFalse();
    }

    [Fact]
    [Obsolete]
    public async Task Start_rejects_incomplete_mutual_tls_credentials()
    {
        await using var cluster = await CreateNamedRuntimeAsync();
        await using var connection = new HubbleConnection(
            cluster.Runtime,
            NullLogger<HubbleConnection>.Instance,
            new HubbleConnectionOptions { ClientCertificatePem = "certificate" });

        await Should.ThrowAsync<InvalidOperationException>(() => connection.StartAsync());

        cluster.Runtime.Connected.ShouldBeFalse();
    }

    private static async Task<NamedRuntime> CreateNamedRuntimeAsync()
    {
        var generator = new TestClusterGenerator();
        try
        {
            var config = TestClusterConfig.Fake();
            config.Name = "dev";
            var cluster = await generator.CreateAsync(config, TestContext.Current.CancellationToken);
            return new NamedRuntime(cluster.Cluster, generator);
        }
        catch
        {
            await generator.ResetAsync();
            throw;
        }
    }

    private sealed record NamedRuntime(IClusterRuntime Runtime, TestClusterGenerator Generator) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await Generator.ResetAsync();
    }
}
