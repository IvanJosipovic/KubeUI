
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase, IDisposable
{
    public ISettingsService SettingsService { get; }

    public SettingsViewModel()
    {
        Title = Assets.Resources.SettingsView_Title;
        Id = nameof(SettingsViewModel);

        SettingsService = Application.Current.GetRequiredService<ISettingsService>();

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
