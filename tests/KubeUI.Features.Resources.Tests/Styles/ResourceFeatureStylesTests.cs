using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Styling;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Styles;

public sealed class ResourceFeatureStylesTests
{
    [AvaloniaFact]
    public void Kubernetes_status_and_relationship_brushes_follow_theme_changes()
    {
        var application = Application.Current!;
        var originalTheme = application.RequestedThemeVariant;
        var status = new Border().Background(new DynamicResourceExtension("PodStatusReadyBrush"));
        var relationship = new Border().Background(new DynamicResourceExtension("VisualizationRelationshipStorageBrush"));
        var window = new Window
        {
            Content = new StackPanel { Children = { status, relationship } },
            Width = 120,
            Height = 80
        };

        try
        {
            window.Show();
            application.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();
            status.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(Color.Parse("#2E7D32"));
            relationship.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(Color.Parse("#008577"));

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            status.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(Color.Parse("#81C784"));
            relationship.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(Color.Parse("#40D9C0"));
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = originalTheme;
            Dispatcher.UIThread.RunJobs();
        }
    }
}
