namespace KubeUI.Kubernetes.Resources.Editor;

/// <summary>
/// Describes a single choice offered by an enum editor.
/// </summary>
/// <param name="Value">The stored value, or <see langword="null"/> when the field is unset.</param>
/// <param name="DisplayName">The text shown to the user.</param>
public sealed record ResourceEditorEnumOption(string? Value, string DisplayName);
