using System.Net;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Shell.Documents.Updates;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Styles;
using LiveMarkdown.Avalonia;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Velopack;

namespace KubeUI.Avalonia.Tests.Shell.Documents.Updates;

public sealed class UpdateAvailableViewModelTests
{
    [Fact]
    public void release_notes_include_each_available_version_once()
    {
        var markdown = UpdateAvailableViewModel.CreateReleaseNotesMarkdown(
        [
            ("2.1.0", "## 2.1.0\n\nAdded a dashboard."),
            ("2.2.0", "## 2.2.0\n\nFixed a bug."),
            ("2.2.0", "## 2.2.0\n\nDuplicate notes."),
            ("2.3.0", " ")
        ]);

        markdown.ShouldContain("## 2.1.0");
        markdown.ShouldContain("Added a dashboard.");
        markdown.ShouldContain("## 2.2.0");
        markdown.ShouldContain("Fixed a bug.");
        markdown.ShouldNotContain("Duplicate notes.");
        markdown.ShouldNotContain("## 2.3.0");
    }

    [Fact]
    public void release_notes_show_fallback_when_no_release_has_notes()
    {
        var markdown = UpdateAvailableViewModel.CreateReleaseNotesMarkdown(
        [
            ("2.1.0", null),
            ("2.2.0", " ")
        ]);

        markdown.ShouldBe(Assets.Resources.UpdateAvailableView_NoReleaseNotes);
    }

    [Fact]
    public void update_presentation_contains_versions_and_release_notes()
    {
        var presentation = MainViewModel.CreateUpdatePresentation(
            null,
            "2.3.0",
            [
                ("2.2.0", "## 2.2.0\n\nImproved startup."),
                ("2.3.0", "## 2.3.0\n\nAdded update view.")
            ]);

        presentation.CurrentVersion.ShouldBe(Assets.Resources.UpdateAvailableView_CurrentVersion_Unknown);
        presentation.LatestVersion.ShouldBe("2.3.0");
        presentation.ReleaseNotesMarkdown.ShouldContain("## 2.2.0");
        presentation.ReleaseNotesMarkdown.ShouldContain("## 2.3.0");
        presentation.ReleaseNotesMarkdown.ShouldContain("Added update view.");
    }

