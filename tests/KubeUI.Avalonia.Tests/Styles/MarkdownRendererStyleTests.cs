using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Styles;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing.Utilities;
using LiveMarkdown.Avalonia;
using Shouldly;
using SvcSystems.UI.Terminal;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class MarkdownRendererStyleTests
{
    [AvaloniaFact]
    public void creating_a_second_fluent_style_does_not_reuse_palette_resources()
    {
        var application = Application.Current!;
        var styles = new Fluent();

        application.Styles.Add(styles);
        application.Styles.Remove(styles);
    }

    [AvaloniaFact]
    public void fluent_loads_terminal_theme_class()
    {
        var styles = new Fluent();

        styles.OfType<TerminalTheme>().ShouldHaveSingleItem();
    }

    [AvaloniaFact]
    public void markdown_colors_follow_fluent_theme_variants()
    {
        var application = Application.Current!;
        var originalTheme = application.RequestedThemeVariant;

        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();

            GetColor(application, "BorderColor").ShouldBe(Color.Parse("#737373"));
            GetColor(application, "ForegroundColor").ShouldBe(Color.Parse("#000000"));
            GetColor(application, "CardBackgroundColor").ShouldBe(Color.Parse("#EEEEF2"));
            GetColor(application, "SecondaryCardBackgroundColor").ShouldBe(Color.Parse("#CCCCCC"));
            GetColor(application, "CodeInlineColor").ShouldBe(Color.Parse("#5D5D5D"));
            GetColor(application, "QuoteBorderColor").ShouldBe(Color.Parse("#737373"));

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            GetColor(application, "BorderColor").ShouldBe(Color.Parse("#676767"));
            GetColor(application, "ForegroundColor").ShouldBe(Color.Parse("#FFFFFF"));
            GetColor(application, "CardBackgroundColor").ShouldBe(Color.Parse("#1E1E1E"));
            GetColor(application, "SecondaryCardBackgroundColor").ShouldBe(Color.Parse("#333333"));
            GetColor(application, "CodeInlineColor").ShouldBe(Color.Parse("#B4B4B4"));
            GetColor(application, "QuoteBorderColor").ShouldBe(Color.Parse("#676767"));
        }
        finally
        {
            application.RequestedThemeVariant = originalTheme;
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task markdown_text_uses_application_font_family_and_size()
    {
        var application = Application.Current!;
        var expectedFontFamily = GetResource(application, Typography.AppFontFamilyResourceKey)
            .ShouldBeOfType<FontFamily>();
        var expectedFontSize = Convert.ToDouble(GetResource(application, Typography.AppFontSizeResourceKey));

        var renderer = new MarkdownRenderer
        {
            MarkdownBuilder = new ObservableStringBuilder("## Heading\n\nBody text.")
        };
        using var window = application.CreateTestWindow(content: renderer);
        window.Show();
        await WaitForAsync(() => renderer.GetVisualDescendants().OfType<MarkdownTextBlock>().Any());

        var blocks = renderer.GetVisualDescendants().OfType<MarkdownTextBlock>().ToArray();
        blocks.ShouldNotBeEmpty();
        blocks.ShouldAllBe(block => block.FontFamily == expectedFontFamily);
        blocks.Select(block => block.FontSize).ShouldContain(expectedFontSize);
    }

    [AvaloniaFact]
    public async Task markdown_code_uses_application_code_font_and_size()
    {
        var application = Application.Current!;
        var expectedCodeFontFamily = GetResource(application, Typography.CodeFontFamilyResourceKey)
            .ShouldBeOfType<FontFamily>();
        var expectedCodeFontSize = Convert.ToDouble(GetResource(application, Typography.CodeFontSizeResourceKey));

        var renderer = new MarkdownRenderer
        {
            MarkdownBuilder = new ObservableStringBuilder("```\nvar x = 1;\n```")
        };
        using var window = application.CreateTestWindow(content: renderer);
        window.Show();
        await WaitForAsync(() => renderer.GetVisualDescendants().OfType<CodeBlock>().Any());

        var codeBlocks = renderer.GetVisualDescendants().OfType<CodeBlock>().ToArray();
        codeBlocks.ShouldNotBeEmpty();
        codeBlocks.ShouldAllBe(block => block.FontFamily == expectedCodeFontFamily);
        codeBlocks
            .SelectMany(block => block.GetVisualDescendants().OfType<MarkdownTextBlock>())
            .ShouldAllBe(block => block.FontSize == expectedCodeFontSize);
    }

    private static Task WaitForAsync(Func<bool> predicate)
        => TestWait.UntilAsync(
            predicate,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

    private static Color GetColor(Application application, string key)
    {
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        return value.ShouldBeOfType<Color>();
    }

    private static object? GetResource(Application application, string key)
    {
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        return value;
    }
}
