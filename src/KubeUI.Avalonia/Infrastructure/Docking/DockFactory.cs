using System.Runtime.Serialization;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Orientation = Dock.Model.Core.Orientation;

namespace KubeUI.Avalonia.Infrastructure.Docking;

public class DockFactory : Factory
{
    private readonly ILogger<DockFactory> _logger;

    public DockFactory(ILogger<DockFactory> logger)
    {
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
        var nav = Application.Current.GetRequiredService<NavigationViewModel>();
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

        var home = Application.Current.GetRequiredService<HomeViewModel>();

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
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    /// <summary>
    /// Runs dispose on closed Dockables
    /// </summary>
    /// <param name="dockable"></param>
    public override void CloseDockable(IDockable dockable)
    {
        base.CloseDockable(dockable);

        if (dockable is IDisposable disp)
        {
            disp.Dispose();
        }
    }

    /// <summary>
    /// Prevents closing dockables that shouldn't be closed
    /// </summary>
    /// <param name="dockable"></param>
    /// <param name="collapse"></param>
    public override void RemoveDockable(IDockable dockable, bool collapse)
    {
        if (!dockable.CanClose)
        {
            return;
        }

        base.RemoveDockable(dockable, collapse);
    }

    public override void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
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
}


