using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

public interface IYamlValidationService
{
    /// <summary>
    /// Validates YAML and returns diagnostics for syntax, type, or value errors.
    /// </summary>
    /// <param name="yaml">The YAML document to validate.</param>
    /// <param name="modelCatalog">An optional cluster model catalog used for custom-resource types.</param>
    /// <returns>The validation diagnostics; an empty list indicates valid YAML.</returns>
    IReadOnlyList<YamlDiagnostic> Validate(string yaml, ClusterModelCatalog? modelCatalog = null);
}
