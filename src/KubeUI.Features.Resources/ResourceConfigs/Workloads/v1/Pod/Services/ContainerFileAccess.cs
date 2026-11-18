using System.Text;
using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Services;

/// <summary>
/// Reads a container's files through non-interactive exec. Commands are passed as literal argument vectors, never
/// through a shell, so a path cannot change command semantics. Output is bounded while it streams.
/// </summary>
public sealed class ContainerFileAccess : IContainerFileAccess
{
    /// <summary>The largest file read into memory for preview or download.</summary>
    public const int MaxFileBytes = 4 * 1024 * 1024;

    /// <summary>The most entries a single directory listing returns.</summary>
    public const int MaxEntries = 5000;

    /// <summary>The most output bytes a directory listing collects before it is truncated.</summary>
    public const int MaxListingBytes = 2 * 1024 * 1024;

    /// <summary>How long one exec may take before it is cancelled.</summary>
    public static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(30);

    private readonly ClusterWorkspace _cluster;
    private readonly string _podName;
    private readonly string _namespace;
    private readonly string _containerName;

    /// <summary>Creates access for one pod and container.</summary>
    public ContainerFileAccess(ClusterWorkspace cluster, string podName, string @namespace, string containerName)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentException.ThrowIfNullOrWhiteSpace(podName);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        _cluster = cluster;
        _podName = podName;
        _namespace = @namespace;
        _containerName = containerName;
    }

    /// <inheritdoc />
    public async Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Both probes open their own exec stream, so they run concurrently to keep opening a browser at
            // one round trip instead of two.
            var results = await Task.WhenAll(
                RunAsync(["find", "/", "-maxdepth", "0"], cancellationToken),
                RunAsync(["cat", "/dev/null"], cancellationToken)).ConfigureAwait(false);

            // A command that ran without a "not found" error is available. busybox tools often exit non-zero
            // for a missing operand, so absence is detected from the error text rather than the exit status.
            return new ContainerFileCapabilities(
                CanList: !Missing(results[0]),
                CanRead: !Missing(results[1]),
                Problem: null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ContainerFileCapabilities(false, false, ex.Message);
        }

        static bool Missing(ExecResult result) =>
            result.StandardError.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("No such file", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("executable file not found", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // -print0 gives unambiguous framing; %y reports kind in the same call.
        var result = await RunAsync(
            ["find", path, "-maxdepth", "1", "-mindepth", "1", "-printf", "%y\\0%p\\0%s\\0"],
            cancellationToken,
            MaxListingBytes).ConfigureAwait(false);

        if (result.StandardOutput.Length == 0 && result.StandardError.Length > 0)
        {
            throw new ContainerFileAccessException(result.StandardError);
        }

        var listing = ContainerFileListingParser.Parse(result.StandardOutput, MaxEntries);

        // A byte-level cut-off would otherwise drop trailing entries silently.
        return result.Truncated ? listing with { Truncated = true } : listing;
    }

    /// <inheritdoc />
    public async Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var result = await RunAsync(["cat", path], cancellationToken, MaxFileBytes).ConfigureAwait(false);
        if (result.StandardOutput.Length == 0 && result.StandardError.Length > 0)
        {
            throw new ContainerFileAccessException(result.StandardError);
        }

        var content = result.StandardOutput;
        var truncated = result.Truncated || content.Length >= MaxFileBytes;
        return new ContainerFileContent(content, truncated, ContainerFileListingParser.LooksLikeText(content));
    }

    private async Task<ExecResult> RunAsync(string[] command, CancellationToken cancellationToken, int maxBytes = 64 * 1024)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(OperationTimeout);

        using var stdout = new BoundedBuffer(maxBytes);
        using var stderr = new BoundedBuffer(16 * 1024);

        await _cluster.Runtime.Client!.NamespacedPodExecAsync(
            _podName,
            _namespace,
            _containerName,
            command,
            tty: false,
            async (stdIn, stdOut, stdErr) =>
            {
                await Task.WhenAll(
                    stdout.CopyFromAsync(stdOut, timeout.Token),
                    stderr.CopyFromAsync(stdErr, timeout.Token)).ConfigureAwait(false);
            },
            timeout.Token).ConfigureAwait(false);

        return new ExecResult(stdout.ToArray(), stdout.Truncated, Encoding.UTF8.GetString(stderr.ToArray()).Trim());
    }

    private sealed record ExecResult(byte[] StandardOutput, bool Truncated, string StandardError);

    /// <summary>Collects a stream's bytes up to a limit, then drains without storing.</summary>
    private sealed class BoundedBuffer(int limit) : MemoryStream
    {
        public bool Truncated { get; private set; }

        public async Task CopyFromAsync(Stream source, CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            while (true)
            {
                var read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read <= 0)
                {
                    return;
                }

                var room = limit - (int)Length;
                if (room <= 0)
                {
                    Truncated = true;
                    continue;
                }

                Write(buffer, 0, Math.Min(read, room));
                if (read > room)
                {
                    Truncated = true;
                }
            }
        }
    }
}

/// <summary>Creates <see cref="ContainerFileAccess"/> instances, one per browsed container.</summary>
public sealed class ContainerFileAccessFactory : IContainerFileAccessFactory
{
    /// <inheritdoc />
    public IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string @namespace, string containerName)
        => new ContainerFileAccess(cluster, podName, @namespace, containerName);
}
