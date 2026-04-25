using System.Runtime.InteropServices;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Catalog.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Documents.About.ViewModels;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks.ViewModels;
using KubeUI.Avalonia.Shell.Documents.Settings.ViewModels;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Velopack;
using Velopack.Sources;

namespace KubeUI.Avalonia.Shell.Main.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IFactory _factory = null!;

    [ObservableProperty]
    private partial ISettingsService SettingsService { get; set; }

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    private readonly IDialogService _dialogService;

    public MainViewModel(
        IServiceProvider serviceProvider,
        ILogger<MainViewModel> logger,
        IFactory factory,
        ISettingsService settingsService,
        ClusterWorkspaceCatalog clusterCatalog,
        IDialogService dialogService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _factory = factory;
        SettingsService = settingsService;
        ClusterCatalog = clusterCatalog;
        _dialogService = dialogService;

        DebugFactoryEvents(_factory);

        Layout = _factory?.CreateLayout();
        if (Layout is not null)
        {
            _factory?.InitLayout(Layout);
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

        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            Layout = layout;
            _factory?.InitLayout(layout);
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

        _ = Task.Run(CheckForUpdates);
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
    private async Task LoadKubeConfig()
    {
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = Assets.Resources.MainView_Menu_File_LoadKubeConfig_Open,
            AllowMultiple = true,
        });

        foreach (var file in files)
        {
            try
            {
                ClusterCatalog.LoadFromConfigFromPath(file.Path.LocalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading kube config");
            }
        }
    }

    private async Task CheckForUpdates()
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

        var um = new UpdateManager(source, new UpdateOptions() { ExplicitChannel = channel });

        if (um.IsInstalled)
        {
            var update = await um.CheckForUpdatesAsync().ConfigureAwait(true);

            if (update != null)
            {
                ContentDialogSettings settings = new()
                {
                    Title = Assets.Resources.MainViewModel_CheckForUpdates_Title,
                    Content = Assets.Resources.MainViewModel_CheckForUpdates_Content + "\n\n" + Markdig.Markdown.ToPlainText(update.TargetFullRelease.NotesMarkdown),
                    PrimaryButtonText = Assets.Resources.MainViewModel_CheckForUpdates_Primary,
                    SecondaryButtonText = Assets.Resources.MainViewModel_CheckForUpdates_Secondary,
                    DefaultButton = FAContentDialogButton.Secondary
                };

                var result = await _dialogService.ShowContentDialogAsync(this, settings).ConfigureAwait(true);

                if (result == FAContentDialogResult.Primary)
                {
                    ContentDialogSettings updatePrompt = new()
                    {
                        Title = Assets.Resources.MainViewModel_CheckForUpdates_Updating_Title,
                        Content = Assets.Resources.MainViewModel_CheckForUpdates_Updating_Content,
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



