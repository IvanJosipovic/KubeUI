namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Signals Helm data mutations so open list documents can reload their data.</summary>
public interface IHelmListNotifier
{
    /// <summary>Raised after a release is installed or upgraded.</summary>
    event EventHandler? ReleasesChanged;

    /// <summary>Raised after a repository is added, updated, or removed.</summary>
    event EventHandler? RepositoriesChanged;

    /// <summary>Signals that release data changed.</summary>
    void NotifyReleasesChanged();

    /// <summary>Signals that repository data changed.</summary>
    void NotifyRepositoriesChanged();
}
