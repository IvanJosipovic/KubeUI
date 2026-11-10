using System.Text;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.App.Infrastructure.Presentation;
using LiveMarkdown.Avalonia;
using Microsoft.Extensions.Logging;

namespace KubeUI.App.Shell.Documents.Updates;

public sealed partial class UpdateAvailableViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IPlatformServices _platformServices;
    private readonly ILogger<UpdateAvailableViewModel> _logger;
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

    public UpdateAvailableViewModel(
        IDialogService dialogService,
        IPlatformServices platformServices,
        ILogger<UpdateAvailableViewModel> logger)
    {
        _dialogService = dialogService;
        _platformServices = platformServices;
        _logger = logger;
        Id = nameof(UpdateAvailableViewModel);
        Title = AppLanguageManager.Instance.UpdateAvailableView_Title.CurrentValue()!;
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
            AppLanguageManager.Instance.UpdateAvailableView_VersionSummaryFormat.CurrentValue()!,
            currentVersion,
            latestVersion);
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
        DownloadProgressText = string.Format(AppLanguageManager.Instance.UpdateAvailableView_DownloadProgress.CurrentValue()!, 0);

        try
        {
            await updateAsync(ReportDownloadProgress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to install update {Version}", LatestVersion);
            await _dialogService.ShowContentDialogAsync(this, CreateUpdateFailureDialogSettings());
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
                AppLanguageManager.Instance.UpdateAvailableView_DownloadProgress.CurrentValue()!,
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
            : AppLanguageManager.Instance.UpdateAvailableView_NoReleaseNotes.CurrentValue()!;
    }

    internal static ContentDialogSettings CreateUpdateConfirmationDialogSettings(string latestVersion)
    {
        return new ContentDialogSettings
        {
            Title = AppLanguageManager.Instance.UpdateAvailableView_Confirm_Title.CurrentValue()!,
            Content = string.Format(AppLanguageManager.Instance.UpdateAvailableView_Confirm_Content.CurrentValue()!, latestVersion),
            PrimaryButtonText = AppLanguageManager.Instance.UpdateAvailableView_Confirm_Primary.CurrentValue()!,
            SecondaryButtonText = AppLanguageManager.Instance.UpdateAvailableView_Confirm_Secondary.CurrentValue()!,
            DefaultButton = FAContentDialogButton.Secondary
        };
    }

    internal static ContentDialogSettings CreateUpdateFailureDialogSettings()
    {
        return new ContentDialogSettings
        {
            Title = AppLanguageManager.Instance.UpdateAvailableView_Failure_Title.CurrentValue()!,
            Content = AppLanguageManager.Instance.UpdateAvailableView_Failure_Content.CurrentValue()!,
            PrimaryButtonText = AppLanguageManager.Instance.UpdateAvailableView_Failure_Primary.CurrentValue()!,
            DefaultButton = FAContentDialogButton.Primary
        };
    }
}
