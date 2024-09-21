
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase, IDisposable
{
    public SettingsService SettingsService { get; }

    public SettingsViewModel()
    {
        Title = Resources.SettingsView_Title;
        Id = nameof(SettingsViewModel);

        SettingsService = Application.Current.GetRequiredService<SettingsService>();

        SettingsService.Settings.PropertyChanged += Settings_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Save();
    }

    [RelayCommand]
    private void Save()
    {
        SettingsService.SaveSettings();
    }

    [RelayCommand]
    private void Load()
    {
        SettingsService.LoadSettings();
    }

    public void Dispose()
    {
        SettingsService.Settings.PropertyChanged -= Settings_PropertyChanged;
    }
}
