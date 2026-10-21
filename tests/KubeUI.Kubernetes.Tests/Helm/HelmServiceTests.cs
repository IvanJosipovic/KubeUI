using System.Text.Json;
using Helm.NET;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Helm;

public sealed class HelmServiceTests
{
    private static HelmConnection CreateConnection() => new("/home/user/.kube/config", "kind-test", "apps");

    private static HelmService CreateService(FakeHelmClientFactory factory) => new(factory);

    [Fact]
    public async Task ListRepositoriesAsync_returns_repositories_from_client()
    {
        var client = new FakeHelmClient();
        client.Repositories.Add(new HelmRepository("bitnami", "https://charts.bitnami.com/bitnami"));
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var repositories = await sut.ListRepositoriesAsync(CreateConnection(), TestContext.Current.CancellationToken);

        repositories.ShouldBe([new HelmRepository("bitnami", "https://charts.bitnami.com/bitnami")]);
        factory.Connections.ShouldBe([CreateConnection()]);
    }

    [Fact]
    public async Task AddRepositoryAsync_passes_name_url_and_credentials()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.AddRepositoryAsync(CreateConnection(), "internal", "https://charts.internal/index.yaml", "user", "secret", TestContext.Current.CancellationToken);

        client.AddRepositoryCalls.ShouldBe([("internal", "https://charts.internal/index.yaml", "user", "secret")]);
    }

    [Fact]
    public async Task AddRepositoryAsync_passes_null_credentials_when_not_supplied()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.AddRepositoryAsync(CreateConnection(), "public", "https://charts.public.example", cancellationToken: TestContext.Current.CancellationToken);

        client.AddRepositoryCalls.ShouldBe([("public", "https://charts.public.example", null, null)]);
    }

    [Fact]
    public async Task RemoveRepositoryAsync_passes_name()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.RemoveRepositoryAsync(CreateConnection(), "internal", TestContext.Current.CancellationToken);

        client.RemoveRepositoryCalls.ShouldBe(["internal"]);
    }

    [Fact]
    public async Task UpdateRepositoriesAsync_invokes_client()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.UpdateRepositoriesAsync(CreateConnection(), TestContext.Current.CancellationToken);

        client.UpdateRepositoriesCalls.ShouldBe(1);
    }

    [Fact]
    public async Task ListReleasesAsync_passes_all_namespaces_and_filter()
    {
        var client = new FakeHelmClient();
        client.Releases.Add(new HelmRelease("nginx", "apps", "1", "2026-01-01", "deployed", "nginx-1.0.0", "1.0.0"));
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var releases = await sut.ListReleasesAsync(CreateConnection(), allNamespaces: true, "nginx", TestContext.Current.CancellationToken);

        releases.Count.ShouldBe(1);
        client.ListReleasesCalls.ShouldBe([(true, "nginx")]);
    }

    [Fact]
    public async Task ListReleasesAsync_passes_scoped_namespaces()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.ListReleasesAsync(CreateConnection(), allNamespaces: false, cancellationToken: TestContext.Current.CancellationToken);

        client.ListReleasesCalls.ShouldBe([(false, null)]);
    }

    [Fact]
    public async Task InstallAsync_passes_chart_values_and_flags()
    {
        var client = new FakeHelmClient
        {
            InstallRelease = new HelmRelease("web", "apps", "1", "2026-01-01", "deployed", "nginx-1.0.0", "1.0.0"),
        };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);
        Dictionary<string, object?> values = new() { ["replicas"] = 3 };

        var release = await sut.InstallAsync(
            CreateConnection(),
            "web",
            "bitnami/nginx",
            values,
            createNamespace: true,
            wait: true,
            timeout: TimeSpan.FromMinutes(5),
            cancellationToken: TestContext.Current.CancellationToken);

        release.Name.ShouldBe("web");
        client.InstallCalls.ShouldBe([("web", "bitnami/nginx", values, true, true, TimeSpan.FromMinutes(5))]);
    }

    [Fact]
    public async Task UpgradeAsync_passes_install_if_missing()
    {
        var client = new FakeHelmClient
        {
            UpgradeRelease = new HelmRelease("web", "apps", "2", "2026-01-02", "deployed", "nginx-1.1.0", "1.1.0"),
        };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.UpgradeAsync(CreateConnection(), "web", "bitnami/nginx", installIfMissing: true, cancellationToken: TestContext.Current.CancellationToken);

        client.UpgradeCalls.ShouldBe([("web", "bitnami/nginx", true)]);
    }

    [Fact]
    public async Task StatusAsync_returns_release()
    {
        var client = new FakeHelmClient
        {
            StatusRelease = new HelmRelease("web", "apps", "2", "2026-01-02", "deployed", "nginx-1.1.0", "1.1.0"),
        };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var release = await sut.StatusAsync(CreateConnection(), "web", TestContext.Current.CancellationToken);

        release.Chart.ShouldBe("nginx-1.1.0");
        client.StatusCalls.ShouldBe(["web"]);
    }

    [Fact]
    public async Task HistoryAsync_returns_revisions()
    {
        var client = new FakeHelmClient();
        client.HistoryReleases.Add(new HelmRelease("web", "apps", "1", "2026-01-01", "superseded", "nginx-1.0.0", "1.0.0"));
        client.HistoryReleases.Add(new HelmRelease("web", "apps", "2", "2026-01-02", "deployed", "nginx-1.1.0", "1.1.0"));
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var history = await sut.HistoryAsync(CreateConnection(), "web", TestContext.Current.CancellationToken);

        history.Count.ShouldBe(2);
        client.HistoryCalls.ShouldBe(["web"]);
    }

    [Fact]
    public async Task GetValuesAsync_returns_values()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var values = await sut.GetValuesAsync(CreateConnection(), "web", TestContext.Current.CancellationToken);

        values.Count.ShouldBe(0);
        client.GetValuesCalls.ShouldBe(["web"]);
    }

    [Fact]
    public async Task RollbackAsync_passes_revision()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.RollbackAsync(CreateConnection(), "web", 3, TestContext.Current.CancellationToken, wait: true);

        client.RollbackCalls.ShouldBe([("web", 3, true)]);
    }

    [Fact]
    public async Task UninstallAsync_passes_release_name()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.UninstallAsync(CreateConnection(), "web", TestContext.Current.CancellationToken, wait: true);

        client.UninstallCalls.ShouldBe(["web"]);
        client.UninstallWaitCalls.ShouldBe([true]);
    }

    [Fact]
    public async Task UninstallAsync_propagates_helm_action_exception()
    {
        var client = new FakeHelmClient { ThrowOnUninstall = true };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await Should.ThrowAsync<HelmActionException>(() => sut.UninstallAsync(CreateConnection(), "web", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Operation_with_cancelled_token_does_not_create_client()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            sut.ListRepositoriesAsync(CreateConnection(), cancellation.Token));

        factory.Connections.ShouldBeEmpty();
    }

    [Fact]
    public async Task InstallAsync_rejects_empty_release_name()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await Should.ThrowAsync<ArgumentException>(() =>
            sut.InstallAsync(CreateConnection(), " ", "bitnami/nginx", cancellationToken: TestContext.Current.CancellationToken));

        factory.Connections.ShouldBeEmpty();
    }

    [Fact]
    public async Task TemplateAsync_passes_values_and_returns_manifest()
    {
        var client = new FakeHelmClient { TemplateResult = "kind: Deployment" };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);
        Dictionary<string, object?> values = new() { ["image"] = "nginx:1.27" };

        var manifest = await sut.TemplateAsync(CreateConnection(), "web", "bitnami/nginx", values, TestContext.Current.CancellationToken);

        manifest.ShouldBe("kind: Deployment");
        client.TemplateCalls.ShouldBe([("web", "bitnami/nginx", values)]);
    }

    [Fact]
    public async Task Native_operations_for_different_connections_are_serialized()
    {
        var client = new FakeHelmClient { BlockingEntries = 1 };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);
        var connection = CreateConnection();

        var first = sut.ListRepositoriesAsync(connection, TestContext.Current.CancellationToken);
        await client.FirstEntry.Task.WaitAsync(TestContext.Current.CancellationToken);

        var second = sut.ListRepositoriesAsync(
            new HelmConnection("/other/config", "other-context", "system"),
            TestContext.Current.CancellationToken);
        await factory.SecondCreated.Task.WaitAsync(TestContext.Current.CancellationToken);

        second.IsCompleted.ShouldBeFalse();

        client.Release.SetResult();
        var firstResult = await first;
        var secondResult = await second;

        firstResult.Count.ShouldBe(0);
        secondResult.Count.ShouldBe(0);
        client.MaxConcurrent.ShouldBe(1);
        client.EntryCount.ShouldBe(2);
    }
}
