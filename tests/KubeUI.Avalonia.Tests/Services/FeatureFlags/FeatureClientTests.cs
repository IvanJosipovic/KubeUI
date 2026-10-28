using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.FeatureFlags;
using KubeUI.Avalonia.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Tests.Services.FeatureFlags;

public sealed class FeatureClientTests
{
    [Fact]
    public void settings_provider_resolves_preview_feature_values()
    {
        var settings = new AppSettings
        {
            Features = new FeatureSettings
            {
                Helm = true,
                Hubble = false,
                JsonAllowReflection = true,
                YamlAllowReflection = false
            }
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var client = CreateClient(new SettingsFeatureProvider(settingsService.Object));

        client.GetBooleanValue(FeatureFlagKeys.Helm, false).ShouldBeTrue();
        client.GetBooleanValue(FeatureFlagKeys.Hubble, true).ShouldBeFalse();
        client.GetBooleanValue(FeatureFlagKeys.JsonAllowReflection, false).ShouldBeTrue();
        client.GetBooleanValue(FeatureFlagKeys.YamlAllowReflection, true).ShouldBeFalse();

        var details = client.GetBooleanDetails(FeatureFlagKeys.Helm, false);
        details.FlagKey.ShouldBe(FeatureFlagKeys.Helm);
        details.Reason.ShouldBe(FeatureReason.Static);
        details.Variant.ShouldBe("on");
        details.ErrorType.ShouldBe(FeatureErrorType.None);
    }

    [Fact]
    public void settings_provider_defaults_preview_features_to_disabled()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(new AppSettings());
        var client = CreateClient(new SettingsFeatureProvider(settingsService.Object));

        client.GetBooleanValue(FeatureFlagKeys.Helm, true).ShouldBeFalse();
        client.GetBooleanValue(FeatureFlagKeys.Hubble, true).ShouldBeFalse();
        client.GetBooleanValue(FeatureFlagKeys.JsonAllowReflection, true).ShouldBeFalse();
        client.GetBooleanValue(FeatureFlagKeys.YamlAllowReflection, true).ShouldBeFalse();
    }

    [Fact]
    public void settings_provider_resolves_flags_when_features_is_explicitly_null()
    {
        var settings = new AppSettings { Features = null };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var client = CreateClient(new SettingsFeatureProvider(settingsService.Object));

        client.GetBooleanValue(FeatureFlagKeys.Helm, true).ShouldBeFalse();
        client.GetBooleanValue(FeatureFlagKeys.Hubble, false).ShouldBeFalse();
    }

    [Fact]
    public void settings_provider_returns_unknown_flag_default()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(new AppSettings());
        var client = CreateClient(new SettingsFeatureProvider(settingsService.Object));

        var details = client.GetBooleanDetails("missing.flag", true);

        details.Value.ShouldBeTrue();
        details.ErrorType.ShouldBe(FeatureErrorType.FlagNotFound);
        details.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void client_fails_closed_when_provider_throws()
    {
        var provider = new Mock<IFeatureProvider>();
        provider.SetupGet(value => value.Metadata).Returns(new FeatureProviderMetadata("test"));
        provider
            .Setup(value => value.ResolveBooleanValue("broken", true, It.IsAny<EvaluationContext?>()))
            .Throws(new InvalidOperationException("provider failure"));
        var client = CreateClient(provider.Object);

        var details = client.GetBooleanDetails("broken", true);

        details.Value.ShouldBeTrue();
        details.ErrorType.ShouldBe(FeatureErrorType.General);
        details.Reason.ShouldBe(FeatureReason.Error);
        details.ErrorMessage.ShouldBe("provider failure");
    }

    [Fact]
    public void client_rejects_empty_flag_keys()
    {
        var provider = new Mock<IFeatureProvider>();
        var client = CreateClient(provider.Object);

        var details = client.GetBooleanDetails(" ", true);

        details.Value.ShouldBeTrue();
        details.ErrorType.ShouldBe(FeatureErrorType.FlagNotFound);
        provider.Verify(
            value => value.ResolveBooleanValue(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<EvaluationContext?>()),
            Times.Never);
    }

    [Fact]
    public void client_passes_evaluation_context_to_the_provider()
    {
        var context = new EvaluationContext("user", new Dictionary<string, object> { ["plan"] = "pro" });
        var provider = new Mock<IFeatureProvider>();
        provider.SetupGet(value => value.Metadata).Returns(new FeatureProviderMetadata("test"));
        provider
            .Setup(value => value.ResolveBooleanValue("flag", false, context))
            .Returns(new ResolutionDetails<bool>("flag", true, Reason: FeatureReason.Static));
        var client = CreateClient(provider.Object);

        client.GetBooleanValue("flag", false, context).ShouldBeTrue();

        provider.Verify(
            value => value.ResolveBooleanValue("flag", false, context),
            Times.Once);
    }

    private static FeatureClient CreateClient(IFeatureProvider provider)
        => new(provider, NullLogger<FeatureClient>.Instance);
}
