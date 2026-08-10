using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Shell.Documents.Settings;

public sealed partial class SettingsViewModel : ViewModelBase, IDisposable
{
    public ISettingsService SettingsService { get; }

    public SettingsViewModel(ISettingsService settingsService)
    {
        Title = Assets.Resources.SettingsView_Title;
        Id = nameof(SettingsViewModel);

        SettingsService = settingsService;

        SettingsService.Settings.PropertyChanged += Settings_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SettingsService.SaveSettings();
    }

    public void Dispose()
    {
        SettingsService.Settings.PropertyChanged -= Settings_PropertyChanged;
    }
}


