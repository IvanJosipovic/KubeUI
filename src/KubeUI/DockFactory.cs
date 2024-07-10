using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using KubeUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace KubeUI;

[ServiceDescriptor<IFactory>(ServiceLifetime.Singleton)]
public class DockFactory : Factory
{
    private readonly ILogger<DockFactory> _logger;

    public DockFactory(ILogger<DockFactory> logger)
    {
        _logger = logger;
    }

    public IRootDock? RootDock;
    private IToolDock? LeftDock;
    private IToolDock? RightDock;
    public IDocumentDock? DocumentDock;

    public override IDocumentDock CreateDocumentDock() => new CustomDocumentDock();

    public override IRootDock CreateLayout()
    {
        var nav = Application.Current.GetRequiredService<NavigationViewModel>();
        nav.Title = "Navigation";
        nav.Id = "Navigation";
        nav.CanClose = false;
        nav.CanFloat = false;

        var leftDock = new ToolDock
        {
            Id = "LeftDock",
            Title = "LeftDock",
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(nav),
            Alignment = Alignment.Left,
            Dock = DockMode.Left,
            CanClose = false
        };

        var rightDock = new ToolDock
        {
            Id = "RightDock",
            Title = "RightDock",
            Proportion = 0.3,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Right,
            Dock = DockMode.Right,
            CanClose = false
        };

        var home = Application.Current.GetRequiredService<HomeViewModel>();
        home.Title = "Home";
        home.Id = "Home";

        var documentDock = new CustomDocumentDock
        {
            Title = "Documents",
            Id = "Documents",
            VisibleDockables = CreateList<IDockable>(home),
            CanCreateDocument = true,
            IsCollapsable = false,
            Dock = DockMode.Center
        };

        var mainLayout2 = new ProportionalDock
        {
            Id = "DocumentPropDock",
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock
            ),
            CanClose = false
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
            )
        };

        var rootDock = CreateRootDock();
        rootDock.Title = "Root";
        rootDock.Id = "Root";
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
        rootDock.IsCollapsable = false;

        DocumentDock = documentDock;
        RootDock = rootDock;
        LeftDock = leftDock;
        RightDock = rightDock;

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
            ["Root"] = () => RootDock,
            ["Documents"] = () => DocumentDock,
            ["LeftDock"] = () => LeftDock,
            ["RightDock"] = () => RightDock,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    /// <summary>
    /// Fixes Float on ToolDock floating the whole dock.
    /// </summary>
    /// <param name="dockable"></param>
    public override void FloatDockable(IDockable dockable)
    {
        if (dockable is IToolDock tool)
        {
            dockable = tool.ActiveDockable;
        }

        base.FloatDockable(dockable);
    }

    public override void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        if (sourceDockable.Id == "Navigation")
        {
            return;
        }

        base.MoveDockable(sourceDock, targetDock, sourceDockable, targetDockable);
    }

    public override void CloseDockable(IDockable dockable)
    {
        base.CloseDockable(dockable);

        if (dockable is IDisposable disp)
        {
            disp.Dispose();
        }
    }

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
    public static IDockable? FindDockableById(this IFactory factory, IDockable dockable)
    {
        return factory.Find(x => string.Equals(x.Id, dockable.Id, StringComparison.Ordinal)).FirstOrDefault();
    }

    public static bool AddToDocuments(this IFactory factory, IDockable vm, bool duplicateCheck = true)
    {
        var documents = factory.GetDockable<IDocumentDock>("Documents");

        if (duplicateCheck)
        {
            var existing = factory.FindDockableById(vm);

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
                var existing = factory.FindDockableById(vm);

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
