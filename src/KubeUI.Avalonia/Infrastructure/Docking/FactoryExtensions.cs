using Dock.Model.Controls;
using Dock.Model.Core;

namespace KubeUI.Avalonia.Infrastructure.Docking;

public static class FactoryExtensions
{
    public static IDockable? FindDockableById(this IFactory factory, string id)
    {
        return factory.Find(x => string.Equals(x.Id, id, StringComparison.Ordinal)).FirstOrDefault()
            ?? factory.GetDockable<IDocumentDock>("Documents")?.VisibleDockables?
                .FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.Ordinal))
            ?? factory.GetDockable<IToolDock>("RightDock")?.VisibleDockables?
                .FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.Ordinal))
            ?? factory.GetDockable<IToolDock>("BottomDock")?.VisibleDockables?
                .FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.Ordinal));
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

    /// <summary>
    /// Shows <paramref name="vm"/> in the right tool dock, replacing (and disposing) any dockable with the same id.
    /// If the replaced dockable was pinned, the new one is pinned in its place.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="vm">The dockable to show.</param>
    /// <param name="previewPinned">Whether a pinned replacement is temporarily shown; pass <see langword="false"/> to keep it collapsed.</param>
    public static void AddToRight(this IFactory factory, IDockable vm, bool previewPinned = true)
    {
        var rightDock = factory.GetDockable<IToolDock>("RightDock");

        var pinnedDoc = factory.FindRightPinnedDockable(vm.Id);

        if (pinnedDoc != null)
        {
            vm.PinnedBounds = pinnedDoc.PinnedBounds;
            factory.RemoveDockable(pinnedDoc, false);
            DisposeReplaced(pinnedDoc, vm);
        }

        var existingDock = factory.FindDockableById(vm.Id);

        if (existingDock != null)
        {
            factory.RemoveDockable(existingDock, false);
            DisposeReplaced(existingDock, vm);
        }

        var insertIndex = rightDock.VisibleDockables?.Count ?? 0;
        factory.InsertDockable(rightDock, vm, insertIndex);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(rightDock, vm);

        if (pinnedDoc != null)
        {
            factory.PinDockable(vm);
            if (previewPinned)
            {
                factory.PreviewPinnedDockable(vm);
            }
        }
    }

    /// <summary>
    /// Finds a dockable with <paramref name="id"/> that is pinned to the right auto-hide strip.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="id">The dockable id.</param>
    /// <returns>The pinned dockable, or <see langword="null"/> when none is pinned.</returns>
    public static IDockable? FindRightPinnedDockable(this IFactory factory, string id)
    {
        var root = factory.GetDockable<IRootDock>("Root");
        return root?.RightPinnedDockables?.FirstOrDefault(x => x.Id == id);
    }

    private static void DisposeReplaced(IDockable replaced, IDockable replacement)
    {
        if (!ReferenceEquals(replaced, replacement) && replaced is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
