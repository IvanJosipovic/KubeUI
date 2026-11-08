using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AvaloniaEdit;
using KubeUI.Controls.Editors;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class YamlEditorDefaultsTests
{
    [AvaloniaFact]
    public void Apply_sets_shared_editor_defaults_and_preserves_caller_owned_values()
    {
        var editor = new TextEditor
        {
            IsReadOnly = true,
            WordWrap = true,
            FontFamily = new FontFamily("serif")
        };

        YamlEditorDefaults.Apply(editor);

        editor.IsReadOnly.ShouldBeTrue();
        editor.WordWrap.ShouldBeTrue();
        editor.FontFamily.ShouldBe(new FontFamily("serif"));
        editor.FontWeight.ShouldBe(FontWeight.Normal);
        editor.HorizontalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Auto);
        editor.VerticalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Visible);
        editor.ShowLineNumbers.ShouldBeTrue();
        editor.Options.ConvertTabsToSpaces.ShouldBeTrue();
        editor.Options.EnableEmailHyperlinks.ShouldBeFalse();
        editor.Options.EnableHyperlinks.ShouldBeFalse();
        editor.Options.HighlightCurrentLine.ShouldBeTrue();
        editor.Options.IndentationSize.ShouldBe(2);
        editor.Options.ShowBoxForControlCharacters.ShouldBeFalse();
    }

    [Fact]
    public void Apply_rejects_null_editor()
    {
        Should.Throw<ArgumentNullException>(() => YamlEditorDefaults.Apply(null!));
    }
}