    [Fact]
    public async Task release_notes_fetch_every_published_release_between_versions()
    {
        List<string> requestedPages = [];
        using var handler = new StubHttpMessageHandler(request =>
        {
            requestedPages.Add(request.RequestUri!.Query);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    request.RequestUri.Query.EndsWith("page=1", StringComparison.Ordinal)
                        ? """
                          [
                            { "tag_name": "v1.6.2", "body": "Latest changes." },
                            { "tag_name": "v1.6.1", "body": "Intermediate changes." },
                            { "tag_name": "v1.6.0", "body": "Release without a Velopack package." },
                            { "tag_name": "v1.6.3", "body": "Newer than target." },
                            { "tag_name": "not-a-version", "body": "Malformed release tag." },
                            { "tag_name": "v1.6.1-rc.1", "body": "Prerelease.", "prerelease": true },
                            { "tag_name": "v1.6.1-draft", "body": "Draft.", "draft": true },
                            { "tag_name": "v1.6.1", "body": "Duplicate." },
                            { "tag_name": "v1.4.2", "body": "Older release." },
                            { "tag_name": "v1.4.1", "body": "Older release." },
                            { "tag_name": "v1.4.0", "body": "Older release." }
                          ]
                          """
                        : request.RequestUri.Query.EndsWith("page=2", StringComparison.Ordinal)
                            ? """[{ "tag_name": "v1.5.0", "body": "Current release." }]"""
                            : "[]",
                    Encoding.UTF8,
                    "application/json")
            };
        });
        using var client = new HttpClient(handler, disposeHandler: false);

        var releases = await MainViewModel.GetReleaseNotesAsync(
            client,
            SemanticVersion.Parse("1.5.0"),
            SemanticVersion.Parse("1.6.2"),
            includePrereleases: false,
            TestContext.Current.CancellationToken);

        releases.Select(release => release.Version)
            .ShouldBe(["1.6.0", "1.6.1", "1.6.2"]);
        releases[0].Markdown.ShouldBe("Release without a Velopack package.");
        requestedPages.ShouldBe(["?per_page=10&page=1", "?per_page=10&page=2"]);
    }

    [Fact]
    public async Task release_notes_include_prereleases_when_enabled()
    {
        using var handler = new StubHttpMessageHandler(request =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    request.RequestUri!.Query.EndsWith("page=1", StringComparison.Ordinal)
                        ? """
                          [
                            { "tag_name": "v1.6.1", "body": "Stable." },
                            { "tag_name": "v1.6.1-rc.1", "body": "Release candidate.", "prerelease": true }
                          ]
                          """
                        : "[]",
                    Encoding.UTF8,
                    "application/json")
            });
        using var client = new HttpClient(handler, disposeHandler: false);

        var releases = await MainViewModel.GetReleaseNotesAsync(
            client,
            SemanticVersion.Parse("1.5.0"),
            SemanticVersion.Parse("1.6.2"),
            includePrereleases: true,
            TestContext.Current.CancellationToken);

        releases.Select(release => release.Version)
            .ShouldBe(["1.6.1-rc.1", "1.6.1"]);
    }

    [Theory]
    [InlineData(FAContentDialogResult.Primary, true)]
    [InlineData(FAContentDialogResult.Secondary, false)]
    public async Task update_command_runs_update_only_after_confirmation(
        FAContentDialogResult result,
        bool shouldUpdate)
    {
        List<ContentDialogSettings> dialogs = [];
        Mock<IDialogManager> manager = new();
        manager.SetupGet(x => x.Logger).Returns((ILogger<IDialogManager>?)null);
        manager.SetupProperty(x => x.AllowConcurrentDialogs);
        manager
            .Setup(x => x.ShowFrameworkDialogAsync(
                It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .Callback<System.ComponentModel.INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>(
                (_, settings, _) => dialogs.Add(settings))
            .ReturnsAsync(result);

        var updateStarted = false;
        var viewModel = new UpdateAvailableViewModel(new DialogService(manager.Object), Mock.Of<IPlatformServices>(), Mock.Of<ILogger<UpdateAvailableViewModel>>());
        viewModel.SetUpdate(
            "2.1.0",
            "2.2.0",
            "## 2.2.0",
            _ =>
            {
                updateStarted = true;
                return Task.CompletedTask;
            });

        await viewModel.UpdateCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        dialogs.ShouldHaveSingleItem();
        dialogs[0].Title.ShouldBe(Assets.Resources.UpdateAvailableView_Confirm_Title);
        dialogs[0].Content.ShouldBe("Are you sure you want to update to 2.2.0?");
        dialogs[0].DefaultButton.ShouldBe(FAContentDialogButton.Secondary);
        updateStarted.ShouldBe(shouldUpdate);
        viewModel.IsUpdating.ShouldBeFalse();
    }

    [Fact]
    public async Task update_command_without_update_information_fails_explicitly()
    {
        var viewModel = new UpdateAvailableViewModel(Mock.Of<IDialogService>(), Mock.Of<IPlatformServices>(), Mock.Of<ILogger<UpdateAvailableViewModel>>());

        await Should.ThrowAsync<InvalidOperationException>(
            () => viewModel.UpdateCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task update_command_reports_update_failures_and_resets_busy_state()
    {
        List<ContentDialogSettings> dialogs = [];
        Mock<IDialogManager> manager = new();
        manager.SetupGet(x => x.Logger).Returns((ILogger<IDialogManager>?)null);
        manager.SetupProperty(x => x.AllowConcurrentDialogs);
        manager
            .Setup(x => x.ShowFrameworkDialogAsync(
                It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .Callback<System.ComponentModel.INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>(
                (_, settings, _) => dialogs.Add(settings))
            .ReturnsAsync(FAContentDialogResult.Primary);
        var viewModel = new UpdateAvailableViewModel(new DialogService(manager.Object), Mock.Of<IPlatformServices>(), Mock.Of<ILogger<UpdateAvailableViewModel>>());
        viewModel.SetUpdate(
            "2.1.0",
            "2.2.0",
            "## 2.2.0",
            _ => Task.FromException(new InvalidOperationException("Update failed.")));

        await viewModel.UpdateCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        dialogs.Count.ShouldBe(2);
        dialogs[1].Title.ShouldBe(Assets.Resources.UpdateAvailableView_Failure_Title);
        dialogs[1].Content.ShouldBe(Assets.Resources.UpdateAvailableView_Failure_Content);
        viewModel.IsUpdating.ShouldBeFalse();
        viewModel.CanClose.ShouldBeTrue();
    }

    [Fact]
    public void setting_update_updates_document_versions_and_markdown()
    {
        Mock<IDialogService> dialogService = new();
        UpdateAvailableViewModel viewModel = new(dialogService.Object, Mock.Of<IPlatformServices>(), Mock.Of<ILogger<UpdateAvailableViewModel>>());

        viewModel.SetUpdate(
            "2.1.0",
            "2.2.0",
            "## 2.2.0\n\nNew feature.",
            _ => Task.CompletedTask);

        viewModel.Title.ShouldBe(Assets.Resources.UpdateAvailableView_Title);
        viewModel.VersionSummary.ShouldBe("Update Available 2.1.0 -> 2.2.0");
        viewModel.LatestVersion.ShouldBe("2.2.0");
        viewModel.MarkdownBuilder.ToString().ShouldContain("New feature.");
    }

    [AvaloniaFact]
    public async Task view_displays_update_versions_markdown_and_action()
    {
        var viewModel = new UpdateAvailableViewModel(Mock.Of<IDialogService>(), Mock.Of<IPlatformServices>(), Mock.Of<ILogger<UpdateAvailableViewModel>>());
        viewModel.SetUpdate(
            "2.1.0",
            "2.2.0",
            "## 2.2.0\n\nNew feature.",
            _ => Task.CompletedTask);
        var view = new UpdateAvailableView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var descendants = view.GetVisualDescendants().ToArray();
        var header = descendants.OfType<TextBlock>()
            .Single(x => x.Text == "Update Available 2.1.0 -> 2.2.0");
        header.FontSize.ShouldBe(GetResource(Application.Current!, Typography.TitleFontSizeResourceKey));
        descendants.OfType<MarkdownRenderer>().ShouldHaveSingleItem();
        descendants.OfType<Button>()
            .Any(button => button.Content?.ToString() == Assets.Resources.UpdateAvailableView_Update)
            .ShouldBeTrue();
        descendants.OfType<ProgressBar>().ShouldHaveSingleItem();
    }

    [AvaloniaFact]
    public async Task update_command_reports_download_progress_to_view()
    {
        Mock<IDialogManager> manager = new();
        manager.SetupGet(x => x.Logger).Returns((ILogger<IDialogManager>?)null);
        manager.SetupProperty(x => x.AllowConcurrentDialogs);
        manager
            .Setup(x => x.ShowFrameworkDialogAsync(
                It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .ReturnsAsync(FAContentDialogResult.Primary);
        UpdateAvailableViewModel viewModel = new(new DialogService(manager.Object), Mock.Of<IPlatformServices>(), Mock.Of<ILogger<UpdateAvailableViewModel>>());
        TaskCompletionSource updateStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource finishUpdate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        viewModel.SetUpdate(
            "2.1.0",
            "2.2.0",
            "## 2.2.0",
            async reportProgress =>
            {
                reportProgress(42);
                updateStarted.TrySetResult();
                await finishUpdate.Task.WaitAsync(TestContext.Current.CancellationToken);
            });
        var view = new UpdateAvailableView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var updateTask = viewModel.UpdateCommand.ExecuteAsync(null);
        await updateStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        viewModel.CanClose.ShouldBeFalse();
        viewModel.UpdateCommand.CanExecute(null).ShouldBeFalse();
        var progressBar = view.GetVisualDescendants().OfType<ProgressBar>().ShouldHaveSingleItem();
        progressBar.IsVisible.ShouldBeTrue();
        progressBar.Value.ShouldBe(42d);
        viewModel.DownloadProgressText.ShouldBe("Downloading update: 42%");

        finishUpdate.TrySetResult();
        await updateTask.WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        progressBar.IsVisible.ShouldBeFalse();
        viewModel.CanClose.ShouldBeTrue();
    }

    private static double GetResource(Application application, string key)
    {
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        return Convert.ToDouble(value);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(responseFactory(request));
        }
    }
}
