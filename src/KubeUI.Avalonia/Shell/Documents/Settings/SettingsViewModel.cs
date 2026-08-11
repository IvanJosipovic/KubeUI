using KubeUI.AI.Agents;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Shell.Documents.Settings;

public sealed partial class SettingsViewModel : ViewModelBase, IDisposable
{
    private readonly IAgentRegistry? _agentRegistry;

    public ISettingsService SettingsService { get; }
    public IReadOnlyList<IAgent> Agents => _agentRegistry?.Agents ?? [];

    public decimal McpServerPort
    {
        get => SettingsService.Settings.McpServerPort;
        set
        {
            var port = decimal.ToInt32(decimal.Clamp(value, 1024, 65535));
            if (SettingsService.Settings.McpServerPort == port)
                return;

            SettingsService.Settings.McpServerPort = port;
            OnPropertyChanged();
        }
    }

    public IAgent? SelectedAgent
    {
        get => Agents.FirstOrDefault(agent => string.Equals(
            agent.Id,
            SettingsService.Settings.SelectedAgentId,
            StringComparison.Ordinal)) ?? Agents.FirstOrDefault();
        set
        {
            var id = value?.Id;
            if (string.Equals(SettingsService.Settings.SelectedAgentId, id, StringComparison.Ordinal))
                return;

            SettingsService.Settings.SelectedAgentId = id;
            OnPropertyChanged();
        }
    }

    public SettingsViewModel(ISettingsService settingsService, IAgentRegistry? agentRegistry = null)
    {
        Title = Assets.Resources.SettingsView_Title;
        Id = nameof(SettingsViewModel);

        SettingsService = settingsService;
        _agentRegistry = agentRegistry;

        SettingsService.Settings.PropertyChanged += Settings_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SettingsService.SaveSettings();
        if (e.PropertyName == nameof(KubeUI.Avalonia.Options.Settings.SelectedAgentId))
            OnPropertyChanged(nameof(SelectedAgent));
        else if (e.PropertyName == nameof(KubeUI.Avalonia.Options.Settings.McpServerPort))
            OnPropertyChanged(nameof(McpServerPort));
    }

    public void Dispose()
    {
        SettingsService.Settings.PropertyChanged -= Settings_PropertyChanged;
    }
}
