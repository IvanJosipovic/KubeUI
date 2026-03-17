using KubeUI.Client;

namespace KubeUI.Avalonia.Tests.Infra;

public class TestSettingsService : ISettingsService
{
    public Settings Settings { get; set; } = new();

    public void ApplySettings()
    {
    }

    public void SaveSettings()
    {
    }
}
