using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Services;

public sealed class SettingsServiceTests
{
    [Fact]
    public void Settings_removes_persisted_prometheus_tokens_on_load()
    {
        var data = new SettingsPersistenceData();
        data.Settings.ClusterSettings["cluster"] = new ClusterSettings
        {
            PrometheusBearerToken = "old-token",
        };
        var persistence = new MemorySettingsPersistence(data);
        var service = new SettingsService(NullLogger<SettingsService>.Instance, persistence);

        var settings = service.Settings;

        settings.ClusterSettings["cluster"].PrometheusBearerToken.ShouldBeNull();
        persistence.Saved.ShouldBeSameAs(data);
        persistence.SaveCalls.ShouldBe(1);
    }

    private sealed class MemorySettingsPersistence(SettingsPersistenceData data) : ISettingsPersistence
    {
        public string SettingsDirectory => string.Empty;

        public int SaveCalls { get; private set; }

        public SettingsPersistenceData? Saved { get; private set; }

        public bool EnsureDirectoryExists() => true;

        public SettingsPersistenceData Load() => data;

        public void Save(SettingsPersistenceData persistenceData)
        {
            SaveCalls++;
            Saved = persistenceData;
        }
    }
}
