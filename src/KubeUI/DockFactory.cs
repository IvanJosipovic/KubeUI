using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;
using k8s;
using KubeUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrutor;

namespace KubeUI;

[ServiceDescriptor<IFactory>(ServiceLifetime.Singleton)]
public class DockFactory : Factory
{
    private readonly ILogger<DockFactory> _logger;

    public DockFactory(ILogger<DockFactory> logger)
    {
        _logger = logger;

        KubernetesClientConfiguration.ExecStdError += KubernetesClientConfiguration_ExecStdError;
    }

    private void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
    {
        var doc = GetDockable<IDocumentDock>("Documents");

        var vm = new ClusterErrorViewModel()
        {
            Error = e.Data
        };

        Dispatcher.UIThread.Invoke(() =>
        {
            AddDockable(doc, vm);
            SetActiveDockable(vm);
            SetFocusedDockable(doc, vm);
        });
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

        var tools = CreateToolDock();
        tools.Alignment = Alignment.Bottom;
        tools.Proportion = 0;

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

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        return base.CreateWindowFrom(dockable);
    }

    public override IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation)
    {
        return base.CreateSplitLayout(dock, dockable, operation);
    }

    public override void MoveDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
    {
        base.MoveDockable(dock, sourceDockable, targetDockable);
    }

    public override void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        base.MoveDockable(sourceDock, targetDock, sourceDockable, targetDockable);
    }

    public override void OnWindowMoveDragEnd(IDockWindow? window)
    {
        base.OnWindowMoveDragEnd(window);
    }

    public override void OnWindowMoveDrag(IDockWindow? window)
    {
        base.OnWindowMoveDrag(window);
    }

    public override void OnDockableMoved(IDockable? dockable)
    {
        base.OnDockableMoved(dockable);
    }

    public override void SwapDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
    {
        base.SwapDockable(dock, sourceDockable, targetDockable);
    }

    public override void SwapDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable)
    {
        base.SwapDockable(sourceDock, targetDock, sourceDockable, targetDockable);
    }

    public override void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        base.SplitToDock(dock, dockable, operation);
    }

    public override void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height)
    {
        base.SplitToWindow(dock, dockable, x, y, width, height);
    }

    public override void AddDockable(IDock dock, IDockable dockable)
    {
        base.AddDockable(dock, dockable);
    }

    public override void InitDockWindow(IDockWindow window, IDockable? owner)
    {
        base.InitDockWindow(window, owner);
    }

    public override void AddWindow(IRootDock rootDock, IDockWindow window)
    {
        base.AddWindow(rootDock, window);
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

    public void AddToDocumentBottom(IDockable vm)
    {
        if (DocumentDock.Owner is ProportionalDock propDock)
        {
            var tools = propDock.VisibleDockables.FirstOrDefault(x => x is IToolDock) as IToolDock;

            if (tools == null)
            {
                AddDockable(propDock, CreateProportionalDockSplitter());

                tools = CreateToolDock();
                tools.Alignment = Alignment.Bottom;

                AddDockable(propDock, tools);
            }

            if (DocumentDock.Proportion == 1)
            {
                DocumentDock.Proportion = 0.4;
                tools.Proportion = 0.6;
            }

            AddDockable(tools, vm);
            SetActiveDockable(vm);
            SetFocusedDockable(tools, vm);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public override IDockWindow CreateDockWindow() => new DockWindow()
    {
        Topmost = false
    };
}
