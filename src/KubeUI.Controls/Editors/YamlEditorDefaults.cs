using Avalonia.Controls.Primitives;
using AvaloniaEdit;

namespace KubeUI.Controls.Editors;

/// <summary>Applies the common visual and indentation defaults to a YAML text editor.</summary>
public static class YamlEditorDefaults
{
    /// <summary>Applies shared YAML editor options without changing text, readonly, wrap, or font bindings.</summary>
    /// <param name="editor">Editor to configure.</param>
    public static void Apply(TextEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        editor.FontWeight = FontWeight.Normal;
        editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        editor.ShowLineNumbers = true;
        editor.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        editor.Options = new TextEditorOptions
        {
            ConvertTabsToSpaces = true,
            EnableEmailHyperlinks = false,
            EnableHyperlinks = false,
            HighlightCurrentLine = true,
            IndentationSize = 2,
            ShowBoxForControlCharacters = false
        };
    }
}
