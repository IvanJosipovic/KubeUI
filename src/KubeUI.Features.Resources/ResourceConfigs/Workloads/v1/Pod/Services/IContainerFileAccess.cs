using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Services;

/// <summary>One entry in a container directory listing.</summary>
/// <param name="Name">The entry's name, without its parent path.</param>
/// <param name="Path">The entry's absolute path inside the container.</param>
/// <param name="IsDirectory">Whether the entry is a directory.</param>
/// <param name="IsSymbolicLink">Whether the entry is a symbolic link.</param>
/// <param name="Size">The entry's size in bytes when the container could report it.</param>
public sealed record ContainerFileEntry(string Name, string Path, bool IsDirectory, bool IsSymbolicLink, long? Size)
{
    /// <summary>Whether the entry is a regular file rather than a folder or symbolic link.</summary>
    public bool IsFile => !IsDirectory && !IsSymbolicLink;
}

/// <summary>The outcome of a container directory listing.</summary>
/// <param name="Entries">The entries found, ordered directories first then by name.</param>
/// <param name="Truncated">Whether the listing stopped at the entry limit.</param>
public sealed record ContainerDirectoryListing(IReadOnlyList<ContainerFileEntry> Entries, bool Truncated);

/// <summary>What a container can do for file browsing, probed before any listing.</summary>
/// <param name="CanList">Whether <c>find</c> with NUL framing and type reporting is available.</param>
/// <param name="CanRead">Whether <c>cat</c> is available.</param>
/// <param name="Problem">Why the probe could not complete, when it could not.</param>
public sealed record ContainerFileCapabilities(bool CanList, bool CanRead, string? Problem)
{
    /// <summary>Whether browsing can proceed with the utilities found.</summary>
    public bool CanBrowse => Problem is null && CanList && CanRead;

    /// <summary>The missing capabilities, named for the user.</summary>
    public IReadOnlyList<string> Missing
    {
        get
        {
            List<string> missing = [];
            if (!CanList)
            {
                missing.Add("find");
            }

            if (!CanRead)
            {
                missing.Add("cat");
            }

            return missing;
        }
    }
}

/// <summary>The outcome of reading a container file.</summary>
/// <param name="Content">The bytes read, at most the requested limit.</param>
/// <param name="Truncated">Whether the file was larger than the limit.</param>
/// <param name="IsText">Whether the content looks like text rather than binary.</param>
public sealed record ContainerFileContent(byte[] Content, bool Truncated, bool IsText);

/// <summary>Raised when a container file operation cannot complete, with a message fit for the user.</summary>
public sealed class ContainerFileAccessException(string message, Exception? innerException = null)
    : Exception(message, innerException);

/// <summary>Reads a running container's files through non-interactive exec.</summary>
public interface IContainerFileAccess
{
    /// <summary>Probes the container for the utilities browsing needs.</summary>
    Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists a directory's entries.</summary>
    Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Reads a file's content, bounded by the implementation's limit.</summary>
    Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>Creates access to a container's files.</summary>
public interface IContainerFileAccessFactory
{
    /// <summary>Creates access for one pod and container.</summary>
    IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string podNamespace, string containerName);
}
