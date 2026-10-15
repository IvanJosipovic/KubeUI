using System.Text;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using LiveMarkdown.Avalonia;

namespace KubeUI.Avalonia.Shell.Documents.Updates;

public sealed partial class UpdateAvailableViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IPlatformServices _platformServices;
    private Func<Action<int>, Task>? _updateAsync;

    public ObservableStringBuilder MarkdownBuilder { get; } = new();

    [ObservableProperty]
    public partial string VersionSummary { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string LatestVersion { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUpdating { get; private set; }

    [ObservableProperty]
    public partial int DownloadProgress { get; private set; }

    [ObservableProperty]
    public partial string DownloadProgressText { get; private set; } = string.Empty;

    public UpdateAvailableViewModel(IDialogService dialogService, IPlatformServices platformServices)
    {
        _dialogService = dialogService;
        _platformServices = platformServices;
        Id = nameof(UpdateAvailableViewModel);
        Title = Assets.Resources.UpdateAvailableView_Title;
        CanClose = true;
    }

    internal void SetUpdate(
        string currentVersion,
        string latestVersion,
        string releaseNotesMarkdown,
        Func<Action<int>, Task> updateAsync)
    {
        LatestVersion = latestVersion;
        VersionSummary = string.Format(
            Assets.Resources.UpdateAvailableView_VersionSummaryFormat,
            currentVersion,
            latestVersion);
        Title = VersionSummary;
        MarkdownBuilder.Clear();
        MarkdownBuilder.Append(releaseNotesMarkdown);
        _updateAsync = updateAsync;
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (IsUpdating)
        {
            return;
        }

        var updateAsync = _updateAsync
            ?? throw new InvalidOperationException("Update information has not been configured.");
        var result = await _dialogService.ShowContentDialogAsync(
            this,
            CreateUpdateConfirmationDialogSettings(LatestVersion));

        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        IsUpdating = true;
        CanClose = false;
        DownloadProgress = 0;
        DownloadProgressText = string.Format(Assets.Resources.UpdateAvailableView_DownloadProgress, 0);

        try
        {
            await updateAsync(ReportDownloadProgress);
        }
        finally
        {
            IsUpdating = false;
            CanClose = true;
        }
    }

    [RelayCommand]
    private async Task OpenReleaseNoteLinkAsync(LinkClickedEventArgs args)
    {
        var uri = args.HRef;
        if (uri.IsAbsoluteUri
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps
                || uri.Scheme == Uri.UriSchemeMailto))
        {
            await _platformServices.LaunchUriAsync(uri);
        }
    }

    private void ReportDownloadProgress(int progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DownloadProgress = Math.Clamp(progress, 0, 100);
            DownloadProgressText = string.Format(
                Assets.Resources.UpdateAvailableView_DownloadProgress,
                DownloadProgress);
        });
    }

    internal static string CreateReleaseNotesMarkdown(IEnumerable<(string Version, string? Markdown)> releases)
    {
        StringBuilder markdown = new();
        HashSet<string> seenVersions = new(StringComparer.OrdinalIgnoreCase);

        foreach (var (version, notes) in releases)
        {
            if (string.IsNullOrWhiteSpace(notes) || !seenVersions.Add(version))
            {
                continue;
            }

            if (markdown.Length > 0)
            {
                markdown.AppendLine();
            }

            markdown.AppendLine(notes.Trim());
        }

        return markdown.Length > 0
            ? markdown.ToString()
            : Assets.Resources.UpdateAvailableView_NoReleaseNotes;
    }

    internal static ContentDialogSettings CreateUpdateConfirmationDialogSettings(string latestVersion)
    {
        return new ContentDialogSettings
        {
            Title = Assets.Resources.UpdateAvailableView_Confirm_Title,
            Content = string.Format(Assets.Resources.UpdateAvailableView_Confirm_Content, latestVersion),
            PrimaryButtonText = Assets.Resources.UpdateAvailableView_Confirm_Primary,
            SecondaryButtonText = Assets.Resources.UpdateAvailableView_Confirm_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };
    }
}
