namespace KubeUI.AI.Configuration;

internal static class ExecutableLocator
{
    public static string? Find(string executable)
    {
        if (Path.IsPathRooted(executable))
            return File.Exists(executable) ? executable : null;

        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var candidates = OperatingSystem.IsWindows()
            ? GetWindowsCandidates(executable)
            : [executable];
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var candidate in candidates)
            {
                var pathCandidate = Path.Combine(directory, candidate);
                if (File.Exists(pathCandidate))
                    return pathCandidate;
            }
        }
        return null;
    }

    private static IReadOnlyList<string> GetWindowsCandidates(string executable)
    {
        if (Path.HasExtension(executable))
            return [executable];

        var extensions = Environment.GetEnvironmentVariable("PATHEXT")?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [".COM", ".EXE", ".BAT", ".CMD"];
        return [.. extensions.Select(extension => executable + extension)];
    }
}
