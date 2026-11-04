using System.ComponentModel;
using KubeUI.App.Options;
using KubeUI.Features.AI.Chat;
using AppSettings = KubeUI.App.Options.Settings;

namespace KubeUI.Features.AI.Tests.Chat;

internal sealed class TestAgentChatSettings : IAgentChatSettings
{
    private readonly AppSettings _settings;

    public string? SelectedAgentId => _settings.SelectedAgentId;

    public bool McpServerEnabled => _settings.McpServerEnabled;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TestAgentChatSettings(AppSettings settings)
    {
        _settings = settings;
        _settings.PropertyChanged += SettingsOnPropertyChanged;
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppSettings.SelectedAgentId) or nameof(AppSettings.McpServerEnabled) or null)
            PropertyChanged?.Invoke(this, e);
    }
}
