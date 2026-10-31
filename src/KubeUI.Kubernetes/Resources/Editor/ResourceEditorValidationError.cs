namespace KubeUI.Kubernetes.Resources.Editor;

/// <summary>
/// Describes a validation problem detected in a resource editor document.
/// </summary>
/// <param name="Path">The dotted path of the affected field.</param>
/// <param name="Message">The user-facing description of the problem.</param>
public sealed record ResourceEditorValidationError(string Path, string Message);
