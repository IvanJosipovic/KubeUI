using System.Xml.Linq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Architecture;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void AvaloniaFoundation_has_no_internal_project_references()
    {
        var projects = LoadProjectGraph();

        projects["KubeUI.Avalonia"].References.ShouldBeEmpty();
    }

    [Fact]
    public void Controls_references_only_the_Avalonia_foundation()
    {
        var projects = LoadProjectGraph();

        projects["KubeUI.Controls"].References.ShouldBe(["KubeUI.Avalonia"]);
    }

    [Fact]
    public void Desktop_references_only_App()
    {
        var projects = LoadProjectGraph();

        projects["KubeUI.Desktop"].References.ShouldBe(["KubeUI.App"]);
    }

    [Fact]
    public void Features_do_not_reference_shell_or_framework_foundation()
    {
        var projects = LoadProjectGraph();
        string[] forbidden = ["KubeUI.App", "KubeUI.Desktop", "KubeUI.Avalonia"];

        foreach (var project in projects.Values.Where(static project => project.Name.StartsWith("KubeUI.Features.", StringComparison.Ordinal)))
        {
            foreach (var target in forbidden)
            {
                project.References.ShouldNotContain(target, $"{project.Name} must not reference {target}.");
            }
        }
    }

    [Fact]
    public void Kubernetes_runtime_does_not_reference_UI_or_feature_projects()
    {
        var projects = LoadProjectGraph();
        string[] forbidden =
        [
            "KubeUI.App",
            "KubeUI.Controls",
            "KubeUI.Desktop",
            "KubeUI.Avalonia",
            .. projects.Keys.Where(static name => name.StartsWith("KubeUI.Features.", StringComparison.Ordinal))
        ];

        foreach (var target in forbidden)
        {
            projects["KubeUI.Kubernetes"].References.ShouldNotContain(
                target,
                $"KubeUI.Kubernetes is the non-UI runtime layer and must not reference {target}.");
        }
    }

    [Fact]
    public void Clusters_does_not_reference_Resources()
    {
        var projects = LoadProjectGraph();

        projects["KubeUI.Features.Clusters"].References.ShouldNotContain("KubeUI.Features.Resources");
    }

    [Fact]
    public void Feature_to_feature_references_match_the_ownership_map()
    {
        Dictionary<string, string[]> expectedFeatureDependencies = new(StringComparer.Ordinal)
        {
            ["KubeUI.Features.AI"] = [],
            ["KubeUI.Features.Azure"] = [],
            ["KubeUI.Features.Clusters"] = ["KubeUI.Features.Azure"],
            ["KubeUI.Features.Cilium"] = ["KubeUI.Features.Clusters", "KubeUI.Features.Resources"],
            ["KubeUI.Features.Crossplane"] = ["KubeUI.Features.Clusters", "KubeUI.Features.Resources"],
            ["KubeUI.Features.GatewayApi"] = ["KubeUI.Features.Resources"],
            ["KubeUI.Features.Helm"] = ["KubeUI.Features.Clusters", "KubeUI.Features.Resources"],
            ["KubeUI.Features.Resources"] = ["KubeUI.Features.AI", "KubeUI.Features.Clusters"]
        };
        var projects = LoadProjectGraph();
        var actualFeatureProjects = projects.Keys
            .Where(static name => name.StartsWith("KubeUI.Features.", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        actualFeatureProjects.ShouldBe(expectedFeatureDependencies.Keys.Order(StringComparer.Ordinal).ToArray());

        foreach (var (projectName, expectedDependencies) in expectedFeatureDependencies)
        {
            var actualDependencies = projects[projectName].References
                .Where(static name => name.StartsWith("KubeUI.Features.", StringComparison.Ordinal))
                .Order(StringComparer.Ordinal)
                .ToArray();

            actualDependencies.ShouldBe(
                expectedDependencies.Order(StringComparer.Ordinal).ToArray(),
                $"{projectName} feature references must match docs/architecture/ownership-map.md.");
        }
    }

    [Fact]
    public void Source_project_dependency_graph_is_acyclic()
    {
        var projects = LoadProjectGraph();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();
        string? cycle = null;

        bool Visit(string projectName)
        {
            if (active.Contains(projectName))
            {
                var cycleStart = path.IndexOf(projectName);
                cycle = string.Join(" -> ", path.Skip(cycleStart).Append(projectName));
                return false;
            }

            if (!visited.Add(projectName))
            {
                return true;
            }

            active.Add(projectName);
            path.Add(projectName);
            foreach (var reference in projects[projectName].References.Order(StringComparer.Ordinal))
            {
                if (!Visit(reference))
                {
                    return false;
                }
            }

            path.RemoveAt(path.Count - 1);
            active.Remove(projectName);
            return true;
        }

        var isAcyclic = projects.Keys.Order(StringComparer.Ordinal).All(Visit);

        isAcyclic.ShouldBeTrue(cycle is null ? "Project graph must be acyclic." : $"Project dependency cycle: {cycle}");
    }

    private static Dictionary<string, ProjectReferences> LoadProjectGraph()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "src");
        var projectPaths = Directory.EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories).ToArray();
        var projectNamesByPath = projectPaths.ToDictionary(
            Path.GetFullPath,
            static path => Path.GetFileNameWithoutExtension(path),
            StringComparer.OrdinalIgnoreCase);
        var projects = new Dictionary<string, ProjectReferences>(StringComparer.Ordinal);

        foreach (var projectPath in projectPaths)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var projectDirectory = Path.GetDirectoryName(projectPath)!;
            var document = XDocument.Load(projectPath);
            var references = document
                .Descendants()
                .Where(static element => element.Name.LocalName == "ProjectReference")
                .Select(static element => (string?)element.Attribute("Include"))
                .Where(static include => !string.IsNullOrWhiteSpace(include))
                .Select(include => Path.GetFullPath(Path.Combine(projectDirectory, include!)))
                .Select(path => projectNamesByPath.TryGetValue(path, out var name)
                    ? name
                    : throw new InvalidDataException($"{projectPath} references a project outside src or a missing project: {path}"))
                .ToHashSet(StringComparer.Ordinal);

            projects.Add(projectName, new ProjectReferences(projectName, references));
        }

        return projects;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KubeUI.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate KubeUI.slnx from the test output directory.");
    }

    private sealed record ProjectReferences(string Name, HashSet<string> References);
}
