using System.Globalization;

namespace KubeUI.Benchmarks;

internal static class YamlBenchmarkData
{
    /// <summary>Builds nested YAML containing exactly the requested number of lines.</summary>
    public static string BuildNestedYaml(int lineCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(lineCount, 10);

        var lines = new List<string>(lineCount)
        {
            "root:",
            "  environments:",
        };

        var environmentIndex = 0;
        while (lines.Count + 8 <= lineCount)
        {
            lines.Add($"    - name: environment-{environmentIndex.ToString(CultureInfo.InvariantCulture)}");
            lines.Add("      config:");
            lines.Add("        steps:");
            lines.Add("          - name: deploy");
            lines.Add("            arguments: [validate, apply]");
            lines.Add("            resources:");
            lines.Add("              limits: {cpu: \"100m\", memory: \"64Mi\"}");
            lines.Add("        labels: {team: platform, region: local}");
            environmentIndex++;
        }

        var remainder = lineCount - lines.Count;
        if (remainder > 0)
        {
            lines.Add("  labels:");
            for (var index = 1; index < remainder; index++)
            {
                lines.Add($"    key-{index.ToString(CultureInfo.InvariantCulture)}: value-{index.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        if (lines.Count != lineCount)
        {
            throw new InvalidOperationException($"Expected {lineCount} YAML lines, generated {lines.Count}.");
        }

        return string.Join('\n', lines);
    }

    /// <summary>Builds a Kubernetes Pod YAML fixture containing exactly the requested number of lines.</summary>
    public static string BuildPodYaml(int lineCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(lineCount, 10);

        var lines = new List<string>(lineCount);
        if (lineCount == 10)
        {
            lines.Add("apiVersion: v1");
            lines.Add("kind: Pod");
            lines.Add("metadata: {name: yaml-benchmark, labels: {app: kubeui}, annotations: {benchmark.kubeui.com/config: value}}");
            lines.Add("spec:");
            lines.Add("  containers:");
            lines.Add("  - name: app");
            lines.Add("    image: nginx:latest");
            lines.Add("    env: [{name: FEATURE_FLAG, value: \"true\"}]");
            lines.Add("    resources:");
            lines.Add("      limits: {cpu: \"100m\", memory: \"64Mi\"}");
        }
        else
        {
            lines.Add("apiVersion: v1");
            lines.Add("kind: Pod");
            lines.Add("metadata:");
            lines.Add("  name: yaml-benchmark");
            lines.Add("  labels:");
            lines.Add("    app: kubeui");
            lines.Add("  annotations:");
            lines.Add("    benchmark.kubeui.com/config: value");

            var additionalContainerCount = (lineCount - 17) / 7;
            var annotationFillerCount = lineCount - 17 - (additionalContainerCount * 7);
            for (var index = 0; index < annotationFillerCount; index++)
            {
                lines.Add($"    benchmark.kubeui.com/value-{index.ToString(CultureInfo.InvariantCulture)}: item-{index.ToString(CultureInfo.InvariantCulture)}");
            }

            lines.Add("spec:");
            lines.Add("  containers:");
            AppendContainer(lines, 0);
            for (var index = 0; index < additionalContainerCount; index++)
            {
                AppendContainer(lines, index + 1);
            }
        }

        if (lines.Count != lineCount)
        {
            throw new InvalidOperationException($"Expected {lineCount} YAML lines, generated {lines.Count}.");
        }

        return string.Join('\n', lines);
    }

    private static void AppendContainer(List<string> lines, int index)
    {
        lines.Add($"  - name: app-{index.ToString(CultureInfo.InvariantCulture)}");
        lines.Add("    image: nginx:latest");
        lines.Add("    env:");
        lines.Add("    - name: FEATURE_FLAG");
        lines.Add("      value: \"true\"");
        lines.Add("    resources:");
        lines.Add("      limits: {cpu: \"100m\", memory: \"64Mi\"}");
    }

}
