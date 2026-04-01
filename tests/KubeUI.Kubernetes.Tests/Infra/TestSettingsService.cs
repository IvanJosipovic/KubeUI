using KubeUI.Avalonia;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class TestSettingsService : ISettingsService, IClusterSettingsStore
{
    public Settings Settings { get; set; } = new();
    public AppearanceSettings Appearance { get; set; } = new();
    public IClusterSettingsStore Clusters => this;

    public void ApplySettings()
    {
    }

    public void SaveSettings()
    {
    }

    IReadOnlyCollection<string> IClusterSettingsStore.KubeConfigPaths => Settings.KubeConfigs;

    public void AddKubeConfigPath(string path)
    {
        Settings.AddKubeConfig(path);
    }

    public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster)
    {
        return Settings.GetClusterSettings(cluster).Namespaces ?? [];
    }
}

