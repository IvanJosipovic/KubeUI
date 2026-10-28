using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Shell.Documents.Settings;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Tests.Shell.Documents.Settings;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void selected_agent_updates_persisted_agent_id()
    {
        var settings = new AppSettings { SelectedAgentId = "missing" };
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

        using var viewModel = CreateViewModel(settingsService.Object, registry.Object);

        viewModel.SelectedAgent.ShouldBeSameAs(first.Object);
        viewModel.SelectedAgent = second.Object;
        viewModel.SelectedAgent = second.Object;

        settings.SelectedAgentId.ShouldBe("second");
        settingsService.Verify(service => service.SaveSettings(), Times.Once);
    }

    [Fact]
    public void mcp_server_port_clamps_to_valid_bounds_and_saves_only_changes()
    {
        var settings = new AppSettings { McpServerPort = 62888 };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        using var viewModel = CreateViewModel(settingsService.Object);

        viewModel.McpServerPort = 70000;
        settings.McpServerPort.ShouldBe(65535);
        viewModel.McpServerPort = 65535;
        viewModel.McpServerPort = 80;
        settings.McpServerPort.ShouldBe(1024);

        settingsService.Verify(service => service.SaveSettings(), Times.Exactly(2));
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
        using var viewModel = CreateViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(500, 500, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var comboBox = view.GetVisualDescendants().OfType<ComboBox>()
            .Single(control => control.ItemsSource?.Cast<object>().FirstOrDefault() is IAgent);
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
        using var viewModel = CreateViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(500, 700, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<ComboBox>()
            .Single(control => control.ItemsSource?.Cast<object>().FirstOrDefault() is IAgent)
            .IsEffectivelyEnabled.ShouldBeFalse();
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
        using var viewModel = CreateViewModel(settingsService.Object, registry.Object);
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

    [AvaloniaFact]
    public void settings_view_exposes_preview_feature_toggles()
    {
        var settings = new AppSettings
        {
            Features = new FeatureSettings
            {
                Helm = true,
                Hubble = false,
                JsonAllowReflection = false,
                YamlAllowReflection = true
            }
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);
        using var viewModel = CreateViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(600, 900, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text)
            .ShouldContain(Assets.Resources.SettingsView_FeaturesHeading);

        var helmRow = FindToggleRow(view, Assets.Resources.SettingsView_HelmFeature_Label!);
        var hubbleRow = FindToggleRow(view, Assets.Resources.SettingsView_HubbleFeature_Label!);
        var jsonRow = FindToggleRow(view, Assets.Resources.SettingsView_JsonAllowReflection_Label!);
        var yamlRow = FindToggleRow(view, Assets.Resources.SettingsView_YamlAllowReflection_Label!);
        var helm = helmRow.Children.OfType<CheckBox>().Single();
        var hubble = hubbleRow.Children.OfType<CheckBox>().Single();
        var json = jsonRow.Children.OfType<CheckBox>().Single();
        var yaml = yamlRow.Children.OfType<CheckBox>().Single();

        helm.IsChecked.ShouldBe(true);
        hubble.IsChecked.ShouldBe(false);
        json.IsChecked.ShouldBe(false);
        yaml.IsChecked.ShouldBe(true);

        helm.IsChecked = false;
        hubble.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        settings.PreviewFeatures.Helm.ShouldBeFalse();
        settings.PreviewFeatures.Hubble.ShouldBeTrue();
        json.IsChecked = true;
        yaml.IsChecked = false;
        Dispatcher.UIThread.RunJobs();
        settings.PreviewFeatures.JsonAllowReflection.ShouldBeTrue();
        settings.PreviewFeatures.YamlAllowReflection.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void settings_view_shows_cluster_scope_as_checkboxes()
    {
        var settings = new AppSettings { McpServerEnabled = true };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var dev = new Mock<IClusterRuntime>();
        dev.SetupGet(cluster => cluster.Name).Returns("dev");
        var prod = new Mock<IClusterRuntime>();
        prod.SetupGet(cluster => cluster.Name).Returns("prod");
        var clusterCatalog = new Mock<IClusterRuntimeCatalog>();
        clusterCatalog.SetupGet(catalog => catalog.Clusters).Returns([dev.Object, prod.Object]);
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);
        using var viewModel = CreateViewModel(
            settingsService.Object,
            registry.Object,
            clusterCatalog: clusterCatalog.Object,
            mcpServerState: CreateBoundMcpServerState());
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(600, 800, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var prodCheckbox = view.GetVisualDescendants().OfType<CheckBox>()
            .Single(checkbox => checkbox.Content?.ToString() == "prod");
        prodCheckbox.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        viewModel.ExternalMcpClusterOptions.Single(option => option.Name == "prod").IsSelected.ShouldBeTrue();
        view.GetVisualDescendants().OfType<TextBlock>().Select(textBlock => textBlock.Text)
            .ShouldContain(Assets.Resources.SettingsView_McpClients_Heading);
    }

    [Fact]
    public void external_mcp_credentials_are_shown_once_and_can_be_revoked()
    {
        var settings = new AppSettings();
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        var dev = new Mock<IClusterRuntime>();
        dev.SetupGet(cluster => cluster.Name).Returns("dev");
        var prod = new Mock<IClusterRuntime>();
        prod.SetupGet(cluster => cluster.Name).Returns("prod");
        var clusterCatalog = new Mock<IClusterRuntimeCatalog>();
        clusterCatalog.SetupGet(catalog => catalog.Clusters).Returns([dev.Object, prod.Object]);
        using var viewModel = CreateViewModel(
            settingsService.Object,
            clusterCatalog: clusterCatalog.Object,
            mcpServerState: CreateBoundMcpServerState(),
            mcpCredentialService: new McpCredentialService(settingsService.Object));
        viewModel.ExternalMcpClientName = "Codex";
        viewModel.ExternalMcpClusterOptions.Single(option => option.Name == "dev").IsSelected = true;
        viewModel.ExternalMcpClusterOptions.Single(option => option.Name == "prod").IsSelected = true;

        viewModel.CreateExternalMcpCredentialCommand.Execute(null);

        var token = viewModel.NewlyCreatedMcpToken;
        token.ShouldNotBeNullOrWhiteSpace();
        settings.McpExternalClientGrants.Count.ShouldBe(1);
        settings.McpExternalClientGrants[0].TokenVerifier.ShouldNotBe(token);
        settings.McpExternalClientGrants[0].AllowedClusters.ShouldBe(["dev", "prod"]);
        viewModel.SelectedExternalMcpGrant.ShouldBe(settings.McpExternalClientGrants[0]);

        viewModel.RevokeExternalMcpCredentialCommand.Execute(null);

        settings.McpExternalClientGrants.ShouldBeEmpty();
        viewModel.NewlyCreatedMcpToken.ShouldBeNull();
        settingsService.Verify(service => service.TrySaveSettings(), Times.Exactly(2));
    }

    [Fact]
    public void external_mcp_credential_requires_client_name_and_reports_validation_error()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(new AppSettings());
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        using var viewModel = CreateViewModel(settingsService.Object);

        viewModel.RevokeExternalMcpCredentialCommand.Execute(null);
        viewModel.CreateExternalMcpCredentialCommand.Execute(null);

        viewModel.NewlyCreatedMcpToken.ShouldBeNull();
        var validation = viewModel.ShouldBeAssignableTo<INotifyDataErrorInfo>();
        validation.HasErrors.ShouldBeTrue();
        validation.GetErrors(nameof(SettingsViewModel.ExternalMcpClientName))
            .Cast<object>()
            .ShouldContain("Enter a client name.");
        validation.GetErrors("UnknownProperty").Cast<object>().ShouldBeEmpty();
        viewModel.McpGrantMessage.ShouldBeNull();
        settingsService.Verify(service => service.TrySaveSettings(), Times.Never);
    }

    [Fact]
    public void external_mcp_client_name_length_reports_validation_error()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(new AppSettings());
        using var viewModel = CreateViewModel(settingsService.Object);
        viewModel.ExternalMcpClientName = new string('x', 81);

        viewModel.CreateExternalMcpCredentialCommand.Execute(null);

        viewModel.HasErrors.ShouldBeTrue();
        viewModel.GetErrors(nameof(SettingsViewModel.ExternalMcpClientName))
            .Cast<object>()
            .ShouldContain(Assets.Resources.SettingsView_McpClientName_TooLong);
        settingsService.Verify(service => service.TrySaveSettings(), Times.Never);
    }

    [Fact]
    public void external_mcp_credential_rejects_more_than_one_hundred_selected_clusters()
    {
        var settings = new AppSettings();
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        var runtimes = Enumerable.Range(0, 101)
            .Select(index =>
            {
                var runtime = new Mock<IClusterRuntime>();
                runtime.SetupGet(cluster => cluster.Name).Returns($"cluster-{index}");
                return runtime.Object;
            })
            .ToArray();
        var clusterCatalog = new Mock<IClusterRuntimeCatalog>();
        clusterCatalog.SetupGet(catalog => catalog.Clusters).Returns(runtimes);
        using var viewModel = CreateViewModel(
            settingsService.Object,
            clusterCatalog: clusterCatalog.Object,
            mcpServerState: CreateBoundMcpServerState(),
            mcpCredentialService: new McpCredentialService(settingsService.Object));
        viewModel.ExternalMcpClientName = "large-scope";
        foreach (var option in viewModel.ExternalMcpClusterOptions)
            option.IsSelected = true;

        viewModel.CreateExternalMcpCredentialCommand.Execute(null);

        viewModel.NewlyCreatedMcpToken.ShouldBeNull();
        viewModel.McpGrantMessage.ShouldBe(Assets.Resources.SettingsView_McpTokenInvalid);
        settings.McpExternalClientGrants.ShouldBeEmpty();
        settingsService.Verify(service => service.TrySaveSettings(), Times.Never);
    }

    [AvaloniaFact]
    public void empty_client_name_uses_native_textbox_validation()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(new AppSettings { McpServerEnabled = true });
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);
        using var viewModel = CreateViewModel(
            settingsService.Object,
            registry.Object,
            mcpServerState: CreateBoundMcpServerState());
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(600, 800, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();
        viewModel.CreateExternalMcpCredentialCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<TextBox>()
            .Where(DataValidationErrors.GetHasErrors)
            .ShouldHaveSingleItem();
    }

    [Fact]
    public void external_mcp_credential_creation_reports_storage_failure()
    {
        var settings = new AppSettings();
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(false);
        using var viewModel = CreateViewModel(
            settingsService.Object,
            mcpServerState: CreateBoundMcpServerState(),
            mcpCredentialService: new McpCredentialService(settingsService.Object));
        viewModel.ExternalMcpClientName = "Codex";

        viewModel.CreateExternalMcpCredentialCommand.Execute(null);

        viewModel.NewlyCreatedMcpToken.ShouldBeNull();
        viewModel.McpGrantMessage.ShouldBe(Assets.Resources.SettingsView_McpTokenCreateFailed);
        settings.McpExternalClientGrants.ShouldBeEmpty();
    }

    [Fact]
    public void external_mcp_credential_revoke_reports_unsaved_change()
    {
        var settings = new AppSettings();
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.SetupSequence(service => service.TrySaveSettings()).Returns(true).Returns(false);
        using var viewModel = CreateViewModel(
            settingsService.Object,
            mcpServerState: CreateBoundMcpServerState(),
            mcpCredentialService: new McpCredentialService(settingsService.Object));
        viewModel.ExternalMcpClientName = "Copilot";
        viewModel.CreateExternalMcpCredentialCommand.Execute(null);
        var token = viewModel.NewlyCreatedMcpToken;

        viewModel.RevokeExternalMcpCredentialCommand.Execute(null);

        viewModel.McpGrantMessage.ShouldBe(Assets.Resources.SettingsView_McpTokenRevokeFailed);
        viewModel.NewlyCreatedMcpToken.ShouldBe(token);
        settings.McpExternalClientGrants.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public void mcp_client_name_box_shows_placeholder_text()
    {
        var settings = new AppSettings { McpServerEnabled = true };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var agent = new Mock<IAgent>();
        agent.SetupGet(value => value.Id).Returns("test");
        agent.SetupGet(value => value.Name).Returns("Test agent");
        var registry = new Mock<IAgentRegistry>();
        registry.SetupGet(value => value.Agents).Returns([agent.Object]);
        using var viewModel = CreateViewModel(settingsService.Object, registry.Object);
        var view = new SettingsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(500, 700, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var clientNameBox = view.GetVisualDescendants().OfType<TextBox>()
            .Single(textBox => textBox.Name == nameof(SettingsViewModel.ExternalMcpClientName));

        clientNameBox.PlaceholderText.ShouldBe(Assets.Resources.SettingsView_McpClientName_Watermark);
    }

    [Fact]
    public void settings_view_members_are_not_marked_obsolete()
    {
        typeof(SettingsView)
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(member => member.GetCustomAttribute<ObsoleteAttribute>() is not null)
            .Select(member => member.Name)
            .ShouldBeEmpty();
    }

    private static Grid FindToggleRow(SettingsView view, string label)
    {
        return view.GetVisualDescendants()
            .OfType<Grid>()
            .Single(grid => grid.Children.OfType<Label>().Any(control => Equals(control.Content, label)));
    }

    private static McpServerState CreateBoundMcpServerState()
    {
        var state = new McpServerState();
        state.SetBoundPort(62888);
        return state;
    }

    private static SettingsViewModel CreateViewModel(
        ISettingsService settingsService,
        IAgentRegistry? agentRegistry = null,
        IClusterRuntimeCatalog? clusterCatalog = null,
        IMcpServerState? mcpServerState = null,
        IMcpCredentialService? mcpCredentialService = null)
    {
        return new SettingsViewModel(
            settingsService,
            mcpCredentialService ?? Mock.Of<IMcpCredentialService>(),
            agentRegistry,
            clusterCatalog,
            mcpServerState);
    }
}
