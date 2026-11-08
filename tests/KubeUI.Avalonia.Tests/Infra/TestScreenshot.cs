using Avalonia.Controls;
using Avalonia.Headless;

namespace KubeUI.Avalonia.Tests.Infra;

/// <summary>Captures selected headless test windows when screenshot capture is enabled.</summary>
public static class TestScreenshot
{
    /// <summary>Environment variable that enables screenshot capture when set to a truthy value.</summary>
    public const string EnableEnvironmentVariable = "KUBEUI_TEST_SCREENSHOTS";

    /// <summary>Captures a rendered top-level as a PNG under <c>TestResults/Screenshots</c> when enabled.</summary>
    /// <param name="topLevel">The rendered window or other top-level to capture.</param>
    /// <param name="name">A descriptive filename component; invalid filename characters are replaced.</param>
    /// <returns>The absolute image path when captured, or <see langword="null"/> when capture is disabled.</returns>
    public static string? CaptureIfEnabled(TopLevel topLevel, string name)
    {
        ArgumentNullException.ThrowIfNull(topLevel);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!IsEnabled())
        {
            return null;
        }

        using var bitmap = topLevel.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The headless top-level has no rendered frame to capture.");

        var outputDirectory = Path.Combine(FindRepositoryRoot(), "TestResults", "Screenshots");
        Directory.CreateDirectory(outputDirectory);

        var fileName = string.Concat(name.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        var path = Path.Combine(outputDirectory, $"{fileName}.png");
        bitmap.Save(path);
        Console.WriteLine($"Screenshot captured: {path}");
        return path;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KubeUI.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Environment.CurrentDirectory;
    }

    private static bool IsEnabled()
    {
        var value = Environment.GetEnvironmentVariable(EnableEnvironmentVariable);
        return value is not null && (value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }
}
