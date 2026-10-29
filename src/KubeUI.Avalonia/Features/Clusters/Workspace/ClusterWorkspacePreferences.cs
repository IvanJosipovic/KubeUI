using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Features.Clusters.Workspace;

/// <summary>
/// Keeps per-cluster user preferences (enabled state and alias) in sync between workspaces and persisted settings.
/// </summary>
public sealed class ClusterWorkspacePreferences
{
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClusterWorkspacePreferences"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service used to load and persist preferences.</param>
    public ClusterWorkspacePreferences(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Applies persisted preferences to <paramref name="workspace"/> and persists later changes.
    /// </summary>
    /// <param name="workspace">The workspace to track.</param>
    public void Attach(ClusterWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        if (_settingsService.Settings.FindClusterSettings(workspace.Runtime) is { } settings)
        {
            workspace.Alias = settings.Alias;
            workspace.IsEnabled = settings.Enabled;
        }

        workspace.PropertyChanged += OnWorkspacePropertyChanged;
    }

    /// <summary>
    /// Stops persisting preference changes for <paramref name="workspace"/>.
    /// </summary>
    /// <param name="workspace">The workspace to stop tracking.</param>
    public void Detach(ClusterWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        workspace.PropertyChanged -= OnWorkspacePropertyChanged;
    }

    private void OnWorkspacePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ClusterWorkspace workspace
            || (e.PropertyName != nameof(ClusterWorkspace.Alias) && e.PropertyName != nameof(ClusterWorkspace.IsEnabled)))
        {
            return;
        }

        var settings = _settingsService.Settings.GetClusterSettings(workspace.Runtime);
        settings.Alias = string.IsNullOrWhiteSpace(workspace.Alias) ? null : workspace.Alias.Trim();
        settings.Enabled = workspace.IsEnabled;
        _settingsService.SaveSettings();
    }
}
