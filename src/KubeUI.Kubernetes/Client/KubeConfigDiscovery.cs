using k8s;
using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes;

/// <summary>
/// Discovers kubeconfig files by parsing candidate files in a directory.
/// </summary>
public sealed class KubeConfigDiscovery : IKubeConfigDiscovery
{
    private const long MaxKubeConfigSize = 10 * 1024 * 1024;

    private readonly ILogger<KubeConfigDiscovery> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubeConfigDiscovery"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public KubeConfigDiscovery(ILogger<KubeConfigDiscovery> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> DiscoverFiles(string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);

        if (!Directory.Exists(directory))
        {
            return [];
        }

        List<string> files = [];
        try
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (!Path.GetFileName(file).StartsWith('.') && IsKubeConfig(file))
                {
                    files.Add(file);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Unable to scan kubeconfig directory {Directory}", directory);
        }

        files.Sort(StringComparer.Ordinal);
        return files;
    }

    /// <inheritdoc />
    public bool IsKubeConfig(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var fileName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        try
        {
            FileInfo info = new(path);
            if (!info.Exists || info.Length == 0 || info.Length > MaxKubeConfigSize)
            {
                return false;
            }

            var config = KubernetesYaml.Deserialize<K8SConfiguration>(File.ReadAllText(path));
            if (config is null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(config.Kind) && !string.Equals(config.Kind, "Config", StringComparison.Ordinal))
            {
                return false;
            }

            return config.Contexts?.Any() == true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Skipping {Path} because it is not a kubeconfig file", path);
            return false;
        }
    }
}
