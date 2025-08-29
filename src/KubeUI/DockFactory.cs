using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Orientation = Dock.Model.Core.Orientation;

namespace KubeUI;

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

    public override IRootDock CreateLayout()
    {
        var nav = Application.Current.GetRequiredService<NavigationViewModel>();
        nav.CanClose = false;
        nav.CanFloat = false;
        nav.CanDrag = false;

        var leftDock = new ToolDock
        {
            Alignment = Alignment.Left,
            CanClose = false,
            Dock = DockMode.Left,
            Id = "LeftDock",
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(nav)
        };


        var home = Application.Current.GetRequiredService<HomeViewModel>();

        var documentDock = new DocumentDock
        {
            CanClose = false,
            CanCreateDocument = false,
            Dock = DockMode.Center,
            Id = "Documents",
            IsCollapsable = false,
            Proportion = 0.8,
            VisibleDockables = CreateList<IDockable>(home)
        };

        var mainLayout = new ProportionalDock
        {
            Id = "MainLayout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                leftDock,
                new ProportionalDockSplitter() { CanClose = false },
                documentDock
            ),
        };

        var rootDock = CreateRootDock();
        rootDock.Id = "Root";
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
        rootDock.IsCollapsable = false;
        rootDock.EnableGlobalDocking = false;

        _documentDock = documentDock;
        _rootDock = rootDock;
        _leftDock = leftDock;

        return rootDock;
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
            ["LeftDock"] = () => _leftDock,
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

    public override void DockAsDocument(IDockable dockable)
    {
        if (dockable.Id == "Navigation")
        {
            return;
        }

        base.DockAsDocument(dockable);
    }
}

public static class FactoryExtensions
{
    public static IDockable? FindDockableById(this IFactory factory, string id)
    {
        return factory.Find(x => string.Equals(x.Id, id, StringComparison.Ordinal)).FirstOrDefault();
    }

    public static bool AddToDocuments(this IFactory factory, IDockable vm)
    {
        var documents = factory.GetDockable<IDocumentDock>("Documents");

        var existing = factory.FindDockableById(vm.Id);

        if (existing != null)
        {
            factory.SetActiveDockable(existing);
            factory.SetFocusedDockable(documents, existing);

            return false;
        }

        factory.AddDockable(documents, vm);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);

        return true;
    }

    public static bool AddToBottom(this IFactory factory, IDockable vm)
    {
        var documents = factory.GetDockable<IDocumentDock>("Documents")!;
        IToolDock? bottomToolsDock = null;

        //find owners which are a IProportionalDock of Orientation Vertical which contains IDocumentDock
        var props = factory.Find(x => x is ProportionalDock dock && dock.Orientation == Orientation.Vertical).Distinct();

        foreach (var prop in props.Cast<ProportionalDock>())
        {
            var contains = prop.VisibleDockables?.Contains(documents);
            if (contains == true)
            {
                bottomToolsDock = prop.VisibleDockables?.LastOrDefault(x => x is IToolDock) as IToolDock;

                if (bottomToolsDock != null)
                {
                    break;
                }
            }
        }



        if (bottomToolsDock == null)
        {
            bottomToolsDock = factory.CreateToolDock();
            bottomToolsDock.Alignment = Alignment.Bottom;
            bottomToolsDock.Proportion = 0.4;

            factory.SplitToDock(documents, bottomToolsDock, DockOperation.Bottom);
        }

        var existing = factory.FindDockableById(vm.Id);

        if (existing != null)
        {
            factory.SetActiveDockable(existing);
            factory.SetFocusedDockable(bottomToolsDock, existing);

            return false;
        }

        factory.AddDockable(bottomToolsDock, vm);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(bottomToolsDock, vm);

        return true;
    }

    public static bool AddToRight(this IFactory factory, IDockable vm)
    {
        var @new = true;

        var rightDock = factory.FindDockableById("RightDock") as IToolDock;

        if (rightDock == null)
        {
            var documents = factory.GetDockable<IDocumentDock>("Documents");

            rightDock = new ToolDock
            {
                Alignment = Alignment.Right,
                CanDrag = false,
                Dock = DockMode.Right,
                Id = "RightDock",
                Proportion = 0.2,
                VisibleDockables = factory.CreateList<IDockable>()
            };

            // We need to split the Documents
            factory.SplitToDock(documents, rightDock, DockOperation.Right);
        }

        var root = factory.GetDockable<IRootDock>("Root");
        var pinnedDoc = root.RightPinnedDockables?.FirstOrDefault(x => x.Id == vm.Id);

        if (pinnedDoc != null)
        {
            factory.RemoveDockable(pinnedDoc, false);
            @new = false;
        }

        var existingDock = rightDock.VisibleDockables.FirstOrDefault(x => x.Id == vm.Id);

        if (existingDock != null)
        {
            factory.RemoveDockable(existingDock, false);
            @new = false;
        }

        factory?.InsertDockable(rightDock, vm, 0);
        factory?.SetActiveDockable(vm);
        factory?.SetFocusedDockable(rightDock, vm);

        return @new;
    }
}
