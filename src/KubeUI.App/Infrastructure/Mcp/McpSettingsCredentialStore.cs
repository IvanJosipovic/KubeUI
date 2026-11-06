using KubeUI.App.Services.Settings;
using KubeUI.Features.AI.Mcp;

namespace KubeUI.App.Infrastructure.Mcp;

internal sealed class McpSettingsCredentialStore(ISettingsService settingsService) : IMcpCredentialStore
{
    private readonly ISettingsService _settingsService = settingsService;

    public IList<McpExternalClientGrant> ExternalClients => _settingsService.Settings.McpExternalClientGrants;

    public bool TrySave() => _settingsService.TrySaveSettings();
}
