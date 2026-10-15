using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpCredentialServiceTests
{
    [Fact]
    public void external_credentials_store_verifier_and_enforce_cluster_grants()
    {
        var settings = new Settings();
        var settingsService = CreateSettingsService(settings);
        var credentials = new McpCredentialService(settingsService.Object);

        var created = credentials.CreateExternalCredential("Codex", ["dev"]);

        created.Token.ShouldStartWith("kui_");
        created.Grant.TokenVerifier.ShouldNotBe(created.Token);
        settings.McpExternalClientGrants.ShouldContain(created.Grant);
        var identity = credentials.Authenticate(created.Token);
        identity.ShouldNotBeNull();
        identity!.Id.ShouldBe(created.Grant.Id);
        identity.Name.ShouldBe("Codex");
        identity.IsInternal.ShouldBeFalse();
        identity.AllowedClusters.ShouldBe(new HashSet<string>(["dev"], StringComparer.Ordinal));
        credentials.Authenticate("wrong-token").ShouldBeNull();
        created.ToString().ShouldNotContain(created.Token);
        created.Grant.ToString().ShouldNotContain(created.Grant.TokenVerifier);
        settingsService.Verify(service => service.TrySaveSettings(), Times.Once);
    }

    [Fact]
    public void revoked_external_credentials_stop_authenticating()
    {
        var settingsService = CreateSettingsService(new Settings());
        var credentials = new McpCredentialService(settingsService.Object);
        var created = credentials.CreateExternalCredential("Claude", ["prod"]);

        credentials.RevokeExternalCredential(created.Grant.Id).ShouldBeTrue();

        credentials.Authenticate(created.Token).ShouldBeNull();
        credentials.RevokeExternalCredential(created.Grant.Id).ShouldBeFalse();
    }

    [Fact]
    public void external_client_names_and_cluster_scopes_are_normalized()
    {
        var credentials = new McpCredentialService(CreateSettingsService(new Settings()).Object);

        var created = credentials.CreateExternalCredential("  host tool  ", [" dev ", "", "dev", "prod"]);

        created.Grant.Name.ShouldBe("host tool");
        created.Grant.AllowedClusters.ShouldBe(["dev", "prod"]);
    }

    [Fact]
    public void authenticate_skips_bad_verifiers_and_rejects_empty_or_oversized_tokens()
    {
        var settings = new Settings();
        settings.McpExternalClientGrants.Add(new McpExternalClientGrant(
            "short", "short verifier", "AA", DateTimeOffset.UtcNow, ["dev"]));
        settings.McpExternalClientGrants.Add(new McpExternalClientGrant(
            "malformed", "malformed verifier", "not-hex", DateTimeOffset.UtcNow, ["dev"]));
        var credentials = new McpCredentialService(CreateSettingsService(settings).Object);
        var created = credentials.CreateExternalCredential("valid", ["dev"]);

        credentials.Authenticate(created.Token)!.Id.ShouldBe(created.Grant.Id);
        credentials.Authenticate(string.Empty).ShouldBeNull();
        credentials.Authenticate(new string('x', 129)).ShouldBeNull();
    }

    [Fact]
    public void revoking_a_later_grant_skips_prior_entries()
    {
        var credentials = new McpCredentialService(CreateSettingsService(new Settings()).Object);
        credentials.CreateExternalCredential("first", ["dev"]);
        var second = credentials.CreateExternalCredential("second", ["prod"]);

        credentials.RevokeExternalCredential(second.Grant.Id).ShouldBeTrue();

        credentials.Authenticate(second.Token).ShouldBeNull();
    }

    [Fact]
    public void session_credentials_are_memory_only_and_revoked_when_disposed()
    {
        var settings = new Settings();
        var credentials = new McpCredentialService(CreateSettingsService(settings).Object);
        using var lease = credentials.CreateSessionCredential();

        var identity = credentials.Authenticate(lease.Token);
        identity.ShouldNotBeNull();
        identity!.IsInternal.ShouldBeTrue();
        identity.AllowedClusters.ShouldBeEmpty();
        settings.McpExternalClientGrants.ShouldBeEmpty();
        lease.ToString().ShouldNotContain(lease.Token);

        lease.Dispose();
        credentials.Authenticate(lease.Token).ShouldBeNull();
    }

    [Fact]
    public void empty_external_cluster_grant_means_all_clusters()
    {
        var credentials = new McpCredentialService(CreateSettingsService(new Settings()).Object);

        var created = credentials.CreateExternalCredential("tool", []);

        created.Grant.AllowedClusters.ShouldBeEmpty();
        credentials.Authenticate(created.Token)!.AllowedClusters.ShouldBeEmpty();
    }

    [Fact]
    public void external_credential_creation_rejects_blank_and_overlong_client_names()
    {
        var credentials = new McpCredentialService(CreateSettingsService(new Settings()).Object);

        Should.Throw<ArgumentException>(() => credentials.CreateExternalCredential("   ", ["dev"]));
        Should.Throw<ArgumentException>(() => credentials.CreateExternalCredential(new string('x', 81), ["dev"]));
    }

    [Fact]
    public void credential_creation_fails_without_persisting_when_settings_storage_fails()
    {
        var settings = new Settings();
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(false);
        var credentials = new McpCredentialService(settingsService.Object);

        Should.Throw<InvalidOperationException>(() => credentials.CreateExternalCredential("Codex", ["dev"]));

        settings.McpExternalClientGrants.ShouldBeEmpty();
    }

    [Fact]
    public void failed_external_revocation_restores_the_grant_and_token()
    {
        var settings = new Settings();
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.SetupSequence(service => service.TrySaveSettings()).Returns(true).Returns(false);
        var credentials = new McpCredentialService(settingsService.Object);
        var created = credentials.CreateExternalCredential("host tool", ["dev"]);

        Should.Throw<InvalidOperationException>(() => credentials.RevokeExternalCredential(created.Grant.Id));

        settings.McpExternalClientGrants.ShouldContain(created.Grant);
        credentials.Authenticate(created.Token).ShouldNotBeNull();
    }

    [Fact]
    public void external_credential_count_is_bounded()
    {
        var settings = new Settings();
        var credentials = new McpCredentialService(CreateSettingsService(settings).Object);

        for (var index = 0; index < 100; index++)
            credentials.CreateExternalCredential($"host-{index}", ["dev"]);

        Should.Throw<InvalidOperationException>(() => credentials.CreateExternalCredential("host-over-limit", ["dev"]));
        settings.McpExternalClientGrants.Count.ShouldBe(100);
    }

    [Fact]
    public void active_session_credential_count_is_bounded_and_reusable_after_disposal()
    {
        var credentials = new McpCredentialService(CreateSettingsService(new Settings()).Object);
        var leases = new List<McpSessionCredential>();
        try
        {
            for (var index = 0; index < 128; index++)
                leases.Add(credentials.CreateSessionCredential());

            Should.Throw<InvalidOperationException>(() => credentials.CreateSessionCredential());
            var firstToken = leases[0].Token;
            leases[0].Dispose();
            leases.Add(credentials.CreateSessionCredential());
            credentials.Authenticate(firstToken).ShouldBeNull();
        }
        finally
        {
            foreach (var lease in leases)
                lease.Dispose();
        }
    }

    [Fact]
    public void malformed_persisted_verifiers_are_ignored()
    {
        var settings = new Settings();
        settings.McpExternalClientGrants.Add(new McpExternalClientGrant(
            "bad", "bad verifier", "not-hex", DateTimeOffset.UtcNow, ["dev"]));
        var credentials = new McpCredentialService(CreateSettingsService(settings).Object);

        Should.NotThrow(() => credentials.Authenticate("unknown token"));
        credentials.Authenticate("unknown token").ShouldBeNull();
    }

    private static Mock<ISettingsService> CreateSettingsService(Settings settings)
    {
        var service = new Mock<ISettingsService>();
        service.SetupGet(settingsService => settingsService.Settings).Returns(settings);
        service.Setup(settingsService => settingsService.TrySaveSettings()).Returns(true);
        return service;
    }
}
