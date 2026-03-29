namespace KubeUI.Avalonia.ViewModels;

/// <summary>
/// Represents the severity of a YAML editor diagnostic.
/// </summary>
public enum YamlDiagnosticSeverity
{
    Error,
}

/// <summary>
/// Represents a YAML validation diagnostic anchored to a line and column range.
/// </summary>
public sealed record YamlDiagnostic(
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string Message,
    YamlDiagnosticSeverity Severity);
