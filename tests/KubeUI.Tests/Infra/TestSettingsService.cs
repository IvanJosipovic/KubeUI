using KubeUI.Client;

namespace KubeUI.Tests.Infra;

public class TestSettingsService : ISettingsService
{
    public Settings Settings { get; set; } = new();

    public void ApplySettings()
    {
    }

    public void LoadSettings()
    {
    }

    public void SaveSettings()
    {
    }
}
