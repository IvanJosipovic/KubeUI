namespace KubeUI.Kubernetes;

/// <summary>
/// Finds kubeconfig files in a directory.
/// </summary>
public interface IKubeConfigDiscovery
{
    /// <summary>
    /// Returns the top-level files in <paramref name="directory"/> that are kubeconfig files with at least one context.
    /// </summary>
    /// <param name="directory">The directory to scan. Missing directories return an empty list.</param>
    /// <returns>The discovered kubeconfig paths sorted ordinally.</returns>
    IReadOnlyList<string> DiscoverFiles(string directory);

    /// <summary>
    /// Determines whether <paramref name="path"/> is a readable kubeconfig file with at least one context.
    /// </summary>
    /// <param name="path">The file path to inspect.</param>
    /// <returns><see langword="true"/> when the file is a kubeconfig; otherwise <see langword="false"/>.</returns>
    bool IsKubeConfig(string path);
}
