using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Client.Models;

namespace KubeUI.AI.Acp;

internal sealed class FileSystemHandler(
    IAgentPermissionService permissionService,
    IReadOnlySet<string>? fileSystemRoots = null) : IFileSystemHandler
{
    private readonly string[] _fileSystemRoots = fileSystemRoots is null
        ? []
        : fileSystemRoots.Select(Path.GetFullPath).ToArray();

    public async ValueTask<string> ReadTextFileAsync(
        ReadTextFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = request.Path;
        var permission = await permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("read_file", path), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
            throw new UnauthorizedAccessException(permission.Reason ?? $"Reading '{path}' was denied.");
        EnsurePathAllowed(path);

        var start = request.Line is null ? 0 : checked((int)request.Line.Value - 1);
        var lineLimit = request.Limit is null ? int.MaxValue : checked((int)request.Limit.Value);
        if (start < 0 || lineLimit < 0)
            return string.Empty;

        using var reader = File.OpenText(path);
        for (var index = 0; index < start; index++)
        {
            if (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is null)
                return string.Empty;
        }

        var selected = new List<string>(Math.Min(lineLimit, 256));
        while (selected.Count < lineLimit
            && await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } text)
            selected.Add(text);
        return string.Join('\n', selected);
    }

    public async ValueTask WriteTextFileAsync(
        WriteTextFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = request.Path;
        var permission = await permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("write_file", path, IsDestructive: true), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
            throw new UnauthorizedAccessException(permission.Reason ?? $"Writing '{path}' was denied.");
        EnsurePathAllowed(path);
        await File.WriteAllTextAsync(path, request.Content, cancellationToken).ConfigureAwait(false);
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
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
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
