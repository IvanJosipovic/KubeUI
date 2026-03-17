using KubeUI.Client;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class TestSettingsService : ISettingsService
{
    public Settings Settings { get; set; } = new();

    public void ApplySettings()
    {
    }

    public void SaveSettings()
    {
    }
}
