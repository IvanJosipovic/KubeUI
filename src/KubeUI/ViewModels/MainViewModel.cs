using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Client;
using Velopack.Sources;
using Velopack;
using Microsoft.Extensions.Logging;

namespace KubeUI.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private ClusterManager _clusterManager;

    public MainViewModel()
    {
        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();

        _logger = Application.Current.GetRequiredService<ILogger<MainViewModel>>();

        _factory = Application.Current.GetRequiredService<IFactory>();

        DebugFactoryEvents(_factory);

        Layout = _factory?.CreateLayout();
        if (Layout is not null)
        {
            _factory?.InitLayout(Layout);
        }
    }

    private readonly IFactory? _factory;

    [ObservableProperty]
    private IRootDock? _layout;

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
        var app = Application.Current.GetRequiredService<IControlledApplicationLifetime>();
        app.Shutdown();
    }

    [RelayCommand]
    private void OpenAbout()
    {
        CheckVersion();

        var doc = _factory.GetDockable<IDocumentDock>("Documents");

        var about = Application.Current.GetRequiredService<AboutViewModel>();
        about.Title = "About";
        about.Id = "About";

        _factory?.AddDockable(doc, about);
        _factory?.SetActiveDockable(about);
    }

    private void CheckVersion()
    {
        var sor = new GithubSource("https://github.com/IvanJosipovic/KubeUI2", null, true);

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
            var update = um.CheckForUpdates();

            if (update != null)
            {
                um.DownloadUpdates(update);
                um.ApplyUpdatesAndExit(update);
            }
        }
    }
}
