using System.ComponentModel;
using KubeUI.App.Options;
using KubeUI.App.Services.Settings;
using KubeUI.Features.AI.Chat;
using AppSettings = KubeUI.App.Options.Settings;

namespace KubeUI.App.Services.AI;

internal sealed class AgentChatSettings : IAgentChatSettings, IDisposable
{
    private readonly ISettingsService _settingsService;
    private AppSettings _observedSettings;

    public string? SelectedAgentId => _settingsService.Settings.SelectedAgentId;

    public bool McpServerEnabled => _settingsService.Settings.McpServerEnabled;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AgentChatSettings(ISettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);
        _settingsService = settingsService;
        _observedSettings = settingsService.Settings;
        _observedSettings.PropertyChanged += SettingsOnPropertyChanged;
        _settingsService.PropertyChanged += SettingsServiceOnPropertyChanged;
    }

    public void Dispose()
    {
        _settingsService.PropertyChanged -= SettingsServiceOnPropertyChanged;
        _observedSettings.PropertyChanged -= SettingsOnPropertyChanged;
    }

    private void SettingsServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not null && e.PropertyName != nameof(ISettingsService.Settings))
            return;

        _observedSettings.PropertyChanged -= SettingsOnPropertyChanged;
        _observedSettings = _settingsService.Settings;
        _observedSettings.PropertyChanged += SettingsOnPropertyChanged;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAgentId)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(McpServerEnabled)));
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppSettings.SelectedAgentId) or nameof(AppSettings.McpServerEnabled) or null)
            PropertyChanged?.Invoke(this, e);
    }
}
