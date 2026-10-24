using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using Helm.NET;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmEditorViewModelTests
{
    [AvaloniaTheory]
    [InlineData("no-repositories", false)]
    [InlineData("no-repositories", true)]
    [InlineData("no-exact-match", false)]
    [InlineData("no-exact-match", true)]
    [InlineData("index-unavailable", false)]
    [InlineData("index-unavailable", true)]
    public async Task Upgrade_without_resolvable_repository_preserves_values_and_allows_explicit_preview(
        string scenario, bool hasChartMetadata)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>(MockBehavior.Strict);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario == "no-repositories" ? [] : [new("other", "https://other.test")]);
        using var json = System.Text.Json.JsonDocument.Parse("""{"replicas":3,"server":{"enabled":true}}""");
        helm.Setup(service => service.GetValuesAsync(
                It.Is<HelmConnection>(connection => connection.Namespace == "apps"), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(json.RootElement.EnumerateObject()
                .ToDictionary(property => property.Name, property => property.Value.Clone()));
        var metadata = new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0", "", "nginx", "1.0");
        if (!hasChartMetadata)
        {
            helm.Setup(service => service.StatusAsync(
                    It.Is<HelmConnection>(connection => connection.Namespace == "apps"), "web", It.IsAny<CancellationToken>()))
                .ReturnsAsync(metadata);
        }

        var search = helm.Setup(service => service.ListChartsAsync(
            It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        if (scenario == "index-unavailable")
        {
            search.ThrowsAsync(new InvalidOperationException("Repository index unavailable"));
        }
        else
        {
            search.ReturnsAsync(scenario == "no-exact-match"
                ? [new("other/nginx-sidecar", "nginx-sidecar", "9.0")]
                : []);
        }

        var preview = new Mock<IHelmReleasePreviewService>(MockBehavior.Strict);
        var factory = new Mock<IFactory>();
        var editor = new HelmReleaseEditorViewModel(helm.Object, factory.Object, Mock.Of<IHelmListNotifier>(),
            preview.Object, Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "wrong"),
            hasChartMetadata ? metadata : metadata with { ChartName = "", ChartVersion = "" });
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);

        editor.IsLoading.ShouldBeFalse();
        editor.Error.ShouldBe(scenario == "index-unavailable"
            ? "Repository index unavailable"
            : Assets.Resources.HelmReleaseEditorView_ChooseChartReference);
        editor.Chart.ShouldBeEmpty();
        editor.SelectedRepository.ShouldBeNull();
        editor.SelectedChart.ShouldBeNull();
        editor.AvailableCharts.ShouldBeEmpty();
        editor.ValuesYaml.ShouldContain("replicas: 3");
        editor.ValuesYaml.ShouldContain("enabled: true");
        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        editor.PreviewCommand.CanExecute(null).ShouldBeFalse();
        preview.Invocations.ShouldBeEmpty();

        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var textEditor = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextEditor>().Single();
            textEditor.IsReadOnly.ShouldBeFalse();
            textEditor.Text = "replicas: 4\nserver:\n  enabled: true\n";
            editor.ValuesYaml.ShouldBe(textEditor.Text);
            foreach (var reference in new[] { "manual/nginx", "oci://registry.test/charts/nginx" })
            {
                editor.Chart = reference;
                editor.AvailableVersions.ShouldBe(["1.0"]);
                editor.SelectedVersion.ShouldBe("1.0");
                editor.SelectedRepository.ShouldBeNull();
                editor.SelectedChart.ShouldBeNull();
                editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
                preview.SetupSequence(service => service.PreviewVersionAsync(workspace,
                        It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
                        "web", reference, "1.0",
                        It.Is<IReadOnlyDictionary<string, object?>>(values => Convert.ToInt64(values["replicas"]) == 4),
                        It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("Chart reference cannot be resolved"))
                    .Returns(Task.CompletedTask);
                await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
                editor.Error.ShouldBe("Chart reference cannot be resolved");
                editor.Chart.ShouldBe(reference);
                editor.SelectedVersion.ShouldBe("1.0");
                editor.ValuesYaml.ShouldBe("replicas: 4\nserver:\n  enabled: true\n");
                editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
                await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
                editor.Error.ShouldBeEmpty();
            }
        }
        finally
        {
            window.Close();
        }

        preview.VerifyAll();
        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        helm.Verify(service => service.UpgradeAsync(
            It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Never);
    }

    [AvaloniaFact]
    public async Task Repository_and_chart_dropdowns_filter_versions_and_clear_stale_selections()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("first", "https://first.test"), new("second", "https://second.test")]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, System.Text.Json.JsonElement>());
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new("first/web", "web", "1.0"), new("second/web", "web", "2.0"),
                new("second/other", "other", "3.0"), new("second/other", "other", "4.0")]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"),
            new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "web-1.0", "", "web", "1.0"));
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);

        editor.Repositories.ShouldBe(["first", "second"]);
        editor.SelectedRepository.ShouldBeNull();
        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var dropdowns = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().ToDictionary(control => control.Name!);
            dropdowns["Repository"].SelectedItem = "second";
            editor.AvailableCharts.ShouldBe(["web"]);
            dropdowns["Chart"].SelectedItem = "web";
            editor.Chart.ShouldBe("second/web");
            editor.AvailableVersions.ShouldBe(["2.0", "1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");
            dropdowns["ChartVersion"].SelectedItem = "2.0";
            dropdowns["Repository"].SelectedItem = "first";
            editor.Chart.ShouldBe("first/web");
            editor.AvailableVersions.ShouldBe(["1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");

            editor.SelectedRepository = "second";
            editor.ChartSearch = "other";
            await editor.SearchChartsCommand.ExecuteAsync(null);
            editor.AvailableCharts.ShouldBe(["other"]);
            editor.SelectedChart.ShouldBeNull();
            editor.Chart.ShouldBeEmpty();
            editor.SelectedChart = "other";
            editor.Chart.ShouldBe("second/other");
            editor.AvailableVersions.ShouldBe(["3.0", "4.0"]);
            editor.SelectedVersion.ShouldBeNull();
            editor.SelectedVersion = "4.0";
            editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
            editor.Chart = "oci://registry.test/web";
            editor.SelectedChart.ShouldBeNull();
            editor.AvailableVersions.ShouldBe(["1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Install_chart_search_surfaces_no_matches_or_repository_errors(bool fails)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo", "https://example.test")]);
        var search = helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        if (fails)
        {
            search.ThrowsAsync(new InvalidOperationException("Repository index unavailable"));
        }
        else
        {
            search.ReturnsAsync([]);
        }

        var editor = new HelmReleaseEditorViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.Repositories.ShouldBe(["repo"]);
        editor.SearchChartsCommand.CanExecute(null).ShouldBeFalse();
        editor.ChartSearch = "web";
        await editor.SearchChartsCommand.ExecuteAsync(null);
        editor.Error.ShouldNotBeEmpty();
        editor.IsLoading.ShouldBeFalse();
        editor.AvailableCharts.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Release_editor_scrolls_long_values_and_form_in_small_window()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ValuesYaml = string.Join('\n', Enumerable.Range(0, 200).Select(index => $"value{index}: {index}"));
        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Width = 700;
            window.Height = 350;
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var controls = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view).ToArray();
            var textEditor = controls.OfType<TextEditor>().Single();
            var valuesScroll = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(textEditor)
                .OfType<ScrollViewer>().First();
            valuesScroll.Extent.Height.ShouldBeGreaterThan(valuesScroll.Viewport.Height);
            valuesScroll.Offset = new Vector(0, valuesScroll.Extent.Height);
            await TestApplicationExtensions.WaitForUiAsync();
            valuesScroll.Offset.Y.ShouldBeGreaterThan(0);
            var formScroll = controls.OfType<ScrollViewer>()
                .Single(scroll => scroll.Content is Grid);
            formScroll.Extent.Height.ShouldBeGreaterThan(formScroll.Viewport.Height);
            formScroll.ScrollToEnd();
            await TestApplicationExtensions.WaitForUiAsync();
            formScroll.Offset.Y.ShouldBeGreaterThan(0);
            var save = controls.OfType<Button>().Single(button => ReferenceEquals(button.Command, editor.SaveCommand));
            var position = save.TranslatePoint(default, formScroll).ShouldNotBeNull();
            position.Y.ShouldBeGreaterThanOrEqualTo(0);
            (position.Y + save.Bounds.Height).ShouldBeLessThanOrEqualTo(formScroll.Bounds.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Upgrade_loads_existing_values_as_yaml(bool hasChartMetadata)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        using var json = System.Text.Json.JsonDocument.Parse("""{"replicas":3,"server":{"name":"true"},"enabled":true}""");
        helm.Setup(service => service.GetValuesAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(json.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone()));
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new("bitnami/nginx", "nginx", "2.0.0"),
                new("bitnami/nginx", "nginx", "1.0.0"),
                new("bitnami/nginx", "nginx", "2.0.0"),
                new("other/unrelated", "unrelated", "9.0.0")]);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("bitnami", "https://example.test")]);
        var release = new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0.0", "", "nginx", "1.0.0");
        helm.Setup(service => service.StatusAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        helm.Setup(service => service.UpgradeAsync(
                It.IsAny<HelmConnection>(), "web", "bitnami/nginx", It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>(), "2.0.0"))
            .ReturnsAsync(new HelmRelease("web", "apps", 2, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-2.0.0", "2.0.0", "nginx", "2.0.0"));
        var factory = new Mock<IFactory>();
        var dialogs = HelmDialogTestHelper.CreateConfirmingDialogService();
        var preview = new Mock<IHelmReleasePreviewService>();
        var notifier = new HelmListNotifier();
        var releaseChanges = 0;
        notifier.ReleasesChanged += (_, _) => releaseChanges++;
        var editor = new HelmReleaseEditorViewModel(helm.Object, factory.Object, notifier, preview.Object, dialogs);
        editor.Initialize(workspace, new HelmConnection("config", "context", "wrong"),
            hasChartMetadata ? release : release with { ChartName = "", ChartVersion = "" });

        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ValuesYaml.ShouldContain("replicas: 3");
        editor.ValuesYaml.ShouldContain("enabled: true");
        editor.ValuesYaml.ShouldContain("server:");
        editor.Chart.ShouldBe("bitnami/nginx");
        editor.SelectedRepository.ShouldBe("bitnami");
        editor.SelectedChart.ShouldBe("nginx");
        editor.AvailableVersions.ShouldBe(["2.0.0", "1.0.0"]);
        editor.SelectedVersion.ShouldBe("1.0.0");
        editor.Error.ShouldBeEmpty();
        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var textEditor = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextEditor>().Single();
            textEditor.Text.ShouldBe(editor.ValuesYaml);
            textEditor.IsReadOnly.ShouldBeFalse();
            var versions = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().Single(control => control.Name == "ChartVersion");
            var dropdowns = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().ToDictionary(control => control.Name!);
            dropdowns["Repository"].SelectedItem.ShouldBe("bitnami");
            dropdowns["Chart"].SelectedItem.ShouldBe("nginx");
            dropdowns["Repository"].HorizontalAlignment.ShouldBe(global::Avalonia.Layout.HorizontalAlignment.Stretch);
            dropdowns["Chart"].HorizontalAlignment.ShouldBe(global::Avalonia.Layout.HorizontalAlignment.Stretch);
            versions.HorizontalAlignment.ShouldBe(global::Avalonia.Layout.HorizontalAlignment.Stretch);
            var repositoryPosition = dropdowns["Repository"].TranslatePoint(default, view).ShouldNotBeNull();
            var chartPosition = dropdowns["Chart"].TranslatePoint(default, view).ShouldNotBeNull();
            var versionPosition = versions.TranslatePoint(default, view).ShouldNotBeNull();
            chartPosition.X.ShouldBe(repositoryPosition.X);
            versionPosition.X.ShouldBe(repositoryPosition.X);
            dropdowns["Chart"].Bounds.Width.ShouldBe(dropdowns["Repository"].Bounds.Width);
            versions.Bounds.Width.ShouldBe(dropdowns["Repository"].Bounds.Width);
            versions.SelectedItem.ShouldBe("1.0.0");
            versions.SelectedItem = "2.0.0";
            editor.SelectedVersion.ShouldBe("2.0.0");
            var highlighting = Interaction.GetBehaviors(textEditor).OfType<YamlSyntaxHighlightingBehavior>().Single();
            highlighting.IsInstalled.ShouldBeTrue();
            var originalTheme = Application.Current!.RequestedThemeVariant;
            try
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                highlighting.IsInstalled.ShouldBeTrue();
            }
            finally
            {
                Application.Current.RequestedThemeVariant = originalTheme;
            }

            textEditor.Text = "replicas: 4\n";
            editor.ValuesYaml.ShouldBe("replicas: 4\n");
            editor.ValuesYaml = "replicas: 5\n";
            textEditor.Text.ShouldBe(editor.ValuesYaml);
            highlighting.Detach();
            highlighting.IsInstalled.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }

        await editor.PreviewCommand.ExecuteAsync(null);
        preview.Verify(service => service.PreviewVersionAsync(workspace,
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"), "web", "bitnami/nginx", "2.0.0",
            It.Is<IReadOnlyDictionary<string, object?>>(values => Convert.ToInt64(values["replicas"]) == 5),
            It.IsAny<CancellationToken>()), Times.Once);
        editor.SaveCommand.CanExecute(null).ShouldBeTrue();
        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        helm.Verify(service => service.UpgradeAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"), "web", "bitnami/nginx",
            It.Is<IReadOnlyDictionary<string, object?>>(values => Convert.ToInt64(values["replicas"]) == 5),
            false, false, true, TimeSpan.FromMinutes(5), It.IsAny<CancellationToken>(), "2.0.0"), Times.Once);
        releaseChanges.ShouldBe(1);
    }

    [AvaloniaTheory]
    [InlineData("replicas: [")]
    [InlineData("- first\n- second")]
    [InlineData("42")]
    [InlineData("")]
    [InlineData("replicas: 1\nreplicas: 2")]
    public async Task Release_form_rejects_non_mapping_or_invalid_yaml(string yaml)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ReleaseName = "web";
        editor.Chart = "bitnami/nginx";
        editor.ValuesYaml = yaml;

        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        editor.PreviewCommand.CanExecute(null).ShouldBeFalse();
        await editor.PreviewCommand.ExecuteAsync(null);
        editor.Error.ShouldNotBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Upgrade_load_handles_values_failures_and_ambiguous_chart_references(bool valuesSucceed)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        var pendingValues = new TaskCompletionSource<IReadOnlyDictionary<string, System.Text.Json.JsonElement>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .Returns(pendingValues.Task);
        helm.Setup(service => service.StatusAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0", "", "nginx", "1.0"));
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("first/nginx", "nginx", "1.0"), new("second/nginx", "nginx", "1.0")]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"),
            new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0", ""));
        editor.IsLoading.ShouldBeTrue();
        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        if (valuesSucceed)
        {
            pendingValues.SetResult(new Dictionary<string, System.Text.Json.JsonElement>());
        }
        else
        {
            pendingValues.SetException(new InvalidOperationException("Cannot load values"));
        }

        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.IsLoading.ShouldBeFalse();
        editor.Error.ShouldNotBeEmpty();
        editor.Chart.ShouldBeEmpty();
        editor.Chart = "manual/nginx";
        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        editor.PreviewCommand.CanExecute(null).ShouldBe(valuesSucceed);
        if (valuesSucceed)
        {
            editor.AvailableVersions.ShouldBe(["1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");
            editor.Chart = "first/nginx";
            editor.AvailableVersions.ShouldBe(["1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");
            editor.SelectedVersion = null;
            editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
        }
    }

    [AvaloniaFact]
    public async Task Repository_form_saves_repository_and_credentials()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = CreateHelmServiceMock();
        helmService
            .Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRepository>());
        helmService
            .Setup(service => service.AddRepositoryAsync(
                It.IsAny<HelmConnection>(),
                "private",
                "https://charts.example.com",
                "user",
                "secret",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var factory = new Mock<IFactory>();
        var notifier = new HelmListNotifier();
        var repositoryChanges = 0;
        notifier.RepositoriesChanged += (_, _) => repositoryChanges++;
        var editor = new HelmRepositoryEditorViewModel(helmService.Object, factory.Object, notifier);
        editor.Initialize(workspace, new HelmConnection("config", "context", null), null);
        editor.Name = "private";
        editor.Url = "https://charts.example.com";
        editor.Username = "user";
        editor.Password = "secret";

        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.AddRepositoryAsync(
            It.IsAny<HelmConnection>(),
            "private",
            "https://charts.example.com",
            "user",
            "secret",
            It.IsAny<CancellationToken>()), Times.Once);
        repositoryChanges.ShouldBe(1);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Once);
    }

    [AvaloniaFact]
    public async Task Repository_form_skips_unchanged_edit_so_stored_credentials_survive()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = CreateHelmServiceMock();
        var factory = new Mock<IFactory>();
        var notifier = new HelmListNotifier();
        var repositoryChanges = 0;
        notifier.RepositoriesChanged += (_, _) => repositoryChanges++;
        var editor = new HelmRepositoryEditorViewModel(helmService.Object, factory.Object, notifier);
        editor.Initialize(workspace, new HelmConnection("config", "context", null),
            new HelmRepository("private", "https://charts.example.com"));

        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.AddRepositoryAsync(
            It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryChanges.ShouldBe(1);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Once);
    }

    [AvaloniaFact]
    public async Task Release_form_previews_and_installs_chart_with_values()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = CreateHelmServiceMock();
        helmService.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("bitnami", "https://example.test")]);
        helmService.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("bitnami/nginx", "nginx", "1.0")]);
        var previewService = new Mock<IHelmReleasePreviewService>();
        helmService
            .Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRelease>());
        previewService
            .Setup(service => service.PreviewAsync(
                workspace,
                It.IsAny<HelmConnection>(),
                "web",
                "bitnami/nginx",
                It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        helmService
            .Setup(service => service.InstallAsync(
                It.IsAny<HelmConnection>(), "web", "bitnami/nginx", It.IsAny<IReadOnlyDictionary<string, object?>>(), true, true,
                TimeSpan.FromSeconds(30), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0.0", "1.0.0"));
        var factory = new Mock<IFactory>();
        var dialogs = HelmDialogTestHelper.CreateConfirmingDialogService();
        var notifier = new HelmListNotifier();
        var releaseChanges = 0;
        notifier.ReleasesChanged += (_, _) => releaseChanges++;
        var editor = new HelmReleaseEditorViewModel(helmService.Object, factory.Object, notifier,
            previewService.Object, dialogs);
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ChartSearch = "nginx";
        await editor.SearchChartsCommand.ExecuteAsync(null);
        editor.SelectedRepository = "bitnami";
        editor.SelectedChart = "nginx";
        editor.Chart.ShouldBe("bitnami/nginx");
        editor.Error.ShouldBeEmpty();
        editor.ReleaseName = "web";
        editor.Chart = "bitnami/nginx";
        editor.Namespace = "apps";
        editor.ValuesYaml = "replicas: 3";
        editor.CreateNamespace = true;
        editor.WaitForReady = true;
        editor.TimeoutSeconds = 30;

        await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        previewService.Verify(service => service.PreviewAsync(
            workspace,
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            "web",
            "bitnami/nginx",
            It.Is<IReadOnlyDictionary<string, object?>>(values => values.ContainsKey("replicas")),
            It.IsAny<CancellationToken>()), Times.Once);
        helmService.Verify(service => service.InstallAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            "web",
            "bitnami/nginx",
            It.Is<IReadOnlyDictionary<string, object?>>(values => values.ContainsKey("replicas")),
            true,
            true,
            TimeSpan.FromSeconds(30),
            It.IsAny<CancellationToken>()), Times.Once);
        releaseChanges.ShouldBe(1);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Once);
    }

    private static Mock<IHelmService> CreateHelmServiceMock()
    {
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListChartsAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmChartSearchResult>());
        return helm;
    }
}
