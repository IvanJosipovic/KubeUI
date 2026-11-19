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
            // one round trip instead of two. The listing probe runs the exact options a listing needs,
            // because a find that accepts -maxdepth can still reject -mindepth or -printf.
            var results = await Task.WhenAll(
                RunAsync(["find", "/", "-mindepth", "0", "-maxdepth", "0", "-printf", "%y\\0%p\\0%s\\0"], cancellationToken),
                RunAsync(["cat", "/dev/null"], cancellationToken)).ConfigureAwait(false);

            // busybox tools often exit non-zero for a missing operand, so absence is detected from the
            // error text rather than the exit status.
            var canList = !Missing(results[0]);
            var canRead = !Missing(results[1]);

            // The listing probe must also produce output: a find that accepts the options but prints
            // nothing would otherwise look usable and then fail on the first listing. A failure for
            // another reason, such as a denied root, keeps its own error so the browser can show it.
            if (canList && results[0].StandardOutput.Length == 0)
            {
                return new ContainerFileCapabilities(
                    CanList: false,
                    CanRead: canRead,
                    Problem: results[0].StandardError.Length > 0 ? results[0].StandardError : null);
            }

            return new ContainerFileCapabilities(canList, canRead, Problem: null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ContainerFileCapabilities(false, false, ex.Message);
        }

        static bool Missing(ExecResult result) =>
            result.StandardError.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("No such file", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("executable file not found", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("unrecognized", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("unknown predicate", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("invalid option", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("illegal option", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("unsupported", StringComparison.OrdinalIgnoreCase);
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

        // A nonzero cat status is reported on stderr; without this a partial read after an error would
        // look like a complete file and could be downloaded. A truncated buffer is already flagged, and
        // the failure it may have hit is indistinguishable from the limit, so it is not treated as fatal.
        if (!result.Truncated && result.StandardError.Length > 0)
        {
            throw new ContainerFileAccessException(result.StandardError);
        }

        var content = result.StandardOutput;
        return new ContainerFileContent(content, result.Truncated, ContainerFileListingParser.LooksLikeText(content));
    }

    private async Task<ExecResult> RunAsync(string[] command, CancellationToken cancellationToken, int maxBytes = 64 * 1024)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(OperationTimeout);
        // Reaching the output cap stops the exec so a huge file yields its bounded prefix quickly
        // instead of being drained until the operation times out.
        using var limitReached = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);

        using var stdout = new BoundedBuffer(maxBytes, limitReached.Cancel);
        using var stderr = new BoundedBuffer(16 * 1024);

        try
        {
            await _cluster.Runtime.Client!.NamespacedPodExecAsync(
                _podName,
                _namespace,
                _containerName,
                command,
                tty: false,
                async (stdIn, stdOut, stdErr) =>
                {
                    await Task.WhenAll(
                        stdout.CopyFromAsync(stdOut, limitReached.Token),
                        stderr.CopyFromAsync(stdErr, limitReached.Token)).ConfigureAwait(false);
                },
                limitReached.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (limitReached.IsCancellationRequested
            && !timeout.IsCancellationRequested
            && !cancellationToken.IsCancellationRequested)
        {
            // Truncated success: the bounded prefix is returned below.
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // The exec timeout is not caller cancellation; surface it so a read cannot look empty.
            throw new ContainerFileAccessException(
                ResourcesLanguageManager.Instance.ContainerFileBrowserView_OperationTimeout.CurrentValue()!);
        }

        return new ExecResult(stdout.ToArray(), stdout.Truncated, Encoding.UTF8.GetString(stderr.ToArray()).Trim());
    }

    private sealed record ExecResult(byte[] StandardOutput, bool Truncated, string StandardError);

    /// <summary>Collects a stream's bytes up to a limit and reports when that limit is crossed.</summary>
    private sealed class BoundedBuffer(int limit, Action? onLimit = null) : MemoryStream
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
                    onLimit?.Invoke();
                    return;
                }

                Write(buffer, 0, Math.Min(read, room));
                if (read > room)
                {
                    Truncated = true;
                    onLimit?.Invoke();
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
