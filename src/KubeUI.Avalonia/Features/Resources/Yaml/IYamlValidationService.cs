using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal interface IYamlValidationService
{
    IReadOnlyList<YamlDiagnostic> Validate(string yaml, ModelCache? modelCache = null);
}
