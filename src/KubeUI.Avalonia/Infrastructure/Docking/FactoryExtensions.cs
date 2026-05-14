using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Avalonia.Infrastructure.Docking;

namespace KubeUI.Avalonia.Infrastructure.Docking;

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

            if (vm is IDisposable disp)
            {
                disp.Dispose();
            }

            return false;
        }

        var insertIndex = documents.VisibleDockables?.Count ?? 0;
        factory.InsertDockable(documents, vm, insertIndex);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);

        return true;
    }

    public static bool AddToBottom(this IFactory factory, IDockable vm)
    {
        var bottomToolsDock = factory.GetDockable<IToolDock>("BottomDock")!;

        var existing = factory.FindDockableById(vm.Id);

        if (existing != null)
        {
            factory.SetActiveDockable(existing);
            factory.SetFocusedDockable(bottomToolsDock, existing);

            return false;
        }

        var insertIndex = bottomToolsDock.VisibleDockables?.Count ?? 0;
        factory.InsertDockable(bottomToolsDock, vm, insertIndex);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(bottomToolsDock, vm);

        return true;
    }

    public static void AddToRight(this IFactory factory, IDockable vm)
    {
        var rightDock = factory.GetDockable<IToolDock>("RightDock");

        var root = factory.GetDockable<IRootDock>("Root");

        var pinnedDoc = root.RightPinnedDockables?.FirstOrDefault(x => x.Id == vm.Id);

        if (pinnedDoc != null)
        {
            vm.PinnedBounds = pinnedDoc.PinnedBounds;
            factory.RemoveDockable(pinnedDoc, false);
        }

        var existingDock = factory.FindDockableById(vm.Id);

        if (existingDock != null)
        {
            factory.RemoveDockable(existingDock, false);
        }

        var insertIndex = rightDock.VisibleDockables?.Count ?? 0;
        factory.InsertDockable(rightDock, vm, insertIndex);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(rightDock, vm);

        if (pinnedDoc != null)
        {
            factory.PinDockable(vm);
            factory.PreviewPinnedDockable(vm);
        }
    }
}

