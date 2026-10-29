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
    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(true, true, true, true)]
    public void settings_provider_resolves_preview_feature_values(bool helm, bool hubble, bool disableJsonReflection, bool disableYamlReflection)
    {
        var settings = new AppSettings
        {
            PreviewFeatures = new FeatureSettings
            {
                Helm = helm,
                Hubble = hubble,
                DisableJsonSerializationReflection = disableJsonReflection,
                DisableYamlSerializationReflection = disableYamlReflection
            }
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var client = CreateClient(new SettingsFeatureProvider(settingsService.Object));

        client.GetBooleanValue(FeatureFlagKeys.Helm, !helm).ShouldBe(helm);
        client.GetBooleanValue(FeatureFlagKeys.Hubble, !hubble).ShouldBe(hubble);
        client.GetBooleanValue(FeatureFlagKeys.DisableJsonSerializationReflection, !disableJsonReflection).ShouldBe(disableJsonReflection);
        client.GetBooleanValue(FeatureFlagKeys.DisableYamlSerializationReflection, !disableYamlReflection).ShouldBe(disableYamlReflection);

        var details = client.GetBooleanDetails(FeatureFlagKeys.Helm, false);
        details.FlagKey.ShouldBe(FeatureFlagKeys.Helm);
        details.Reason.ShouldBe(FeatureReason.Static);
        details.Variant.ShouldBe(helm ? "on" : "off");
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
        client.GetBooleanValue(FeatureFlagKeys.DisableJsonSerializationReflection, true).ShouldBeFalse();
        client.GetBooleanValue(FeatureFlagKeys.DisableYamlSerializationReflection, true).ShouldBeFalse();
    }

    [Fact]
    public void settings_provider_resolves_flags_when_features_is_explicitly_null()
    {
        var settings = new AppSettings { PreviewFeatures = null };
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
