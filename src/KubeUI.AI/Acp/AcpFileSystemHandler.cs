using dotacp.protocol;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Acp;

internal sealed class AcpFileSystemHandler(
    IAgentPermissionService permissionService,
    IReadOnlySet<string>? fileSystemRoots = null)
{
    private readonly string[] _fileSystemRoots = fileSystemRoots is null
        ? []
        : fileSystemRoots.Select(Path.GetFullPath).ToArray();

    public async Task<ReadTextFileResponse> ReadTextFileAsync(
        ReadTextFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var permission = await permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("read_file", request.Path), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
            throw new UnauthorizedAccessException(permission.Reason ?? $"Reading '{request.Path}' was denied.");
        EnsurePathAllowed(request.Path);

        var start = request.Line is null ? 0 : checked((int)request.Line.Value - 1);
        var limit = request.Limit is null ? int.MaxValue : checked((int)request.Limit.Value);
        if (start < 0 || limit < 0)
            return new ReadTextFileResponse { Content = string.Empty };

        using var reader = File.OpenText(request.Path);
        for (var index = 0; index < start; index++)
        {
            if (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is null)
                return new ReadTextFileResponse { Content = string.Empty };
        }

        var selected = new List<string>(Math.Min(limit, 256));
        while (selected.Count < limit
            && await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
            selected.Add(line);
        return new ReadTextFileResponse { Content = string.Join('\n', selected) };
    }

    public async Task<WriteTextFileResponse> WriteTextFileAsync(
        WriteTextFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var permission = await permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("write_file", request.Path, IsDestructive: true), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
            throw new UnauthorizedAccessException(permission.Reason ?? $"Writing '{request.Path}' was denied.");
        EnsurePathAllowed(request.Path);

        await File.WriteAllTextAsync(request.Path, request.Content, cancellationToken).ConfigureAwait(false);
        return new WriteTextFileResponse();
    }

    private void EnsurePathAllowed(string path)
    {
        if (_fileSystemRoots.Length == 0)
            return;

        var fullPath = Path.GetFullPath(path);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (_fileSystemRoots.Any(root => IsWithinRoot(fullPath, root, comparison)
            && !ContainsReparsePoint(root, fullPath)))
            return;
        throw new UnauthorizedAccessException($"File path '{path}' is outside configured ACP filesystem roots.");
    }

    private static bool ContainsReparsePoint(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        var current = root;
        foreach (var component in relative.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            if (HasReparsePoint(current))
                return true;
        }

        return HasReparsePoint(root);
    }

    private static bool HasReparsePoint(string path)
    {
        try
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    private static bool IsWithinRoot(string path, string root, StringComparison comparison)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative == "."
            || (!Path.IsPathRooted(relative)
                && !string.Equals(relative, "..", comparison)
                && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", comparison)
                && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", comparison));
    }
}
