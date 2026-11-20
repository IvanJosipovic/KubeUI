using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AvaloniaEdit;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Features.Resources.Settings;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Infrastructure;

public sealed class CodeFontSizeKeyBindingsTests
{
    [AvaloniaFact]
    public void Applying_the_shortcuts_adds_the_shared_font_size_bindings()
    {
        var settings = new RecordingAppearanceSettings();
        var editor = new TextEditor();

        editor.WithCodeFontSizeKeyBindings(settings).ShouldBeSameAs(editor);

        editor.KeyBindings.Count.ShouldBe(4);
        editor.KeyBindings.ShouldAllBe(binding => binding.Gesture!.KeyModifiers == KeyModifiers.Control);

        editor.KeyBindings[0].Command!.Execute(null);
        editor.KeyBindings[2].Command!.Execute(null);

        settings.Adjustments.ShouldBe([1, -1]);
    }

    [AvaloniaFact]
    public void Applying_the_shortcuts_works_on_any_input_element()
    {
        var settings = new RecordingAppearanceSettings();
        var root = new Grid();

        root.WithCodeFontSizeKeyBindings(settings);

        root.KeyBindings.Count.ShouldBe(4);
        root.KeyBindings[0].Command!.Execute(null);

        settings.Adjustments.ShouldBe([1]);
    }

    [Fact]
    public void Applying_the_shortcuts_rejects_null_arguments()
    {
        var settings = new RecordingAppearanceSettings();

        Should.Throw<ArgumentNullException>(() => ((InputElement)null!).WithCodeFontSizeKeyBindings(settings));
        Should.Throw<ArgumentNullException>(() => new Grid().WithCodeFontSizeKeyBindings(null!));
    }

    private sealed class RecordingAppearanceSettings : IResourceAppearanceSettings
    {
        public List<int> Adjustments { get; } = [];

        public void AdjustConsoleFontSize(int change) => Adjustments.Add(change);
    }
}
