namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Default <see cref="IHelmListNotifier"/> shared by Helm list and editor documents.</summary>
public sealed class HelmListNotifier : IHelmListNotifier
{
    /// <inheritdoc />
    public event EventHandler? ReleasesChanged;

    /// <inheritdoc />
    public event EventHandler? RepositoriesChanged;

    /// <inheritdoc />
    public void NotifyReleasesChanged() => ReleasesChanged?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc />
    public void NotifyRepositoriesChanged() => RepositoriesChanged?.Invoke(this, EventArgs.Empty);
}
