using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.ViewModels;

internal interface IYamlValidationService
{
    IReadOnlyList<YamlDiagnostic> Validate(string yaml, ModelCache? modelCache = null);
}
