using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AvaloniaEdit;
using KubeUI.Controls.Styles;
using KubeUI.Foundation.Styles;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class SharedControlStylesTextEditorTests
{
    [AvaloniaFact]
    public void shared_styles_apply_the_code_font_to_every_text_editor()
    {
        var application = Application.Current!;
        var styles = new SharedControlStyles();
        application.Styles.Add(styles);

        try
        {
            var editor = new TextEditor();
            var window = new Window { Content = editor };
            window.Show();

            var expectedFamily = (FontFamily)application.FindResource(Typography.CodeFontFamilyResourceKey)!;
            var expectedSize = (double)application.FindResource(Typography.CodeFontSizeResourceKey)!;

            editor.FontFamily.ShouldBe(expectedFamily);
            editor.FontSize.ShouldBe(expectedSize);
            editor.FontWeight.ShouldBe(FontWeight.Normal);

            // The TextView renders the glyphs, so the style must reach it too.
            editor.TextArea.TextView.GetValue(TemplatedControl.FontFamilyProperty).ShouldBe(expectedFamily);
            editor.TextArea.TextView.GetValue(TemplatedControl.FontSizeProperty).ShouldBe(expectedSize);
        }
        finally
        {
            application.Styles.Remove(styles);
        }
    }

    [AvaloniaFact]
    public void a_caller_supplied_font_still_wins_over_the_shared_style()
    {
        var application = Application.Current!;
        var styles = new SharedControlStyles();
        application.Styles.Add(styles);

        try
        {
            var editor = new TextEditor { FontFamily = new FontFamily("serif") };
            var window = new Window { Content = editor };
            window.Show();

            editor.FontFamily.ShouldBe(new FontFamily("serif"));
        }
        finally
        {
            application.Styles.Remove(styles);
        }
    }
}
