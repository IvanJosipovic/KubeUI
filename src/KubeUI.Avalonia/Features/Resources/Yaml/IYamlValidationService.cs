using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

public interface IYamlValidationService
{
    IReadOnlyList<YamlDiagnostic> Validate(string yaml, ModelCache? modelCache = null);
}
