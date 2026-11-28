using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.App.Infrastructure.Mcp;
using KubeUI.App.Options;
using KubeUI.App.Services.Settings;
using KubeUI.App.Shell.Documents.Settings;
using KubeUI.App.Tests.Infra;
using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Mcp;
using Moq;
using Shouldly;
using AppSettings = KubeUI.App.Options.Settings;

namespace KubeUI.App.Tests.App.Shell;

/// <summary>Renders the Settings document for programmatic visual-evidence capture.</summary>
public sealed class SettingsViewScreenshotTests
{
    [AvaloniaFact]
    public void preview_features_section_renders()
    {
        var settings = new AppSettings
        {
            PreviewFeatures = new FeatureSettings
            {
                Hubble = false,
                DisableJsonSerializationReflection = false,
                DisableYamlSerializationReflection = true
            }
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);

        using var viewModel = new SettingsViewModel(
            settingsService.Object,
            Mock.Of<IMcpCredentialService>(),
            registry.Object,
            null,
            new McpServerState());
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(800, 1600, view);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<TextBlock>()
            .ShouldContain(text => text.Text == AppLanguageManager.Instance.SettingsView_FeaturesHeading.CurrentValue());

        TestScreenshot.CaptureIfEnabled(window, "SettingsView_PreviewFeatures");
    }
}
