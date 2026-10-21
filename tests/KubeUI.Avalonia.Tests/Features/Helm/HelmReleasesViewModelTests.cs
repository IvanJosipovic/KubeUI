using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmReleasesViewModelTests
{
    [AvaloniaFact]
    public async Task View_values_reports_fetch_errors_without_creating_yaml_viewer()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Values unavailable"));
        var createViewer = new Mock<Func<ResourceYamlViewModel>>();
        using var viewModel = new HelmReleasesViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(),
            Mock.Of<IHelmReleasePreviewService>(), createViewer.Object);
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);

        await viewModel.ViewValuesCommand.ExecuteAsync(new HelmRelease("web", "apps", "1", "", "deployed", "web-1.0", ""));

        viewModel.Status.ShouldBe("Values unavailable");
        createViewer.Verify(factory => factory(), Times.Never);
    }

    [AvaloniaFact]
    public async Task View_values_opens_locked_yaml_document_instead_of_toolbar_json()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        using var json = JsonDocument.Parse("""{"enabled":true,"replicas":2,"server":{"name":"true","ports":[80,443]},"unset":null}""");
        helm.Setup(service => service.GetValuesAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(json.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone()));
        var factory = new Mock<IFactory>();
        ResourceYamlViewModel? opened = null;
        var bottom = Mock.Of<IToolDock>();
        bottom.VisibleDockables = new System.Collections.ObjectModel.ObservableCollection<IDockable>();
        factory.Setup(item => item.GetDockable<IToolDock>("BottomDock")).Returns(bottom);
        factory.Setup(item => item.InsertDockable(bottom, It.IsAny<IDockable>(), 0))
            .Callback<IDockable, IDockable, int>((_, document, _) =>
            {
                opened = document.ShouldBeOfType<ResourceYamlViewModel>();
                bottom.VisibleDockables.Add(document);
            });
        using var viewModel = new HelmReleasesViewModel(
            helm.Object, factory.Object, Mock.Of<IDialogService>(), Mock.Of<IHelmReleasePreviewService>(),
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>());
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);

        await viewModel.ViewValuesCommand.ExecuteAsync(new HelmRelease("web", "apps", "1", "", "deployed", "web-1.0", ""));

        viewModel.Status.ShouldBeEmpty();
        using var yaml = opened.ShouldNotBeNull();
        yaml.EditMode.ShouldBeFalse();
        yaml.IsReadOnlyLocked.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => yaml.EditMode = true);
        yaml.SetEditModeCommand.CanExecute(null).ShouldBeFalse();
        yaml.SaveCommand.CanExecute(null).ShouldBeFalse();
        yaml.DryRunCommand.CanExecute(null).ShouldBeFalse();
        yaml.Title.ShouldContain("web");
        yaml.Title.ShouldContain("apps");
        yaml.YamlDocument.Text.ShouldContain("enabled: true");
        yaml.YamlDocument.Text.ShouldContain("replicas: 2");
        yaml.YamlDocument.Text.TrimStart().ShouldNotStartWith("{");
        var parsed = Kubernetes.Serialization.KubernetesYaml.Deserialize<Dictionary<string, object>>(yaml.YamlDocument.Text);
        parsed.ShouldContainKey("server");
        parsed.ShouldContainKey("unset");
        parsed["enabled"].ShouldBe(true);
        parsed["replicas"].ShouldBe(2);
        parsed["unset"].ShouldBeNull();
        var server = parsed["server"].ShouldBeOfType<Dictionary<object, object>>();
        server["name"].ShouldBe("true");
        server["ports"].ShouldBeOfType<List<object>>().ShouldBe([80, 443]);
        viewModel.Status.ShouldBeEmpty();

        var view = new ResourceYamlView { DataContext = yaml };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor").ShouldNotBeNull();
            editor.IsReadOnly.ShouldBeTrue();
            editor.Text.ShouldBe(yaml.YamlDocument.Text);
            yaml.ValidationDiagnostics.ShouldBeEmpty();
            view.FindControl<global::Avalonia.Controls.Primitives.ToggleButton>("HideNoisyFieldsToggle")!
                .IsVisible.ShouldBeFalse();
            yaml.HideNoisyFields = !yaml.HideNoisyFields;
            editor.Text.ShouldBe(yaml.YamlDocument.Text);
            editor.Text.ShouldContain("enabled: true");
        }
        finally
        {
            window.Close();
        }

        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, JsonElement> { ["replicas"] = JsonSerializer.SerializeToElement(4) });
        await viewModel.ViewValuesCommand.ExecuteAsync(new HelmRelease("web", "apps", "1", "", "deployed", "web-1.0", ""));
        viewModel.Status.ShouldBeEmpty();
        yaml.YamlDocument.Text.ShouldContain("replicas: 4");
        factory.Verify(item => item.InsertDockable(bottom, It.IsAny<IDockable>(), It.IsAny<int>()), Times.Once);
        factory.Verify(item => item.SetActiveDockable(yaml), Times.Exactly(2));
    }

    [AvaloniaFact]
    public async Task Loading_release_view_refreshes_all_namespace_data_each_time()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        workspace.SelectedNamespaces.Clear();
        var helm = new Mock<IHelmService>();
        var release = new HelmRelease("cilium", "kube-system", "1", "", "deployed", "cilium-1.20.3", "");
        helm.SetupSequence(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .ReturnsAsync([release])
            .ReturnsAsync([release, new HelmRelease("web", "apps", "1", "", "deployed", "web-1.0.0", "")]);
        using var viewModel = new HelmReleasesViewModel(
            helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(), Mock.Of<IHelmReleasePreviewService>(),
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>());
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        var view = new HelmReleasesView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestWait.UntilAsync(() => viewModel.ItemCount == 1,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            window.Content = null;
            await TestApplicationExtensions.WaitForUiAsync();
            window.Content = view;
            await TestWait.UntilAsync(() => viewModel.ItemCount == 2,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            helm.Verify(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
                It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Visualize_release_surfaces_open_errors_in_status()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var preview = new Mock<IHelmReleasePreviewService>();
        preview.Setup(service => service.VisualizeRelease(workspace, "cilium", "kube-system"))
            .Throws(new InvalidOperationException("Unable to open visualization"));
        using var viewModel = new HelmReleasesViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(), preview.Object,
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>());
        viewModel.Initialize(workspace);
        viewModel.VisualizeReleaseCommand.Execute(new HelmRelease("cilium", "kube-system", "1", "", "deployed", "cilium-1.20.3", ""));

        viewModel.Status.ShouldBe("Unable to open visualization");
    }

    [AvaloniaFact]
    public async Task Visualize_release_does_not_render_chart_or_fetch_values()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<string, JsonElement>());
        var preview = new Mock<IHelmReleasePreviewService>();
        using var viewModel = new HelmReleasesViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(), preview.Object,
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>());
        viewModel.Initialize(workspace);
        var release = new HelmRelease("cilium", "kube-system", "1", "", "deployed", "cilium-1.20.3", "");

        viewModel.VisualizeReleaseCommand.Execute(release);

        helm.Verify(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        preview.Verify(service => service.PreviewAsync(It.IsAny<ClusterWorkspace>(), It.IsAny<HelmConnection>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        preview.Verify(service => service.VisualizeRelease(workspace, "cilium", "kube-system"), Times.Once);
    }

    [AvaloniaFact]
    public async Task Release_filter_follows_linked_and_local_namespace_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        workspace.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "apps" } });
        workspace.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "system" } });
        var releases = new[]
        {
            new HelmRelease("web", "apps", "1", "2026-01-01", "deployed", "nginx-1.0.0", "1.0.0"),
            new HelmRelease("dns", "system", "1", "2026-01-01", "deployed", "dns-1.0.0", "1.0.0"),
            new HelmRelease("other", "other", "1", "2026-01-01", "deployed", "other-1.0.0", "1.0.0")
        };
        var helmService = new Mock<IHelmService>();
        var previewService = new Mock<IHelmReleasePreviewService>();
        helmService
            .Setup(service => service.ListReleasesAsync(
                It.IsAny<HelmConnection>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(releases);
        var factory = new Mock<IFactory>();
        var dialogs = new Mock<IDialogService>();
        using var viewModel = new HelmReleasesViewModel(helmService.Object, factory.Object, dialogs.Object, previewService.Object,
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>());

        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        ReferenceEquals(viewModel.SelectedNamespaces, workspace.SelectedNamespaces).ShouldBeTrue();
        await TestWait.UntilAsync(() => viewModel.ItemCount == 2, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        helmService.Verify(service => service.ListReleasesAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            true,
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        workspace.SelectedNamespaces.RemoveAt(1);
        await TestWait.UntilAsync(() => viewModel.ItemCount == 1, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

        viewModel.IsNamespaceSelectionLinked = false;
        viewModel.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "system" } });
        await TestWait.UntilAsync(() => viewModel.ItemCount == 2, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        workspace.SelectedNamespaces.Count.ShouldBe(1);

        viewModel.SelectedNamespaces.Clear();
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(() => viewModel.ItemCount == releases.Length, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
    }

    [AvaloniaFact]
    public async Task Selected_release_actions_call_helm_and_present_results()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        var previewService = new Mock<IHelmReleasePreviewService>();
        var release = new HelmRelease("web", "apps", "2", "2026-01-02", "deployed", "nginx-1.1.0", "1.1.0");
        helmService
            .Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRelease>());
        helmService
            .Setup(service => service.StatusAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        helmService
            .Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, JsonElement>());
        helmService
            .Setup(service => service.HistoryAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync([release]);
        helmService
            .Setup(service => service.RollbackAsync(It.IsAny<HelmConnection>(), release.Name, 0, It.IsAny<CancellationToken>(), false))
            .Returns(Task.CompletedTask);
        helmService
            .Setup(service => service.UninstallAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>(), false))
            .Returns(Task.CompletedTask);
        var factory = new Mock<IFactory>();
        factory.Setup(item => item.GetDockable<IToolDock>("BottomDock")).Returns(Mock.Of<IToolDock>());
        using var yamlViewer = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        List<ContentDialogSettings> dialogs = [];
        using var viewModel = new HelmReleasesViewModel(
            helmService.Object,
            factory.Object,
            HelmDialogTestHelper.CreateConfirmingDialogService(dialogs),
            previewService.Object,
            () => yamlViewer);
        viewModel.Initialize(workspace);

        await viewModel.ViewStatusCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldContain("deployed");
        await viewModel.ViewValuesCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldBeEmpty();
        yamlViewer.YamlDocument.Text.ShouldContain("{}");
        await viewModel.ViewHistoryCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldContain("2: deployed");

        await viewModel.RollbackCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.VisualizeReleaseCommand.Execute(release);
        await viewModel.UninstallCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.RollbackAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            release.Name,
            0,
            It.IsAny<CancellationToken>(),
            false), Times.Once);
        helmService.Verify(service => service.UninstallAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == release.Namespace),
            release.Name,
            It.IsAny<CancellationToken>(),
            false), Times.Once);
        previewService.Verify(service => service.VisualizeRelease(
            workspace, release.Name, release.Namespace), Times.Once);
        dialogs.Count.ShouldBe(2);
        dialogs[0].Content.ShouldBeOfType<StackPanel>().Children.OfType<NumericUpDown>().ShouldHaveSingleItem();
        dialogs[0].Content.ShouldBeOfType<StackPanel>().Children.OfType<CheckBox>().ShouldHaveSingleItem();
        dialogs[1].Content.ShouldBeOfType<StackPanel>().Children.OfType<CheckBox>().ShouldHaveSingleItem();
    }
}
