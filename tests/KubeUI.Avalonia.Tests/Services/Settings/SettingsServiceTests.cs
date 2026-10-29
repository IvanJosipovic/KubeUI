using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Tests.Services.Settings;

public sealed class SettingsServiceTests
{
    [Fact]
    public void changing_nested_preview_feature_flag_saves_settings()
    {
        var settings = new AppSettings();
        var service = CreateService(settings);
        _ = service.Settings;

        settings.PreviewFeatures.Helm = true;

        service.SaveCount.ShouldBe(1);
    }

    [Fact]
    public void changing_nested_source_generation_flags_saves_settings()
    {
        var settings = new AppSettings();
        var service = CreateService(settings);
        _ = service.Settings;

        settings.PreviewFeatures.DisableJsonSerializationReflection = true;
        settings.PreviewFeatures.DisableYamlSerializationReflection = true;

        service.SaveCount.ShouldBe(2);
    }

    [Fact]
    public void replacing_preview_features_hooks_the_new_instance()
    {
        var settings = new AppSettings();
        var service = CreateService(settings);
        _ = service.Settings;
        var original = settings.PreviewFeatures;
        var replacement = new FeatureSettings();

        settings.PreviewFeatures = replacement;
        replacement.Hubble = true;
        original.Helm = true;

        service.SaveCount.ShouldBe(2);
    }

    [Fact]
    public void replacing_settings_hooks_the_new_feature_instance()
    {
        var originalSettings = new AppSettings();
        var service = CreateService(originalSettings);
        _ = service.Settings;
        var replacementSettings = new AppSettings();

        service.Settings = replacementSettings;
        replacementSettings.PreviewFeatures.Helm = true;
        originalSettings.PreviewFeatures.Hubble = true;

        service.SaveCount.ShouldBe(2);
    }

    private static RecordingSettingsService CreateService(AppSettings settings)
    {
        var persistence = new Mock<ISettingsPersistence>();
        persistence
            .Setup(value => value.Load())
            .Returns(() => new SettingsPersistenceData { Settings = settings });
        return new RecordingSettingsService(persistence.Object);
    }

    private sealed class RecordingSettingsService : SettingsService
    {
        public RecordingSettingsService(ISettingsPersistence persistence)
            : base(NullLogger<SettingsService>.Instance, persistence)
        {
        }

        public int SaveCount { get; private set; }

        public override void SaveSettings()
        {
            SaveCount++;
        }
    }
}
