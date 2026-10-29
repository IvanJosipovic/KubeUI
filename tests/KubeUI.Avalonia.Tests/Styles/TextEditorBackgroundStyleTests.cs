using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class TextEditorBackgroundStyleTests
{
    [AvaloniaFact]
    public void all_text_editors_use_alt_high_background_in_each_theme()
    {
        var application = Application.Current!;
        var originalTheme = application.RequestedThemeVariant;
        TextEditor editor = new();
        using var window = application.CreateTestWindow(content: editor);

        try
        {
            window.Show();

            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                application.RequestedThemeVariant = variant;
                Dispatcher.UIThread.RunJobs();

                application.TryGetResource("SystemAltHighColor", variant, out var resource).ShouldBeTrue();
                editor.Background.ShouldBeAssignableTo<ISolidColorBrush>().Color.ShouldBe(resource.ShouldBeOfType<Color>());
            }
        }
        finally
        {
            application.RequestedThemeVariant = originalTheme;
            Dispatcher.UIThread.RunJobs();
        }
    }
}
