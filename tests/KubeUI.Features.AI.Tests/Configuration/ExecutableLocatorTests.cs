using KubeUI.Features.AI.Configuration;
using Shouldly;

namespace KubeUI.Features.AI.Tests.Configuration;

public sealed class ExecutableLocatorTests
{
    [Fact]
    public void Find_returns_the_absolute_path_when_it_exists()
    {
        var path = CreateTempFile("probe.txt");

        ExecutableLocator.Find(path).ShouldBe(path);
        ExecutableLocator.Find(Path.Combine(Path.GetTempPath(), $"kubeui-missing-{Guid.NewGuid():N}.txt")).ShouldBeNull();
    }

    [Fact]
    public void Find_resolves_unqualified_names_from_PATH_on_windows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // dotnet is guaranteed to be on PATH for the test host.
        ExecutableLocator.Find("dotnet").ShouldNotBeNull().ShouldBe(ExecutableLocator.Find(Path.Combine("dotnet")));
    }

    [Fact]
    public void Find_returns_null_for_unknown_executables()
    {
        ExecutableLocator.Find($"kubeui-missing-{Guid.NewGuid():N}").ShouldBeNull();
    }

    private static string CreateTempFile(string name)
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-{Guid.NewGuid():N}-{name}");
        File.WriteAllText(path, string.Empty);
        return path;
    }
}
