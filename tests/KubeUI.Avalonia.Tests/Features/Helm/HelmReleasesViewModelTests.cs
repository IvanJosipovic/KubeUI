using System.Globalization;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
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
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmReleasesViewModelTests
{
    [AvaloniaFact]
    public void Status_and_updated_columns_fit_content()
    {
        using var viewModel = new HelmReleasesViewModel(
            Mock.Of<IHelmService>(),
            Mock.Of<IFactory>(),
            Mock.Of<IDialogService>(),
            Mock.Of<IHelmReleasePreviewService>(),
            Mock.Of<Func<ResourceYamlViewModel>>(),
            Mock.Of<IHelmDocumentOpener>(),
            Mock.Of<IHelmListNotifier>());

        var columns = viewModel.TableSource.Columns;

        columns.Single(column => column.Key == "status").WidthMode.ShouldBe(DynamicTableViewWidthMode.Auto);
        columns.Single(column => column.Key == "updated").WidthMode.ShouldBe(DynamicTableViewWidthMode.Auto);
        columns.Single(column => column.Key == "name").WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
    }

    [Theory]
    [InlineData("2026-10-21T12:58:46Z", "1m14s")]
    [InlineData("2026-10-21T13:00:00Z", "0ms")]
    public void Updated_column_uses_pretty_age(string updated, string expected)
    {
        HelmReleasesViewModel.FormatUpdated(
                DateTimeOffset.Parse(updated, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                new DateTime(2026, 10, 21, 13, 0, 0, DateTimeKind.Utc))
            .ShouldBe(expected);
    }

    [Fact]
    public void Timestamp_is_formatted_as_utc_iso_8601()
    {
        HelmReleasesViewModel.FormatTimestamp(
                new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)))
            .ShouldBe("2026-01-02T01:04:05Z");
    }

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
            Mock.Of<IHelmReleasePreviewService>(), createViewer.Object, Mock.Of<IHelmDocumentOpener>(),
            Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        await viewModel.ViewValuesCommand.ExecuteAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0", ""));

        viewModel.Status.ShouldBe(Assets.Resources.HelmReleasesView_ValuesFailed);
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
            .ReturnsAsync(json.RootElement.Clone());
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
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        await viewModel.ViewValuesCommand.ExecuteAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0", ""));

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
            .ReturnsAsync(JsonSerializer.SerializeToElement(new { replicas = 4 }));
        await viewModel.ViewValuesCommand.ExecuteAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0", ""));
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
        var release = new HelmRelease("cilium", "kube-system", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "cilium-1.20.3", "");
        helm.SetupSequence(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([release])
            .ReturnsAsync([release, new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0.0", "")]);
        using var viewModel = new HelmReleasesViewModel(
            helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(), Mock.Of<IHelmReleasePreviewService>(),
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        var view = new HelmReleasesView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            var toolbar = view.GetVisualDescendants().OfType<StackPanel>()
                .Single(panel => panel.Orientation == global::Avalonia.Layout.Orientation.Horizontal
                    && panel.Children.OfType<Ursa.Controls.MultiComboBox>().Any());
            toolbar.Children[0].ShouldBeOfType<TextBlock>();
            await TestWait.UntilAsync(() => viewModel.ItemCount == 1,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            await TestWait.UntilAsync(
                () => view.GetVisualDescendants().OfType<TextBlock>().Any(textBlock => textBlock.Name == "CellTextBlock"),
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            view.GetVisualDescendants().OfType<TextBlock>()
                .Single(textBlock => textBlock.Name == "CellTextBlock")
                .Text.ShouldBe(HelmReleasesViewModel.FormatUpdated(release.Updated, DateTime.UtcNow));
            window.Content = null;
            await TestApplicationExtensions.WaitForUiAsync();
            window.Content = view;
            await TestWait.UntilAsync(() => viewModel.ItemCount == 2,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            helm.Verify(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
                It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
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
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        viewModel.VisualizeReleaseCommand.Execute(new HelmRelease("cilium", "kube-system", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "cilium-1.20.3", ""));

        viewModel.Status.ShouldBe(Assets.Resources.HelmReleasesView_VisualizeFailed);
    }

    [AvaloniaFact]
    public async Task Visualize_release_does_not_render_chart_or_fetch_values()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(JsonSerializer.SerializeToElement(new Dictionary<string, object?>()));
        var preview = new Mock<IHelmReleasePreviewService>();
        using var viewModel = new HelmReleasesViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(), preview.Object,
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        var release = new HelmRelease("cilium", "kube-system", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "cilium-1.20.3", "");

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
            new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0.0", "1.0.0"),
            new HelmRelease("dns", "system", 1, new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero), "deployed", "dns-1.0.0", "1.0.0"),
            new HelmRelease("other", "other", 1, new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero), "deployed", "other-1.0.0", "1.0.0")
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
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());

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
        var release = new HelmRelease("web", "apps", 2, new DateTimeOffset(2026, 01, 02, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.1.0", "1.1.0");
        helmService
            .Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRelease>());
        helmService
            .Setup(service => service.StatusAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        helmService
            .Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.SerializeToElement(new Dictionary<string, object?>()));
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
            () => yamlViewer,
            Mock.Of<IHelmDocumentOpener>(),
            Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);

        await viewModel.ViewStatusCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldBe(Assets.Resources.HelmReleasesView_StatusLoaded);
        await viewModel.ViewValuesCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldBeEmpty();
        yamlViewer.YamlDocument.Text.ShouldContain("{}");
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
        dialogs[0].Content.ShouldBeOfType<StackPanel>().Children.OfType<CheckBox>().ShouldBeEmpty();
        dialogs[1].Content.ShouldBeOfType<StackPanel>().Children.OfType<CheckBox>().ShouldHaveSingleItem();
    }

    [AvaloniaFact]
    public async Task Release_list_refreshes_when_an_editor_reports_a_change()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        workspace.SelectedNamespaces.Clear();
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var notifier = new HelmListNotifier();
        using var viewModel = new HelmReleasesViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(),
            Mock.Of<IHelmReleasePreviewService>(),
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), notifier);
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.ItemCount.ShouldBe(0);

        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0", "")]);
        notifier.NotifyReleasesChanged();

        await TestWait.UntilAsync(() => viewModel.ItemCount == 1,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
    }

    [AvaloniaFact]
    public async Task Release_editor_commands_open_documents_through_the_opener()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var opener = new Mock<IHelmDocumentOpener>();
        using var viewModel = new HelmReleasesViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IDialogService>(),
            Mock.Of<IHelmReleasePreviewService>(),
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            opener.Object, Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        var release = new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0", "");

        viewModel.NewReleaseCommand.Execute(null);
        viewModel.UpgradeReleaseCommand.Execute(release);

        opener.Verify(item => item.OpenReleaseEditor(workspace,
            It.Is<HelmConnection>(connection => connection.KubeConfigPath == workspace.Runtime.KubeConfigPath), null),
            Times.Once);
        opener.Verify(item => item.OpenReleaseEditor(workspace,
            It.Is<HelmConnection>(connection => connection.KubeConfigPath == workspace.Runtime.KubeConfigPath), release),
            Times.Once);
    }
}
