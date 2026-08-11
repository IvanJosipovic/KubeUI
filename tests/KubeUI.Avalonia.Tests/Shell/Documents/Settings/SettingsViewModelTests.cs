using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Documents.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Moq;
using Shouldly;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Tests.Shell.Documents.Settings;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void selected_agent_updates_persisted_agent_id()
    {
        var settings = new AppSettings { SelectedAgentId = "first" };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var first = new Mock<IAgent>();
        first.SetupGet(agent => agent.Id).Returns("first");
        first.SetupGet(agent => agent.Name).Returns("First");
        var second = new Mock<IAgent>();
        second.SetupGet(agent => agent.Id).Returns("second");
        second.SetupGet(agent => agent.Name).Returns("Second");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([first.Object, second.Object]);

        using var viewModel = new SettingsViewModel(settingsService.Object, registry.Object);

        viewModel.SelectedAgent.ShouldBeSameAs(first.Object);
        viewModel.SelectedAgent = second.Object;

        settings.SelectedAgentId.ShouldBe("second");
        settingsService.Verify(service => service.SaveSettings(), Times.Once);
    }

    [AvaloniaFact]
    public void settings_view_exposes_agent_selection()
    {
        var settings = new AppSettings { SelectedAgentId = "second", McpServerEnabled = true };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var first = new Mock<IAgent>();
        first.SetupGet(agent => agent.Id).Returns("first");
        first.SetupGet(agent => agent.Name).Returns("First");
        var second = new Mock<IAgent>();
        second.SetupGet(agent => agent.Id).Returns("second");
        second.SetupGet(agent => agent.Name).Returns("Second");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([first.Object, second.Object]);
        using var viewModel = new SettingsViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(500, 500, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var comboBox = view.GetVisualDescendants().OfType<ComboBox>().Single();
        comboBox.ItemsView!.Count.ShouldBe(2);
        comboBox.SelectedItem.ShouldBeSameAs(second.Object);
    }

    [AvaloniaFact]
    public void settings_view_disables_ai_controls_when_mcp_is_disabled()
    {
        var settings = new AppSettings { McpServerEnabled = false };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Id).Returns("test");
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);
        using var viewModel = new SettingsViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(500, 700, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<ComboBox>().Single().IsEffectivelyEnabled.ShouldBeFalse();
        view.GetVisualDescendants().OfType<NumericUpDown>().Single(control => control.Value == settings.McpServerPort)
            .IsEffectivelyEnabled.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void settings_view_exposes_mcp_server_settings()
    {
        var settings = new AppSettings
        {
            McpServerEnabled = true,
            McpServerPort = 63888
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Id).Returns("test");
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);
        using var viewModel = new SettingsViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(500, 700, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var text = view.GetVisualDescendants().OfType<TextBlock>().Select(control => control.Text).ToList();
        text.ShouldContain(Assets.Resources.SettingsView_AppearanceHeading);
        text.ShouldContain(Assets.Resources.SettingsView_AIHeading);
        text.ShouldContain(Assets.Resources.SettingsView_ApplicationHeading);

        view.GetVisualDescendants().OfType<CheckBox>().Select(checkBox => checkBox.IsChecked).ShouldContain(true);
        view.GetVisualDescendants().OfType<NumericUpDown>().Select(control => control.Value).ShouldContain(63888);
    }
}
