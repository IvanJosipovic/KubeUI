using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using KubeUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Orientation = Dock.Model.Core.Orientation;

namespace KubeUI;

[ServiceDescriptor<IFactory>(ServiceLifetime.Singleton)]
public class DockFactory : Factory
{
    private readonly ILogger<DockFactory> _logger;

    public DockFactory(ILogger<DockFactory> logger)
    {
        _logger = logger;
    }

    private IRootDock? _rootDock;
    private IToolDock? _leftDock;
    private IToolDock? _rightDock;
    private IDocumentDock? _documentDock;

    public override IRootDock CreateLayout()
    {
        var nav = Application.Current.GetRequiredService<NavigationViewModel>();
        nav.CanClose = false;
        nav.CanFloat = false;

        var leftDock = new ToolDock
        {
            Alignment = Alignment.Left,
            CanClose = false,
            Dock = DockMode.Left,
            Id = "LeftDock",
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(nav),
        };

        var rightDock = new ToolDock
        {
            Alignment = Alignment.Right,
            CanClose = false,
            Dock = DockMode.Right,
            Id = "RightDock",
            Proportion = 0.3,
            VisibleDockables = CreateList<IDockable>(),
        };

        var home = Application.Current.GetRequiredService<HomeViewModel>();

        var documentDock = new DocumentDock
        {
            CanClose = false,
            CanCreateDocument = false,
            Dock = DockMode.Center,
            Id = "Documents",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(home),
        };

        var mainLayout2 = new ProportionalDock
        {
            CanClose = false,
            Id = "DocumentPropDock",
            IsCollapsable = false,
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock
            ),
        };

        var mainLayout = new ProportionalDock
        {
            Id = "MainLayout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                leftDock,
                new ProportionalDockSplitter() { CanClose = false },
                mainLayout2,
                new ProportionalDockSplitter() {  CanClose = false },
                rightDock
            ),
        };

        var rootDock = CreateRootDock();
        rootDock.Id = "Root";
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
        rootDock.IsCollapsable = false;

        _documentDock = documentDock;
        _rootDock = rootDock;
        _leftDock = leftDock;
        _rightDock = rightDock;

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
            ["RightDock"] = () => _rightDock,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    /// <summary>
    /// Prevents Navigation from being moved
    /// </summary>
    /// <param name="sourceDock"></param>
    /// <param name="targetDock"></param>
    /// <param name="sourceDockable"></param>
    /// <param name="targetDockable"></param>
    public override void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        if (sourceDockable.Id == "Navigation")
        {
            return;
        }

        base.MoveDockable(sourceDock, targetDock, sourceDockable, targetDockable);
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
}

public static class FactoryExtensions
{
    public static IDockable? FindDockableById(this IFactory factory, string id)
    {
        return factory.Find(x => string.Equals(x.Id, id, StringComparison.Ordinal)).FirstOrDefault();
    }

    public static bool AddToDocuments(this IFactory factory, IDockable vm, bool duplicateCheck = true)
    {
        var documents = factory.GetDockable<IDocumentDock>("Documents");

        if (duplicateCheck)
        {
            var existing = factory.FindDockableById(vm.Id);

            if (existing != null)
            {
                factory.SetActiveDockable(existing);
                factory.SetFocusedDockable(documents, existing);

                return false;
            }
        }

        factory.AddDockable(documents, vm);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);

        return true;
    }

    public static bool AddToBottom(this IFactory factory, IDockable vm, bool duplicateCheck = true)
    {
        var documents = factory.GetDockable<IDocumentDock>("Documents");

        if (documents.Owner is ProportionalDock propDock)
        {
            var tools = propDock.VisibleDockables.FirstOrDefault(x => x is IToolDock) as IToolDock;

            if (tools == null)
            {
                factory.AddDockable(propDock, factory.CreateProportionalDockSplitter());

                tools = factory.CreateToolDock();
                tools.Alignment = Alignment.Bottom;

                factory.AddDockable(propDock, tools);
            }

            if (documents.Proportion == 1)
            {
                documents.Proportion = 0.4;
                tools.Proportion = 0.6;
            }

            if (duplicateCheck)
            {
                var existing = factory.FindDockableById(vm.Id);

                if (existing != null)
                {
                    factory.SetActiveDockable(existing);
                    factory.SetFocusedDockable(tools, existing);

                    return false;
                }
            }

            factory.AddDockable(tools, vm);
            factory.SetActiveDockable(vm);
            factory.SetFocusedDockable(tools, vm);

            return true;
        }

        throw new NotImplementedException();
    }
}
