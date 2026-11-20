using System.Threading;

namespace KubeUI.Testing.Utilities;

/// <summary>
/// Creates a unique scratch directory for a test and removes it when disposed.
/// </summary>
public sealed class TemporaryDirectory : IDisposable
{
    private const int MaxDeleteAttempts = 10;

    public TemporaryDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"kubeui-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (!Directory.Exists(Path))
        {
            return;
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                Directory.Delete(Path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < MaxDeleteAttempts)
            {
                // Windows can briefly hold a handle to a file in the scratch directory
                // (antivirus/indexer or a just-closed reader). Retry with a short bounded
                // backoff; let the final failure surface so real cleanup bugs are visible.
                Thread.Sleep(20 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < MaxDeleteAttempts)
            {
                Thread.Sleep(20 * attempt);
            }
        }
    }
}
