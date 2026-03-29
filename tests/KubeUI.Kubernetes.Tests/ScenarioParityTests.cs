using System.Reflection;
using Shouldly;
using Xunit;

namespace KubeUI.Kubernetes.Tests;

public class ScenarioParityTests
{
    [Fact]
    public void Mock_and_E2E_suites_should_expose_the_same_test_names()
    {
        var assembly = typeof(ScenarioParityTests).Assembly;

        var mockTests = GetScenarioTests(assembly, "KubeUI.Kubernetes.Tests.Mock");
        var e2eTests = GetScenarioTests(assembly, "KubeUI.Kubernetes.Tests.E2E");

        mockTests.ShouldBe(e2eTests);
    }

    private static List<string> GetScenarioTests(Assembly assembly, string namespacePrefix)
    {
        return assembly.GetTypes()
            .Where(type => type.IsClass && type.Namespace != null && type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttribute<FactAttribute>() != null)
                .Select(method => $"{type.Name}.{method.Name}"))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }
}
