using System.ComponentModel;
using KubeUI.Kubernetes;
using AppAppearanceSettings = KubeUI.App.Options.AppearanceSettings;
using AppSettings = KubeUI.App.Options.Settings;

namespace KubeUI.App.Services.Settings;

public interface ISettingsService : IClusterSettingsStore, INotifyPropertyChanged
{
    AppSettings Settings { get; set; }
    AppAppearanceSettings Appearance { get; set; }
    IClusterSettingsStore Clusters { get; }
    void ApplySettings();
    void SaveSettings();
    /// <summary>Saves settings and reports whether the persisted file was updated.</summary>
    bool TrySaveSettings();
}
