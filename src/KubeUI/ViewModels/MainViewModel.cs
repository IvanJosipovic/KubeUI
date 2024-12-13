using System.Runtime.InteropServices;
using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Client;
using Velopack.Sources;
using Velopack;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs;

namespace KubeUI.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly ILogger<MainViewModel> _logger;

    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    public partial ClusterManager ClusterManager { get; set; }

    private readonly IDialogService _dialogService;

    public MainViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<MainViewModel>>();

        _settingsService = Application.Current.GetRequiredService<ISettingsService>();

        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();

        _dialogService = Application.Current.GetRequiredService<IDialogService>();

        DebugFactoryEvents(Factory);

        Layout = Factory?.CreateLayout();
        if (Layout is not null)
        {
            Factory?.InitLayout(Layout);
        }

        _ = Task.Run(CheckForUpdates);
    }

    [ObservableProperty]
    public partial IRootDock? Layout { get; set; }

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
            _logger.LogDebug($"[DockableAdded] Title='{args.Dockable?.Title}'");
        };

        factory.DockableRemoved += (_, args) =>
        {
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
    }

    [RelayCommand]
    private void ResetLayout()
    {
        if (Layout is not null)
        {
            if (Layout.Close.CanExecute(null))
            {
                Layout.Close.Execute(null);
            }
        }

        var layout = Factory?.CreateLayout();
        if (layout is not null)
        {
            Layout = layout;
            Factory?.InitLayout(layout);
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
        var vm = Application.Current.GetRequiredService<AboutViewModel>();

        Factory?.AddToDocuments(vm);

        _ = Task.Run(CheckForUpdates);
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var vm = Application.Current.GetRequiredService<SettingsViewModel>();

        Factory?.AddToDocuments(vm);
    }

    [RelayCommand]
    private void OpenClusters()
    {
        var vm = Application.Current.GetRequiredService<ClusterListViewModel>();

        Factory?.AddToDocuments(vm);
    }

    [RelayCommand]
    private void SwitchTheme()
    {
        _settingsService.Settings.Theme = _settingsService.Settings.Theme == LocalThemeVariant.Light ? LocalThemeVariant.Dark : LocalThemeVariant.Light;
        _settingsService.SaveSettings();
    }

    private async Task CheckForUpdates()
    {
        var sor = new GithubSource("https://github.com/IvanJosipovic/KubeUI", null, _settingsService.Settings.PreReleaseChannel);

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

        var um = new UpdateManager(sor, new UpdateOptions() { ExplicitChannel = channel }, _logger);

        if (um.IsInstalled)
        {
            var update = await um.CheckForUpdatesAsync().ConfigureAwait(true);

            if (update != null)
            {
                ContentDialogSettings settings = new()
                {
                    Title = Resources.MainViewModel_CheckForUpdates_Title,
                    Content = Resources.MainViewModel_CheckForUpdates_Content,
                    PrimaryButtonText = Resources.MainViewModel_CheckForUpdates_Primary,
                    SecondaryButtonText = Resources.MainViewModel_CheckForUpdates_Secondary,
                    DefaultButton = ContentDialogButton.Secondary
                };

                var result = await _dialogService.ShowContentDialogAsync(this, settings).ConfigureAwait(true);

                if (result == ContentDialogResult.Primary)
                {
                    ContentDialogSettings updatePrompt = new()
                    {
                        Title = Resources.MainViewModel_CheckForUpdates_Updating_Title,
                        Content = Resources.MainViewModel_CheckForUpdates_Updating_Content,
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false
                    };

                    _ = _dialogService.ShowContentDialogAsync(this, updatePrompt);

                    await um.DownloadUpdatesAsync(update).ConfigureAwait(false);
                    um.ApplyUpdatesAndRestart(update);
                }
            }
        }
    }
}
