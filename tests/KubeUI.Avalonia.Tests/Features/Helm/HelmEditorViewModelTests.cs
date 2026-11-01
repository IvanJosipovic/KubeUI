using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using Helm.NET;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmEditorViewModelTests : IDisposable
{
    private readonly KubernetesYamlSerializer _yamlSerializer = new(allowReflection: true);

    public void Dispose() => _yamlSerializer.Dispose();

    [AvaloniaFact]
    public async Task Upgrade_editors_for_same_release_name_in_different_namespaces_have_distinct_ids()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        using var valuesDocument = System.Text.Json.JsonDocument.Parse("{}");
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(valuesDocument.RootElement.Clone());

        HelmReleaseEditorViewModel CreateEditor() => new(
            helm.Object,
            _yamlSerializer,
            Mock.Of<IFactory>(),
            Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(),
            Mock.Of<IDialogService>());
        static HelmRelease CreateRelease(string releaseNamespace) => new(
            "web",
            releaseNamespace,
            1,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "deployed",
            "nginx-1.0.0",
            "1.0.0",
            "nginx",
            "1.0.0");

        var appsEditor = CreateEditor();
        var stagingEditor = CreateEditor();
        appsEditor.Initialize(workspace, new HelmConnection("config", "context", "apps"), CreateRelease("apps"));
        stagingEditor.Initialize(workspace, new HelmConnection("config", "context", "staging"), CreateRelease("staging"));
        await Task.WhenAll(appsEditor.InitializationTask, stagingEditor.InitializationTask)
            .WaitAsync(TestContext.Current.CancellationToken);

        appsEditor.Id.ShouldNotBe(stagingEditor.Id);
        appsEditor.Id.ShouldContain("-apps-web");
        stagingEditor.Id.ShouldContain("-staging-web");
    }

    [AvaloniaFact]
    public async Task Release_values_editor_supports_nested_yaml_folding()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var editor = new HelmReleaseEditorViewModel(
            helm.Object, _yamlSerializer,
            Mock.Of<IFactory>(),
            Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(),
            Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ValuesYaml = "replicas: 2\nserver:\n  ingress:\n    enabled: true\n";

        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var textEditor = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextEditor>().Single();
            var foldingMargin = textEditor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();

            foldingMargin.FoldingManager.ShouldNotBeNull();
            foldingMargin.FoldingManager!.AllFoldings
                .Select(folding => folding.Title.Trim())
                .ShouldContain("server:");
            foldingMargin.FoldingManager.AllFoldings
                .Select(folding => folding.Title.Trim())
                .ShouldContain("ingress:");
            var ingressFolding = foldingMargin.FoldingManager.AllFoldings
                .Single(folding => folding.Title.Trim() == "ingress:");
            ingressFolding.IsFolded = true;
            ingressFolding.IsFolded.ShouldBeTrue();
            ingressFolding.IsFolded = false;
            ingressFolding.IsFolded.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

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
            .ReturnsAsync(json.RootElement.Clone());
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
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, factory.Object, Mock.Of<IHelmListNotifier>(),
            preview.Object, Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "wrong"),
            hasChartMetadata ? metadata : metadata with { ChartName = "", ChartVersion = "" });
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);

        editor.IsLoading.ShouldBeFalse();
        GetFieldError(editor, nameof(HelmReleaseEditorViewModel.SelectedChart)).ShouldBe(scenario == "index-unavailable"
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
            var chartSelector = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().Single(control => control.Name == "Chart");
            DataValidationErrors.GetHasErrors(chartSelector).ShouldBeTrue();
            DataValidationErrors.GetErrors(chartSelector).Cast<string>()
                .ShouldContain(GetFieldError(editor, nameof(HelmReleaseEditorViewModel.SelectedChart)));
            textEditor.Text = "replicas: 4\nserver:\n  enabled: true\n";
            editor.ValuesYaml.ShouldBe(textEditor.Text);
            editor.IsOciSource = true;
            foreach (var reference in new[] { "manual/nginx", "oci://registry.test/charts/nginx" })
            {
                editor.Chart = reference;
                editor.AvailableVersions.ShouldBeEmpty();
                editor.SelectedVersion.ShouldBeNull();
                editor.SelectedRepository.ShouldBeNull();
                editor.SelectedChart.ShouldBeNull();
                editor.SelectedVersion = "1.0";
                editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
                preview.SetupSequence(service => service.PreviewVersionAsync(workspace,
                        It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
                        "web", reference, "1.0",
                        It.Is<IReadOnlyDictionary<string, object?>>(values => Convert.ToInt64(values["replicas"]) == 4),
                        It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("Chart reference cannot be resolved"))
                    .Returns(Task.CompletedTask);
                await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
                GetFieldError(editor, nameof(HelmReleaseEditorViewModel.Chart)).ShouldBe("Chart reference cannot be resolved");
                var chartReference = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                    .OfType<TextBox>().Single(control => control.Name == "ChartReference");
                DataValidationErrors.GetHasErrors(chartReference).ShouldBeTrue();
                editor.Chart.ShouldBe(reference);
                editor.SelectedVersion.ShouldBe("1.0");
                editor.ValuesYaml.ShouldBe("replicas: 4\nserver:\n  enabled: true\n");
                editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
                await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
                editor.HasErrors.ShouldBeFalse();
            }
        }
        finally
        {
            window.Close();
        }

        preview.VerifyAll();
        editor.SaveCommand.CanExecute(null).ShouldBeTrue();
        helm.Verify(service => service.UpgradeAsync(
            It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>(), It.IsAny<string?>()), Times.Never);
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
            .ReturnsAsync(System.Text.Json.JsonSerializer.SerializeToElement(new Dictionary<string, object?>()));
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new("first/web", "web", "1.0"), new("second/web", "web", "2.0"),
                new("second/other", "other", "3.0"), new("second/other", "other", "4.0")]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
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
            var controls = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view).ToArray();
            var repositorySource = controls.OfType<RadioButton>()
                .Single(control => Equals(control.Content, Assets.Resources.HelmReleaseEditorView_RepositorySource));
            var ociSource = controls.OfType<RadioButton>()
                .Single(control => Equals(control.Content, Assets.Resources.HelmReleaseEditorView_OciSource));
            repositorySource.IsChecked.ShouldBe(true);
            var dropdowns = controls.OfType<ComboBox>().ToDictionary(control => control.Name!);
            var chartReference = controls.OfType<TextBox>().Single(control => control.Name == "ChartReference");
            var chartVersionText = controls.OfType<TextBox>().Single(control => control.Name == "ChartVersionText");
            dropdowns["Repository"].IsVisible.ShouldBeTrue();
            dropdowns["Chart"].IsVisible.ShouldBeTrue();
            dropdowns["ChartVersion"].IsVisible.ShouldBeTrue();
            chartReference.IsVisible.ShouldBeFalse();
            chartVersionText.IsVisible.ShouldBeFalse();
            dropdowns["Chart"].IsEnabled.ShouldBeFalse();
            dropdowns["ChartVersion"].IsEnabled.ShouldBeFalse();
            ociSource.IsChecked = true;
            editor.IsOciSource.ShouldBeTrue();
            dropdowns["Repository"].IsVisible.ShouldBeFalse();
            dropdowns["Chart"].IsVisible.ShouldBeFalse();
            dropdowns["ChartVersion"].IsVisible.ShouldBeFalse();
            chartReference.IsVisible.ShouldBeTrue();
            chartVersionText.IsVisible.ShouldBeTrue();
            chartReference.IsEnabled.ShouldBeTrue();
            chartVersionText.IsEnabled.ShouldBeFalse();
            chartReference.Text = "oci://registry.test/web";
            chartVersionText.IsEnabled.ShouldBeTrue();
            chartVersionText.Text = "9.1.0";
            editor.SelectedVersion.ShouldBe("9.1.0");
            repositorySource.IsChecked = true;
            editor.IsOciSource.ShouldBeFalse();
            dropdowns["ChartVersion"].IsVisible.ShouldBeTrue();
            chartVersionText.IsVisible.ShouldBeFalse();
            dropdowns["Repository"].SelectedItem = "second";
            editor.AvailableCharts.ShouldBe(["web", "other"]);
            dropdowns["Chart"].IsEnabled.ShouldBeTrue();
            dropdowns["ChartVersion"].IsEnabled.ShouldBeFalse();
            dropdowns["ChartVersion"].PlaceholderText.ShouldBeNullOrEmpty();
            dropdowns["Chart"].SelectedItem = "web";
            dropdowns["ChartVersion"].IsEnabled.ShouldBeTrue();
            editor.Chart.ShouldBe("second/web");
            editor.AvailableVersions.ShouldBe(["2.0", "1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");
            dropdowns["ChartVersion"].SelectedItem = "2.0";
            editor.SelectedVersion.ShouldBe("2.0");
            dropdowns["Repository"].SelectedItem = "first";
            editor.Chart.ShouldBe("first/web");
            editor.AvailableVersions.ShouldBe(["1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");

            editor.SelectedRepository = "second";
            editor.AvailableCharts.ShouldBe(["web", "other"]);
            editor.SelectedChart.ShouldBe("web");
            editor.Chart.ShouldBe("second/web");
            editor.SelectedChart = "other";
            editor.Chart.ShouldBe("second/other");
            editor.AvailableVersions.ShouldBe(["3.0", "4.0"]);
            editor.SelectedVersion.ShouldBeNull();
            editor.SelectedVersion = "4.0";
            editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
            editor.IsOciSource = true;
            editor.Chart = "oci://registry.test/web";
            editor.SelectedChart.ShouldBeNull();
            editor.AvailableVersions.ShouldBeEmpty();
            editor.SelectedVersion.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Selected_chart_and_version_load_default_values_and_oci_input_is_debounced()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("stable", "https://stable.test")]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("stable/web", "web", "1.0"), new("stable/web", "web", "2.0")]);
        helm.Setup(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HelmConnection connection, string chart, string? version, CancellationToken cancellationToken) =>
                $"chart: {chart}\nversion: {version ?? "default"}\n"
                + string.Join('\n', Enumerable.Range(0, version == "2.0" ? 180 : 100).Select(index => $"value{index}: {index}")) + "\n");
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ValuesYaml.ShouldBeEmpty();
        editor.ReleaseName = "web";
        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Width = 700;
            window.Height = 450;
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var outerScrollViewer = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ScrollViewer>().First();
            var textEditor = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextEditor>().Single();
            var valuesScrollViewer = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(textEditor)
                .OfType<ScrollViewer>().First();
            outerScrollViewer.Offset = new Vector(0, 100);
            outerScrollViewer.Offset.Y.ShouldBe(100);
            var dropdowns = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().ToDictionary(control => control.Name!);

            dropdowns["Repository"].SelectedItem = "stable";
            dropdowns["Chart"].SelectedItem = "web";
            editor.SelectedVersion.ShouldBeNull();
            editor.ValuesYaml.ShouldBeEmpty();
            editor.SaveCommand.CanExecute(null).ShouldBeFalse();
            editor.PreviewCommand.CanExecute(null).ShouldBeFalse();
            helm.Verify(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), "stable/web", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);

            dropdowns["ChartVersion"].Focus();
            dropdowns["ChartVersion"].SelectedItem = "1.0";
            await TestWait.UntilAsync(() => editor.ValuesYaml.Contains("version: 1.0", StringComparison.Ordinal),
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync();

            editor.ValuesYaml.ShouldStartWith("chart: stable/web\nversion: 1.0\n");
            outerScrollViewer.Offset.Y.ShouldBe(100);
            valuesScrollViewer.Offset.Y.ShouldBe(0);
            textEditor.TextArea.Caret.Offset.ShouldBe(0);

            var priorEditorOffset = new Vector(0, Math.Min(80, valuesScrollViewer.ScrollBarMaximum.Y));
            valuesScrollViewer.Offset = priorEditorOffset;
            editor.SelectedVersion = "2.0";
            await TestWait.UntilAsync(() => editor.ValuesYaml.Contains("version: 2.0", StringComparison.Ordinal),
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync();
            valuesScrollViewer.Offset.ShouldBe(priorEditorOffset);
            outerScrollViewer.Offset.Y.ShouldBe(100);

            editor.IsOciSource = true;
            editor.Chart = "oci://registry.test/charts/old";
            editor.Chart = "oci://registry.test/charts/web";
            editor.SelectedVersion = "3.0";
            await TestWait.UntilAsync(() => editor.ValuesYaml.Contains("version: 3.0", StringComparison.Ordinal),
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

            editor.ValuesYaml.ShouldStartWith("chart: oci://registry.test/charts/web\nversion: 3.0\n");
            helm.Verify(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), "oci://registry.test/charts/web", "3.0", It.IsAny<CancellationToken>()), Times.Once);
            helm.Verify(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), "oci://registry.test/charts/old", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Install_catalog_load_surfaces_repository_errors(bool fails)
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

        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.Repositories.ShouldBe(["repo"]);
        GetFieldError(editor, nameof(HelmReleaseEditorViewModel.SelectedChart))
            .ShouldBe(fails ? "Repository index unavailable" : string.Empty);
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
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
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
            var namespaceLabel = controls.OfType<TextBlock>()
                .Single(control => control.Text == Assets.Resources.HelmReleaseEditorView_Namespace);
            var createNamespace = controls.OfType<CheckBox>()
                .Single(control => AutomationProperties.GetName(control) == Assets.Resources.HelmReleaseEditorView_CreateNamespace);
            var createNamespaceLabel = controls.OfType<TextBlock>()
                .Single(control => control.Text == Assets.Resources.HelmReleaseEditorView_CreateNamespace);
            var timeoutLabel = controls.OfType<TextBlock>()
                .Single(control => control.Text == Assets.Resources.HelmReleaseEditorView_Timeout);
            var namespaceInput = controls.OfType<TextBox>().Where(control => control.IsVisible).ElementAt(1);
            namespaceLabel.TranslatePoint(default, view)!.Value.Y.ShouldBeLessThan(createNamespace.TranslatePoint(default, view)!.Value.Y);
            createNamespaceLabel.TranslatePoint(default, view)!.Value.X.ShouldBe(namespaceLabel.TranslatePoint(default, view)!.Value.X);
            createNamespace.TranslatePoint(default, view)!.Value.X.ShouldBe(namespaceInput.TranslatePoint(default, view)!.Value.X);
            createNamespaceLabel.TranslatePoint(default, view)!.Value.Y.ShouldBe(createNamespace.TranslatePoint(default, view)!.Value.Y);
            createNamespace.TranslatePoint(default, view)!.Value.Y.ShouldBeLessThan(timeoutLabel.TranslatePoint(default, view)!.Value.Y);
            timeoutLabel.TranslatePoint(default, view)!.Value.Y.ShouldBeLessThan(textEditor.TranslatePoint(default, view)!.Value.Y);
            textEditor.FontWeight.ShouldBe(FontWeight.Normal);
            textEditor.ShowLineNumbers.ShouldBeTrue();
            textEditor.WordWrap.ShouldBeFalse();
            textEditor.Options.ConvertTabsToSpaces.ShouldBeTrue();
            textEditor.Options.EnableEmailHyperlinks.ShouldBeFalse();
            textEditor.Options.EnableHyperlinks.ShouldBeFalse();
            textEditor.Options.HighlightCurrentLine.ShouldBeTrue();
            textEditor.Options.IndentationSize.ShouldBe(2);
            textEditor.Options.ShowBoxForControlCharacters.ShouldBeFalse();
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
            .ReturnsAsync(json.RootElement.Clone());
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
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, factory.Object, notifier, preview.Object, dialogs);
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
        editor.HasErrors.ShouldBeFalse();
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
            var namespaceField = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextBox>().Where(control => control.IsVisible).ElementAt(1);
            var namespacePosition = namespaceField.TranslatePoint(default, view).ShouldNotBeNull();
            var chartVersionGap = versionPosition.Y - chartPosition.Y - dropdowns["Chart"].Bounds.Height;
            var versionNamespaceGap = namespacePosition.Y - versionPosition.Y - versions.Bounds.Height;
            chartPosition.X.ShouldBe(repositoryPosition.X);
            versionPosition.X.ShouldBe(repositoryPosition.X);
            Math.Abs(chartVersionGap - versionNamespaceGap).ShouldBeLessThanOrEqualTo(2);
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
    [InlineData("replicas: 1\nreplicas: 2")]
    public async Task Release_form_rejects_non_mapping_or_invalid_yaml(string yaml)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ValuesYaml.ShouldBeEmpty();
        editor.ReleaseName = "web";
        editor.Chart = "bitnami/nginx";
        editor.ValuesYaml = yaml;

        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        editor.PreviewCommand.CanExecute(null).ShouldBeFalse();
        await editor.PreviewCommand.ExecuteAsync(null);
        GetFieldError(editor, nameof(HelmReleaseEditorViewModel.ValuesYaml)).ShouldNotBeEmpty();

        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var valuesEditor = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextEditor>().Single();
            DataValidationErrors.GetHasErrors(valuesEditor).ShouldBeTrue();
            DataValidationErrors.GetErrors(valuesEditor).Cast<string>()
                .ShouldContain(GetFieldError(editor, nameof(HelmReleaseEditorViewModel.ValuesYaml)));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Upgrade_load_handles_values_failures_and_ambiguous_chart_references(bool valuesSucceed)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        var pendingValues = new TaskCompletionSource<System.Text.Json.JsonElement>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .Returns(pendingValues.Task);
        helm.Setup(service => service.StatusAsync(It.IsAny<HelmConnection>(), "web", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0", "", "nginx", "1.0"));
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("first/nginx", "nginx", "1.0"), new("second/nginx", "nginx", "1.0")]);
        var editor = new HelmReleaseEditorViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(), Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"),
            new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0", ""));
        editor.IsLoading.ShouldBeTrue();
        editor.SaveCommand.CanExecute(null).ShouldBeFalse();
        if (valuesSucceed)
        {
            pendingValues.SetResult(System.Text.Json.JsonSerializer.SerializeToElement(new Dictionary<string, object?>()));
        }
        else
        {
            pendingValues.SetException(new InvalidOperationException("Cannot load values"));
        }

        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.IsLoading.ShouldBeFalse();
        GetFieldError(editor, valuesSucceed
            ? nameof(HelmReleaseEditorViewModel.SelectedChart)
            : nameof(HelmReleaseEditorViewModel.ReleaseName)).ShouldNotBeEmpty();
        editor.Chart.ShouldBeEmpty();
        editor.IsOciSource = true;
        editor.Chart = "manual/nginx";
        editor.SaveCommand.CanExecute(null).ShouldBe(valuesSucceed);
        editor.PreviewCommand.CanExecute(null).ShouldBe(valuesSucceed);
        if (valuesSucceed)
        {
            editor.AvailableVersions.ShouldBeEmpty();
            editor.SelectedVersion.ShouldBeNull();
            editor.SelectedVersion = "1.0";
            editor.Chart = "first/nginx";
            editor.AvailableVersions.ShouldBe(["1.0"]);
            editor.SelectedVersion.ShouldBe("1.0");
            editor.SelectedVersion = null;
            editor.PreviewCommand.CanExecute(null).ShouldBeTrue();
        }
    }

    [AvaloniaFact]
    public async Task Clearing_timeout_does_not_create_a_binding_error()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = CreateHelmServiceMock();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var editor = new HelmReleaseEditorViewModel(
            helm.Object, _yamlSerializer,
            Mock.Of<IFactory>(),
            Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(),
            Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);

        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var timeoutInput = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<NumericUpDown>().Single();

            timeoutInput.Value = null;
            await TestApplicationExtensions.WaitForUiAsync();

            timeoutInput.Value.ShouldBeNull();
            editor.TimeoutSeconds.ShouldBeNull();
            DataValidationErrors.GetHasErrors(timeoutInput).ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Selecting_repository_chart_shows_required_error_on_chart_version()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRepository("stable", "https://stable.test")]);
        helm.Setup(service => service.ListChartsAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmChartSearchResult("stable/web", "web", "1.0")]);
        var editor = new HelmReleaseEditorViewModel(
            helm.Object, _yamlSerializer,
            Mock.Of<IFactory>(),
            Mock.Of<IHelmListNotifier>(),
            Mock.Of<IHelmReleasePreviewService>(),
            Mock.Of<IDialogService>());
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);

        editor.SelectedRepository = "stable";
        GetFieldError(editor, nameof(HelmReleaseEditorViewModel.SelectedChart))
            .ShouldBe(Assets.Resources.HelmReleaseEditorView_ChartRequired);
        GetFieldError(editor, nameof(HelmReleaseEditorViewModel.SelectedVersion)).ShouldBeEmpty();
        editor.SelectedChart = "web";

        GetFieldError(editor, nameof(HelmReleaseEditorViewModel.SelectedVersion))
            .ShouldBe(Assets.Resources.HelmReleaseEditorView_VersionRequired);
        var view = new HelmReleaseEditorView { DataContext = editor };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var versionSelector = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().Single(control => control.Name == "ChartVersion");
            DataValidationErrors.GetHasErrors(versionSelector).ShouldBeTrue();
            DataValidationErrors.GetErrors(versionSelector).Cast<string>()
                .ShouldContain(Assets.Resources.HelmReleaseEditorView_VersionRequired);
        }
        finally
        {
            window.Close();
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
            .Setup(service => service.PreviewVersionAsync(
                workspace,
                It.IsAny<HelmConnection>(),
                "web",
                "bitnami/nginx",
                "1.0",
                It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        helmService
            .Setup(service => service.InstallAsync(
                It.IsAny<HelmConnection>(), "web", "bitnami/nginx", It.IsAny<IReadOnlyDictionary<string, object?>>(), true, true,
                TimeSpan.FromSeconds(30), It.IsAny<CancellationToken>(), "1.0"))
            .ReturnsAsync(new HelmRelease("web", "apps", 1, new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero), "deployed", "nginx-1.0.0", "1.0.0"));
        var factory = new Mock<IFactory>();
        var dialogs = HelmDialogTestHelper.CreateConfirmingDialogService();
        var notifier = new HelmListNotifier();
        var releaseChanges = 0;
        notifier.ReleasesChanged += (_, _) => releaseChanges++;
        var editor = new HelmReleaseEditorViewModel(helmService.Object, _yamlSerializer, factory.Object, notifier,
            previewService.Object, dialogs);
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.SelectedRepository = "bitnami";
        editor.SelectedChart = "nginx";
        editor.SelectedVersion = "1.0";
        editor.Chart.ShouldBe("bitnami/nginx");
        editor.HasErrors.ShouldBeFalse();
        editor.ReleaseName = "web";
        editor.Chart = "bitnami/nginx";
        editor.Namespace = "apps";
        editor.ValuesYaml = "replicas: 3";
        editor.CreateNamespace = true;
        editor.TimeoutSeconds = 30;

        await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        previewService.Verify(service => service.PreviewVersionAsync(
            workspace,
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            "web",
            "bitnami/nginx",
            "1.0",
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
            It.IsAny<CancellationToken>(),
            "1.0"), Times.Once);
        releaseChanges.ShouldBe(1);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Once);
    }

    private static Mock<IHelmService> CreateHelmServiceMock()
    {
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}\n");
        helm.Setup(service => service.ListChartsAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmChartSearchResult>());
        return helm;
    }

    private static string GetFieldError(HelmReleaseEditorViewModel editor, string propertyName)
        => editor.GetErrors(propertyName).Cast<string>().SingleOrDefault() ?? string.Empty;
}
