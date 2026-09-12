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
    public void load_returns_defaults_when_file_is_missing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-settings-{Guid.NewGuid():N}.json");

        var result = SettingsPersistenceLoader.Load(path);

        result.Settings.TelemetryEnabled.ShouldBeTrue();
        result.Settings.LoggingEnabled.ShouldBeFalse();
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

    private static string CreateSettingsFile(string contents)
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-settings-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, contents);
        return path;
    }
}
