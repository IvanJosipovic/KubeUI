using Avalonia.Platform;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Shell.Navigation;
using Orientation = Dock.Model.Core.Orientation;

namespace KubeUI.Avalonia.Infrastructure.Docking;

public class DockFactory : Factory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DockFactory> _logger;

    public DockFactory(IServiceProvider serviceProvider, ILogger<DockFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private IRootDock? _rootDock;
    private IToolDock? _leftDock;

    private IDocumentDock? _documentDock;
    private IToolDock? _bottomDock;
    private IProportionalDockSplitter? _bottomDockSplitter;

    private IToolDock? _rightDock;
    private IProportionalDockSplitter? _rightDockSplitter;

    public override IRootDock CreateLayout()
    {
        var nav = _serviceProvider.GetRequiredService<NavigationViewModel>();
        nav.CanClose = false;
        nav.CanFloat = false;
        nav.CanDrag = false;
        nav.CanDockAsDocument = false;

        _leftDock = new ToolDock
        {
            Alignment = Alignment.Left,
            CanDrag = false,
            CanClose = false,
            Dock = DockMode.Left,
            Id = "LeftDock",
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(nav)
        };

        _rightDock = new ToolDock
        {
            Alignment = Alignment.Right,
            CanDrag = false,
            CanClose = false,
            Dock = DockMode.Right,
            Id = "RightDock",
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>()
        };

        _bottomDock = new ToolDock()
        {
            Alignment = Alignment.Bottom,
            CanDrag = false,
            CanClose = false,
            Id = "BottomDock",
            Proportion = 0.4,
            VisibleDockables = CreateList<IDockable>()
        };

        var home = _serviceProvider.GetRequiredService<HomeViewModel>();

        _documentDock = new DocumentDock
        {
            CanClose = false,
            CanCreateDocument = false,
            Dock = DockMode.Center,
            Id = "Documents",
            IsCollapsable = false,
            Proportion = 0.8,
            VisibleDockables = CreateList<IDockable>(home)
        };

        _bottomDockSplitter = new ProportionalDockSplitter() { Id = "BottomDockSplitter", CanResize = false, CanClose = false };

        var mainLayout2 = new ProportionalDock
        {
            Id = "DocumentPropDock",
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>
            (
                _documentDock,
                _bottomDockSplitter,
                _bottomDock
            ),
            CanClose = false
        };

        _rightDockSplitter = new ProportionalDockSplitter() { Id = "RightDockSplitter", CanResize = false, CanClose = false };

        var mainLayout = new ProportionalDock
        {
            Id = "MainLayout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                _leftDock,
                new ProportionalDockSplitter(),
                mainLayout2,
                _rightDockSplitter,
                _rightDock
            ),
        };

        _rootDock = CreateRootDock();
        _rootDock.Id = "Root";
        _rootDock.ActiveDockable = mainLayout;
        _rootDock.DefaultDockable = mainLayout;
        _rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
        _rootDock.IsCollapsable = false;
        _rootDock.EnableGlobalDocking = false;

        if (_serviceProvider.GetRequiredService<ISettingsService>().Settings.McpServerEnabled)
        {
            var chat = _serviceProvider.GetRequiredService<AgentChatViewModel>();
            chat.SetPinnedBounds(0, 0, 420, 0); //todo make this dynamic based on window size
            _rootDock.RightPinnedDockables.Add(chat);
        }

        return _rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            ["Dashboard"] = () => layout
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            ["Root"] = () => _rootDock,
            ["Documents"] = () => _documentDock,

            ["BottomDock"] = () => _bottomDock,
            ["BottomDockSplitter"] = () => _bottomDockSplitter,

            ["LeftDock"] = () => _leftDock,
            ["RightDock"] = () => _rightDock,
            ["RightDockSplitter"] = () => _rightDockSplitter,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow
            {
                Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://KubeUI.Avalonia/Assets/icon.ico")))
            }
        };

        base.InitLayout(layout);
    }

    /// <summary>
    /// Runs dispose on closed Dockables
    /// </summary>
    /// <param name="dockable"></param>
    public override void CloseDockable(IDockable dockable)
    {
        try
        {
            base.CloseDockable(dockable);

            if (dockable is IDisposable disp)
            {
                disp.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing dockable");
        }
    }

    /// <summary>
    /// Keeps non-closeable layout docks available during structural cleanup.
    /// </summary>
    /// <param name="dockable">The dockable being removed.</param>
    /// <param name="collapse">Whether empty parent docks may collapse.</param>
    public override void RemoveDockable(IDockable dockable, bool collapse)
    {
        try
        {
            if (!dockable.CanClose)
            {
                return;
            }

            base.RemoveDockable(dockable, collapse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing dockable");
        }
    }

    /// <summary>
    /// Creates a document-hosted floating window for tools opened in the main document dock.
    /// </summary>
    /// <param name="dockable">The dockable to float.</param>
    /// <returns>The floating window, or <see langword="null"/> when it cannot be created.</returns>
    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        IDockWindow? window;

        if (dockable is not ITool)
        {
            window = base.CreateWindowFrom(dockable);
        }
        else
        {
            var sourceDocumentDock = dockable.Owner as IDocumentDock;
            var targetDocumentDock = CreateDocumentDock();
            targetDocumentDock.Title = nameof(IDocumentDock);
            targetDocumentDock.Id = (dockable.Owner as IDock)?.Id ?? dockable.Id;
            targetDocumentDock.CanCreateDocument = sourceDocumentDock?.CanCreateDocument ?? false;
            targetDocumentDock.EnableWindowDrag = sourceDocumentDock?.EnableWindowDrag ?? false;
            targetDocumentDock.VisibleDockables = CreateList<IDockable>();

            if (sourceDocumentDock is IDocumentDockContent sourceContent
                && targetDocumentDock is IDocumentDockContent targetContent)
            {
                targetContent.DocumentTemplate = sourceContent.DocumentTemplate;
            }

            AddDockable(targetDocumentDock, dockable);
            targetDocumentDock.ActiveDockable = dockable;
            window = base.CreateWindowFrom(targetDocumentDock);
        }

        if (window is not null)
        {
            var floatingWindowCount = _rootDock?.Windows?.Count ?? HostWindows.Count;
            window.Title = $"KubeUI {floatingWindowCount + 2}";
        }

        return window;
    }

    /// <summary>
    /// Docks a dockable into the main document dock, including dockables hosted
    /// in a floating window.
    /// </summary>
    /// <param name="dockable">The dockable to dock as a document.</param>
    public override void DockAsDocument(IDockable dockable)
    {
        if (!dockable.CanDockAsDocument
            || dockable.Owner is not IDock sourceDock
            || _documentDock is null)
        {
            return;
        }

        if (sourceDock == _documentDock)
        {
            return;
        }

        var targetDockable = _documentDock.VisibleDockables?.LastOrDefault();
        MoveDockable(sourceDock, _documentDock, dockable, targetDockable);
        SetActiveDockable(dockable);
        SetFocusedDockable(_documentDock, dockable);
    }

    public override void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        try
        {
            var orgProportion = dockable.Proportion;

            base.SplitToDock(dock, dockable, operation);

            //Fixes proportion of dockable when splitting
            if (dock.Owner is ProportionalDock)
            {
                if (orgProportion == 1 || double.IsNaN(orgProportion))
                {
                    dockable.Proportion = 0.5;
                    dock.Proportion = 0.5;
                }
                else
                {
                    dock.Proportion = 1 - orgProportion;
                    dockable.Proportion = orgProportion;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting dockable");
        }
    }
}
