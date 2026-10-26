using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Catalog;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Documents.About;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks;
using KubeUI.Avalonia.Shell.Documents.Settings;
using KubeUI.Avalonia.Shell.Documents.Updates;
using KubeUI.Kubernetes;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace KubeUI.Avalonia.Shell.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    internal const string GitHubReleasesHttpClientName = "GitHubReleases";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IFactory _factory;
    private readonly IClusterRuntimeCatalog _runtimeCatalog;
    private readonly IPlatformServices _platformServices;
    private readonly IHttpClientFactory _httpClientFactory;

    [ObservableProperty]
    private partial ISettingsService SettingsService { get; set; }

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    private readonly IDialogService _dialogService;
    private int _initialized;

    public MainViewModel(
        IServiceProvider serviceProvider,
        ILogger<MainViewModel> logger,
        IFactory factory,
        ISettingsService settingsService,
        ClusterWorkspaceCatalog clusterCatalog,
        IClusterRuntimeCatalog runtimeCatalog,
        IDialogService dialogService,
        IPlatformServices platformServices,
        IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _factory = factory;
        SettingsService = settingsService;
        ClusterCatalog = clusterCatalog;
        _runtimeCatalog = runtimeCatalog;
        _dialogService = dialogService;
        _platformServices = platformServices;
        _httpClientFactory = httpClientFactory;
    }

    internal void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
        {
            return;
        }

        DebugFactoryEvents(_factory);

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
            Layout = layout;
        }

        _ = Task.Run(() => CheckForUpdatesAsync(showNoUpdatesPrompt: false));
    }

    [ObservableProperty]
    public partial IRootDock? Layout { get; set; }

    internal new IFactory Factory => _factory;

    private void DebugFactoryEvents(IFactory factory)
    {
        factory.ActiveDockableChanged += (_, args) =>
        {
            _logger.LogDebug($"[ActiveDockableChanged] Title='{args.Dockable?.Title}'");
        };

        factory.FocusedDockableChanged += (_, args) =>
        {
            _logger.LogDebug($"[FocusedDockableChanged] Title='{args.Dockable?.Title}'");
        };

        factory.DockableAdded += (_, args) =>
        {
            var rightDock = factory.GetDockable<IToolDock>("RightDock");
            var rightSplitter = factory.GetDockable<IProportionalDockSplitter>("RightDockSplitter");
            var bottomDock = factory.GetDockable<IToolDock>("BottomDock");
            var bottomSplitter = factory.GetDockable<IProportionalDockSplitter>("BottomDockSplitter");

            if (rightDock?.VisibleDockables?.Count >= 1 && rightSplitter?.CanResize == false)
            {
                Dispatcher.UIThread.Invoke(() => rightSplitter.CanResize = true);
            }

            if (bottomDock?.VisibleDockables?.Count >= 1 && bottomSplitter?.CanResize == false)
            {
                Dispatcher.UIThread.Invoke(() => bottomSplitter.CanResize = true);
            }

            _logger.LogDebug($"[DockableAdded] Title='{args.Dockable?.Title}'");
        };

        factory.DockableRemoved += (_, args) =>
        {
            var rightDock = factory.GetDockable<IToolDock>("RightDock");
            var rightSplitter = factory.GetDockable<IProportionalDockSplitter>("RightDockSplitter");
            var bottomDock = factory.GetDockable<IToolDock>("BottomDock");
            var bottomSplitter = factory.GetDockable<IProportionalDockSplitter>("BottomDockSplitter");

            if (rightDock?.VisibleDockables?.Count == 0 && rightSplitter?.CanResize == true)
            {
                Dispatcher.UIThread.Invoke(() => rightSplitter.CanResize = false);
            }

            if (bottomDock?.VisibleDockables?.Count == 0 && bottomSplitter?.CanResize == true)
            {
                Dispatcher.UIThread.Invoke(() => bottomSplitter.CanResize = false);
            }

            _logger.LogDebug($"[DockableRemoved] Title='{args.Dockable?.Title}'");
        };

        factory.DockableClosed += (_, args) =>
        {
            _logger.LogDebug($"[DockableClosed] Title='{args.Dockable?.Title}'");
        };

        factory.DockableMoved += (_, args) =>
        {
            _logger.LogDebug($"[DockableMoved] Title='{args.Dockable?.Title}'");
        };

        factory.DockableSwapped += (_, args) =>
        {
            _logger.LogDebug($"[DockableSwapped] Title='{args.Dockable?.Title}'");
        };

        factory.DockablePinned += (_, args) =>
        {
            _logger.LogDebug($"[DockablePinned] Title='{args.Dockable?.Title}'");
        };

        factory.DockableUnpinned += (_, args) =>
        {
            _logger.LogDebug($"[DockableUnpinned] Title='{args.Dockable?.Title}'");
        };

        factory.WindowOpened += (_, args) =>
        {
            _logger.LogDebug($"[WindowOpened] Title='{args.Window?.Title}'");
        };

        factory.WindowClosed += (_, args) =>
        {
            _logger.LogDebug($"[WindowClosed] Title='{args.Window?.Title}'");
        };

        factory.WindowClosing += (_, args) =>
        {
            // NOTE: Set to True to cancel window closing.
#if false
                args.Cancel = true;
#endif
            _logger.LogDebug($"[WindowClosing] Title='{args.Window?.Title}', Cancel={args.Cancel}");
        };

        factory.WindowAdded += (_, args) =>
        {
            _logger.LogDebug($"[WindowAdded] Title='{args.Window?.Title}'");
        };

        factory.WindowRemoved += (_, args) =>
        {
            _logger.LogDebug($"[WindowRemoved] Title='{args.Window?.Title}'");
        };

        factory.WindowMoveDragBegin += (_, args) =>
        {
            // NOTE: Set to True to cancel window dragging.
#if false
                args.Cancel = true;
#endif
            _logger.LogDebug($"[WindowMoveDragBegin] Title='{args.Window?.Title}', Cancel={args.Cancel}, X='{args.Window?.X}', Y='{args.Window?.Y}'");
        };

        factory.WindowMoveDrag += (_, args) =>
        {
            _logger.LogDebug($"[WindowMoveDrag] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
        };

        factory.WindowMoveDragEnd += (_, args) =>
        {
            _logger.LogDebug($"[WindowMoveDragEnd] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
        };
    }

    [RelayCommand]
    private void CloseLayout()
    {
        if (Layout is IDock dock)
        {
            if (dock.Close.CanExecute(null))
            {
                dock.Close.Execute(null);
            }
        }

        Layout = null;
    }

    [RelayCommand]
    private void ResetLayout()
    {
        CloseLayout();

        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            _factory?.InitLayout(layout);
            Layout = layout;
        }
    }

    [RelayCommand]
    private void Close()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            lifetime.TryShutdown();
    }

    [RelayCommand]
    private void OpenAbout()
    {
        var vm = _serviceProvider.GetRequiredService<AboutViewModel>();

        _factory.AddToDocuments(vm);

    }

    [RelayCommand]
    private Task CheckForUpdates()
    {
        return CheckForUpdatesAsync(showNoUpdatesPrompt: true);
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var vm = _serviceProvider.GetRequiredService<SettingsViewModel>();

        _factory.AddToDocuments(vm);
    }

    [RelayCommand]
    private void OpenClusters()
    {
        var vm = _serviceProvider.GetRequiredService<ClusterListViewModel>();

        _factory.AddToDocuments(vm);
    }

    [RelayCommand]
    private void ImportAksCluster()
    {
        var vm = _serviceProvider.GetRequiredService<ImportAksClusterViewModel>();

        _factory.AddToDocuments(vm);
    }

    [RelayCommand]
    private void SwitchTheme()
    {
        SettingsService.Appearance.Theme = SettingsService.Appearance.Theme == LocalThemeVariant.Light ? LocalThemeVariant.Dark : LocalThemeVariant.Light;
        SettingsService.SaveSettings();
    }

    [RelayCommand]
    private void IncreaseFontSize()
    {
        SettingsService.Appearance.FontSize = decimal.Min(
            SettingsService.Appearance.FontSize + 1m,
            AppearanceSettings.MaximumFontSize);
    }

    [RelayCommand]
    private void DecreaseFontSize()
    {
        SettingsService.Appearance.FontSize = decimal.Max(
            SettingsService.Appearance.FontSize - 1m,
            AppearanceSettings.MinimumFontSize);
    }

    [RelayCommand]
    private async Task LoadKubeConfig()
    {
        var files = await _platformServices.OpenFilePickerAsync(new()
        {
            Title = Assets.Resources.MainView_Menu_File_LoadKubeConfig_Open,
            AllowMultiple = true,
        });

        foreach (var file in files)
        {
            try
            {
                _runtimeCatalog.LoadFromConfigFromPath(file.Path.LocalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading kube config");
            }
        }
    }

    private async Task CheckForUpdatesAsync(bool showNoUpdatesPrompt)
    {
        var source = new GithubSource("https://github.com/IvanJosipovic/KubeUI", null, SettingsService.Settings.PreReleaseChannel);

        var arch = "x64";

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            arch = "arm64";
        }

        var channel = $"linux-{arch}";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            channel = $"osx-{arch}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            channel = $"win-{arch}";
        }

        var options = new UpdateOptions() { ExplicitChannel = channel };
        var um = new UpdateManager(source, options);

        if (um.IsInstalled)
        {
            var update = await um.CheckForUpdatesAsync().ConfigureAwait(true);

            if (update is not null)
            {
                IReadOnlyList<(string Version, string? Markdown)> releaseNotes = [];
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient(GitHubReleasesHttpClientName);
                    releaseNotes = await GetReleaseNotesAsync(
                        httpClient,
                        um.CurrentVersion!,
                        update.TargetFullRelease.Version,
                        SettingsService.Settings.PreReleaseChannel).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to retrieve GitHub release notes; showing the update anyway");
                }

                var updatePresentation = CreateUpdatePresentation(
                    um.CurrentVersion?.ToString(),
                    update.TargetFullRelease.Version.ToString(),
                    releaseNotes
                        .Append((update.TargetFullRelease.Version.ToString(), update.TargetFullRelease.NotesMarkdown)));

                await Dispatcher.UIThread.InvokeAsync(() =>
                    ShowUpdateAvailableView(
                        updatePresentation.CurrentVersion,
                        updatePresentation.LatestVersion,
                        updatePresentation.ReleaseNotesMarkdown,
                        async reportProgress =>
                        {
                            await um.DownloadUpdatesAsync(update, reportProgress).ConfigureAwait(false);
                            um.ApplyUpdatesAndRestart(update);
                        }));
            }
            else if (showNoUpdatesPrompt)
            {
                await ShowNoUpdatesAvailableAsync().ConfigureAwait(true);
            }
        }
        else if (showNoUpdatesPrompt)
        {
            await ShowUpdateCheckUnavailableAsync().ConfigureAwait(true);
        }
    }

    internal void ShowUpdateAvailableView(
        string currentVersion,
        string latestVersion,
        string releaseNotesMarkdown,
        Func<Action<int>, Task> updateAsync)
    {
        var vm = _factory.FindDockableById(nameof(UpdateAvailableViewModel)) as UpdateAvailableViewModel
            ?? _serviceProvider.GetRequiredService<UpdateAvailableViewModel>();

        vm.SetUpdate(currentVersion, latestVersion, releaseNotesMarkdown, updateAsync);
        _factory.AddToDocuments(vm);
    }

    internal static (string CurrentVersion, string LatestVersion, string ReleaseNotesMarkdown) CreateUpdatePresentation(
        string? currentVersion,
        string latestVersion,
        IEnumerable<(string Version, string? Markdown)> releases)
    {
        return (
            currentVersion ?? Assets.Resources.UpdateAvailableView_CurrentVersion_Unknown,
            latestVersion,
            UpdateAvailableViewModel.CreateReleaseNotesMarkdown(
                releases.OrderByDescending(release => SemanticVersion.Parse(release.Version))));
    }

    internal static async Task<IReadOnlyList<(string Version, string? Markdown)>> GetReleaseNotesAsync(
        HttpClient httpClient,
        SemanticVersion currentVersion,
        SemanticVersion latestVersion,
        bool includePrereleases,
        CancellationToken cancellationToken = default)
    {
        List<(SemanticVersion Version, string Tag, string? Markdown)> releases = [];
        HashSet<string> seenTags = new(StringComparer.OrdinalIgnoreCase);
        const int releasesPerPage = 10;

        for (var page = 1; ; page++)
        {
            var uri = $"https://api.github.com/repos/IvanJosipovic/KubeUI/releases?per_page={releasesPerPage}&page={page}";
            var githubReleases = await httpClient.GetFromJsonAsync(
                uri,
                GitHubReleaseJsonContext.Default.GitHubReleaseResponseArray,
                cancellationToken).ConfigureAwait(false) ?? [];

            if (githubReleases.Length == 0)
            {
                break;
            }

            var reachedCurrentVersion = false;
            foreach (var release in githubReleases)
            {
                if (string.IsNullOrWhiteSpace(release.TagName)
                    || !seenTags.Add(release.TagName))
                {
                    continue;
                }

                SemanticVersion version;
                try
                {
                    version = SemanticVersion.Parse(release.TagName.TrimStart('v', 'V'));
                }
                catch (ArgumentException)
                {
                    continue;
                }
                if (version.CompareTo(currentVersion) == 0)
                {
                    reachedCurrentVersion = true;
                    continue;
                }

                if (release.Draft
                    || (release.Prerelease && !includePrereleases))
                {
                    continue;
                }

                if (version > currentVersion && !(version > latestVersion))
                {
                    releases.Add((version, release.TagName, release.Body));
                }
            }

            if (reachedCurrentVersion)
            {
                break;
            }
        }

        releases.Sort(static (left, right) => left.Version.CompareTo(right.Version));
        return releases
            .Select(release => (release.Version.ToString(), release.Markdown))
            .ToArray();
    }

    private sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; init; }

        [JsonPropertyName("body")]
        public string? Body { get; init; }

        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }
    }

    [JsonSerializable(typeof(GitHubReleaseResponse[]))]
    private partial class GitHubReleaseJsonContext : JsonSerializerContext;

    private async Task ShowNoUpdatesAvailableAsync()
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.MainView_CheckForUpdates_NoUpdates_Title,
            Content = Assets.Resources.MainView_CheckForUpdates_NoUpdates_Content,
            PrimaryButtonText = Assets.Resources.MainView_CheckForUpdates_NoUpdates_Primary,
            DefaultButton = FAContentDialogButton.Primary
        };

        await _dialogService.ShowContentDialogAsync(this, settings).ConfigureAwait(true);
    }

    private async Task ShowUpdateCheckUnavailableAsync()
    {
        var settings = CreateUpdateCheckUnavailableDialogSettings();

        await _dialogService.ShowContentDialogAsync(this, settings).ConfigureAwait(true);
    }

    internal static ContentDialogSettings CreateUpdateCheckUnavailableDialogSettings()
    {
        return new ContentDialogSettings
        {
            Title = Assets.Resources.MainView_CheckForUpdates_Unavailable_Title,
            Content = Assets.Resources.MainView_CheckForUpdates_Unavailable_Content,
            PrimaryButtonText = Assets.Resources.MainView_CheckForUpdates_Unavailable_Primary,
            DefaultButton = FAContentDialogButton.Primary
        };
    }
}
