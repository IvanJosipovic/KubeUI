using Microsoft.Extensions.Logging;
using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class TestSettingsService : SettingsService
{
    public TestSettingsService(ILogger<SettingsService> logger)
        : base(logger)
    {
    }

    public override void ApplySettings()
    {
    }

    public override void SaveSettings()
    {
    }
}
