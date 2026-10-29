using Shouldly;

namespace KubeUI.Avalonia.Tests.Services.Settings;

public sealed class SettingsPersistenceLoaderTests
{
    [Fact]
    public void load_reads_settings_from_file()
    {
        var path = CreateSettingsFile("{\"Settings\":{\"LoggingEnabled\":true,\"TelemetryEnabled\":false}}");

        try
        {
            var result = SettingsPersistenceLoader.Load(path);

            result.Settings.LoggingEnabled.ShouldBeTrue();
            result.Settings.TelemetryEnabled.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void load_reads_preview_feature_flags_from_file()
    {
        var path = CreateSettingsFile(
            "{\"Settings\":{\"Features\":{\"Helm\":true,\"Hubble\":false,\"DisableJsonSerializationReflection\":true,\"DisableYamlSerializationReflection\":false}}}");

        try
        {
            var result = SettingsPersistenceLoader.Load(path);

            result.Settings.PreviewFeatures.Helm.ShouldBeTrue();
            result.Settings.PreviewFeatures.Hubble.ShouldBeFalse();
            result.Settings.PreviewFeatures.DisableJsonSerializationReflection.ShouldBeTrue();
            result.Settings.PreviewFeatures.DisableYamlSerializationReflection.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void load_defaults_preview_feature_flags_to_disabled_when_settings_omit_them()
    {
        var path = CreateSettingsFile("{\"Settings\":{\"LoggingEnabled\":true}}");

        try
        {
            var result = SettingsPersistenceLoader.Load(path);

            result.Settings.PreviewFeatures.Helm.ShouldBeFalse();
            result.Settings.PreviewFeatures.Hubble.ShouldBeFalse();
            result.Settings.PreviewFeatures.DisableJsonSerializationReflection.ShouldBeFalse();
            result.Settings.PreviewFeatures.DisableYamlSerializationReflection.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void load_returns_defaults_when_file_is_missing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-settings-{Guid.NewGuid():N}.json");

        var result = SettingsPersistenceLoader.Load(path);

        result.Settings.TelemetryEnabled.ShouldBeTrue();
        result.Settings.LoggingEnabled.ShouldBeFalse();
        result.Settings.PreviewFeatures.Helm.ShouldBeFalse();
        result.Settings.PreviewFeatures.Hubble.ShouldBeFalse();
        result.Settings.PreviewFeatures.DisableJsonSerializationReflection.ShouldBeFalse();
        result.Settings.PreviewFeatures.DisableYamlSerializationReflection.ShouldBeFalse();
    }

    [Fact]
    public void load_returns_defaults_when_file_is_invalid()
    {
        var path = CreateSettingsFile("not-json");

        try
        {
            var result = SettingsPersistenceLoader.Load(path);

            result.Settings.TelemetryEnabled.ShouldBeTrue();
            result.Settings.LoggingEnabled.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void load_restricts_settings_file_permissions_on_unix()
    {
        if (OperatingSystem.IsWindows())
            return;
        var path = CreateSettingsFile("{}");

        try
        {
            _ = SettingsPersistenceLoader.Load(path);

            File.GetUnixFileMode(path).ShouldBe(UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateSettingsFile(string contents)
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-settings-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, contents);
        return path;
    }
}
