using System.Text;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Services;

/// <summary>Parses the NUL-delimited output of the container listing command.</summary>
public static class ContainerFileListingParser
{
    /// <summary>
    /// Parses <c>find -printf "%y\0%p\0%s\0"</c> output. Each entry is three NUL-terminated fields: type, path, size.
    /// Malformed or truncated output is rejected rather than guessed at.
    /// </summary>
    /// <param name="output">The raw bytes from the container.</param>
    /// <param name="maxEntries">The most entries to return.</param>
    public static ContainerDirectoryListing Parse(ReadOnlySpan<byte> output, int maxEntries)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEntries);

        var fields = SplitNul(output);
        var entries = new List<ContainerFileEntry>(Math.Min(fields.Count / 3, maxEntries));
        var truncated = false;

        for (var i = 0; i + 2 < fields.Count; i += 3)
        {
            if (entries.Count >= maxEntries)
            {
                truncated = true;
                break;
            }

            var type = fields[i];
            var path = fields[i + 1];
            var sizeText = fields[i + 2];

            if (type.Length == 0 || path.Length == 0)
            {
                continue;
            }

            var isDirectory = type == "d";
            var isLink = type == "l";
            long? size = long.TryParse(sizeText, out var parsed) ? parsed : null;
            var name = NameOf(path);
            if (name.Length == 0)
            {
                continue;
            }

            entries.Add(new ContainerFileEntry(name, path, isDirectory, isLink, size));
        }

        entries.Sort(static (left, right) =>
            left.IsDirectory != right.IsDirectory
                ? left.IsDirectory ? -1 : 1
                : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

        return new ContainerDirectoryListing(entries, truncated);
    }

    /// <summary>Whether the bytes look like text rather than binary.</summary>
    public static bool LooksLikeText(ReadOnlySpan<byte> content)
    {
        if (content.Length == 0)
        {
            return true;
        }

        // A NUL byte in the first block is the usual binary signal.
        var sample = content[..Math.Min(content.Length, 8192)];
        return sample.IndexOf((byte)0) < 0;
    }

    private static List<string> SplitNul(ReadOnlySpan<byte> output)
    {
        var fields = new List<string>();
        var start = 0;
        for (var i = 0; i < output.Length; i++)
        {
            if (output[i] != 0)
            {
                continue;
            }

            fields.Add(Encoding.UTF8.GetString(output[start..i]));
            start = i + 1;
        }

        if (start < output.Length)
        {
            fields.Add(Encoding.UTF8.GetString(output[start..]));
        }

        return fields;
    }

    private static string NameOf(string path)
    {
        var trimmed = path.TrimEnd('/');
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var slash = trimmed.LastIndexOf('/');
        return slash < 0 ? trimmed : trimmed[(slash + 1)..];
    }
}
