using KubeUI.App.Services.Settings;
using Microsoft.Extensions.Logging;

namespace KubeUI.App.Tests.Infra;

public sealed class TestSettingsService : SettingsService
{
    public TestSettingsService(ILogger<SettingsService> logger, ISettingsPersistence persistence)
        : base(logger, persistence)
    {
    }

    public override void SaveSettings()
    {
    }

    public override bool TrySaveSettings() => true;
}
