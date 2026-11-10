namespace KubeUI.Features.Resources.Settings;

/// <summary>Provides appearance preferences needed by resource editors and log views.</summary>
public interface IResourceAppearanceSettings
{
    /// <summary>Adjusts the console font size by the requested number of points.</summary>
    void AdjustConsoleFontSize(int change);
}
