using System.Text.Json;
using Helm.NET;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Helm;

public sealed class HelmServiceTests
{
    [Fact]
    public async Task Alpha9_operations_forward_search_revision_and_template_version()
    {
        var client = new FakeHelmClient { TemplateResult = "kind: ConfigMap" };
        client.Charts.Add(new("cilium/cilium", "cilium", "1.20.3"));
        using var factory = new FakeHelmClientFactory(client);
        using var service = new HelmService(factory);
        var token = TestContext.Current.CancellationToken;
        var connection = CreateConnection();
        var charts = await service.SearchChartsAsync(connection, "cilium", token);
        charts.ShouldBe(client.Charts);
        client.SearchCalls.ShouldBe(["cilium"]);
        (await service.GetManifestAsync(connection, "cilium", 2, token)).ShouldBe(client.TemplateResult);
        client.GetManifestCalls.ShouldBe([("cilium", "apps", 2)]);
        Dictionary<string, object?> values = new() { ["enabled"] = true };
        (await service.TemplateVersionAsync(connection, "cilium", "cilium/cilium", "1.20.3", values, token))
            .ShouldBe(client.TemplateResult);
        client.TemplateVersion.ShouldBe("1.20.3");
        client.TemplateCalls[0].NamespaceName.ShouldBe("apps");
        client.TemplateCalls[0].Values!.Value.GetProperty("enabled").GetBoolean().ShouldBeTrue();
        client.SearchRepoCalls.ShouldContain(("cilium", true, true));
    }

    [Fact]
    public async Task Alpha9_operations_reject_invalid_names_revisions_and_versions()
    {
        using var factory = new FakeHelmClientFactory(new FakeHelmClient());
        using var service = new HelmService(factory);
        var connection = CreateConnection();
        await Should.ThrowAsync<ArgumentException>(() => service.SearchChartsAsync(connection, " "));
        await Should.ThrowAsync<ArgumentException>(() => service.GetManifestAsync(connection, " "));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => service.GetManifestAsync(connection, "web", -1));
        await Should.ThrowAsync<ArgumentException>(() => service.TemplateVersionAsync(connection, " ", "repo/chart", "1"));
        await Should.ThrowAsync<ArgumentException>(() => service.TemplateVersionAsync(connection, "web", " ", "1"));
        await Should.ThrowAsync<ArgumentException>(() => service.TemplateVersionAsync(connection, "web", "repo/chart", " "));
    }

    [Fact]
    public async Task ListChartsAsync_uses_all_cached_versions_and_filters_exact_repository()
    {
        var client = new FakeHelmClient();
        client.Charts.Add(new("repo-a/web", "web", "2.0.0"));
        client.Charts.Add(new("repo-a/web", "web", "1.0.0"));
        client.Charts.Add(new("repo-ab/web", "web", "3.0.0"));
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var charts = await sut.ListChartsAsync(CreateConnection(), "repo-a", TestContext.Current.CancellationToken);

        charts.Select(chart => chart.Reference).ShouldBe(["repo-a/web", "repo-a/web"]);
        client.SearchRepoCalls.ShouldBe([(string.Empty, true, true)]);
    }

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
        client.AddRepositoryForceUpdateCalls.ShouldBe([true]);
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
        client.ListReleasesCalls.ShouldBe([(null, true, "nginx")]);
    }

    [Fact]
    public async Task ListReleasesAsync_passes_scoped_namespaces()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.ListReleasesAsync(CreateConnection(), allNamespaces: false, cancellationToken: TestContext.Current.CancellationToken);

        client.ListReleasesCalls.ShouldBe([("apps", false, null)]);
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
            cancellationToken: TestContext.Current.CancellationToken,
            version: "1.0.0");

        release.Name.ShouldBe("web");
        client.InstallCalls.Count.ShouldBe(1);
        client.InstallCalls[0].Values!.Value.GetProperty("replicas").GetInt32().ShouldBe(3);
        client.InstallCalls[0].NamespaceName.ShouldBe("apps");
        client.InstallCalls[0].ShouldBe(("web", "bitnami/nginx", "apps", client.InstallCalls[0].Values, true, true, TimeSpan.FromMinutes(5)));
        client.InstallVersions.ShouldBe(["1.0.0"]);
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

        await sut.UpgradeAsync(
            CreateConnection(),
            "web",
            "bitnami/nginx",
            installIfMissing: true,
            cancellationToken: TestContext.Current.CancellationToken,
            version: "1.1.0");

        client.UpgradeCalls.ShouldBe([("web", "bitnami/nginx", "apps", true)]);
        client.UpgradeVersions.ShouldBe(["1.1.0"]);
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
        client.StatusCalls.ShouldBe([("web", "apps")]);
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
        client.HistoryCalls.ShouldBe([("web", "apps")]);
    }

    [Fact]
    public async Task GetValuesAsync_returns_values()
    {
        using var document = JsonDocument.Parse("{\"replicas\":3,\"nested\":{\"enabled\":true}}");
        var client = new FakeHelmClient { Values = document.RootElement.Clone() };
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        var values = await sut.GetValuesAsync(CreateConnection(), "web", TestContext.Current.CancellationToken);

        values["replicas"].GetInt32().ShouldBe(3);
        values["nested"].GetProperty("enabled").GetBoolean().ShouldBeTrue();
        client.GetValuesCalls.ShouldBe([("web", "apps")]);
    }

    [Fact]
    public async Task RollbackAsync_passes_revision()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.RollbackAsync(CreateConnection(), "web", 3, TestContext.Current.CancellationToken, wait: true);

        client.RollbackCalls.ShouldBe([("web", 3, "apps", true)]);
    }

    [Fact]
    public async Task UninstallAsync_passes_release_name()
    {
        var client = new FakeHelmClient();
        using var factory = new FakeHelmClientFactory(client);
        using var sut = CreateService(factory);

        await sut.UninstallAsync(CreateConnection(), "web", TestContext.Current.CancellationToken, wait: true);

        client.UninstallCalls.ShouldBe([("web", "apps", true)]);
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
        client.TemplateCalls.Count.ShouldBe(1);
        client.TemplateCalls[0].NamespaceName.ShouldBe("apps");
        client.TemplateCalls[0].Values!.Value.GetProperty("image").GetString().ShouldBe("nginx:1.27");
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
