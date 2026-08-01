using Microsoft.Extensions.Logging;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class TestSettingsService : SettingsService
{
    public TestSettingsService(ILogger<SettingsService> logger, ISettingsPersistence persistence)
        : base(logger, persistence)
    {
    }

    public override void ApplySettings()
    {
    }

    public override void SaveSettings()
    {
    }
}
