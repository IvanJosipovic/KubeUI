namespace KubeUI.Client
{
    public interface ISettingsService
    {
        Settings Settings { get; set; }

        void LoadSettings();
        void SaveSettings();
    }
}