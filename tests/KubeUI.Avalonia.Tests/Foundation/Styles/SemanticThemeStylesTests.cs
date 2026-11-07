using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Styling;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Foundation.Styles;

public sealed class SemanticThemeStylesTests
{
    [AvaloniaFact]
    public void Semantic_brushes_follow_dynamic_light_and_dark_theme_changes()
    {
        var application = Application.Current!;
        var originalTheme = application.RequestedThemeVariant;
        var surface = new Border().Background(new DynamicResourceExtension("SemanticSuccessBrush"));
        var window = new Window { Content = surface, Width = 120, Height = 80 };

        try
        {
            window.Show();
            application.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();
            var lightBrush = surface.Background.ShouldBeOfType<SolidColorBrush>();
            lightBrush.Color.ShouldBe(Color.Parse("#2E7D32"));

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            surface.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(Color.Parse("#81C784"));
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = originalTheme;
            Dispatcher.UIThread.RunJobs();
        }
    }
}
