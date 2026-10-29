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

    /// <summary>
    /// Adds a document, or updates and focuses the open document with the same identifier.
    /// </summary>
    /// <typeparam name="T">Type of the document to add or update.</typeparam>
    /// <param name="factory">Dock factory that owns the document dock.</param>
    /// <param name="document">
    /// Candidate document. The view model must set <see cref="IDockable.Id"/> before this call.
    /// </param>
    /// <param name="update">Applies request state to the live document, whether newly added or already open.</param>
    /// <returns>The live document instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// The document has no identifier, or a different document type owns the identifier.
    /// </exception>
    public static T AddOrUpdateToDocuments<T>(
        this IFactory factory,
        T document,
        Action<T>? update = null)
        where T : class, IDockable
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(document.Id))
        {
            (document as IDisposable)?.Dispose();
            throw new InvalidOperationException($"{document.GetType().Name} must set Id before it is opened.");
        }

        if (factory.FindDockableById(document.Id) is { } existing)
        {
            if (existing is not T dockable)
            {
                (document as IDisposable)?.Dispose();
                throw new InvalidOperationException(
                    $"Document '{document.Id}' is already open as {existing.GetType().Name}.");
            }

            try
            {
                update?.Invoke(dockable);
                ActivateInOwner(factory, dockable);
                return dockable;
            }
            finally
            {
                if (!ReferenceEquals(document, dockable))
                {
                    (document as IDisposable)?.Dispose();
                }
            }
        }

        try
        {
            update?.Invoke(document);
            factory.AddToDocuments(document);
            return document;
        }
        catch
        {
            (document as IDisposable)?.Dispose();
            throw;
        }
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
        if (previewPinned || pinnedDoc is null)
        {
            factory.SetActiveDockable(vm);
            factory.SetFocusedDockable(rightDock, vm);
        }

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

    /// <summary>
    /// Replaces a docked (not auto-hidden) <paramref name="existing"/> with <paramref name="replacement"/> in the same dock and position,
    /// keeping it the active tab if it was, without moving keyboard focus. The replaced dockable is disposed.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="existing">The docked dockable to replace.</param>
    /// <param name="replacement">The dockable to show instead.</param>
    /// <returns><see langword="true"/> when the replacement was docked; <see langword="false"/> when <paramref name="existing"/> has no owning dock.</returns>
    public static bool ReplaceDockable(this IFactory factory, IDockable existing, IDockable replacement)
    {
        if (existing.Owner is not IDock owner || owner.VisibleDockables is not { } visible)
        {
            return false;
        }

        var index = visible.IndexOf(existing);
        var wasActive = ReferenceEquals(owner.ActiveDockable, existing);
        factory.RemoveDockable(existing, false);
        factory.InsertDockable(owner, replacement, Math.Max(0, Math.Min(index, owner.VisibleDockables?.Count ?? 0)));
        if (wasActive)
        {
            factory.SetActiveDockable(replacement);
        }

        DisposeReplaced(existing, replacement);
        return true;
    }

    private static void DisposeReplaced(IDockable replaced, IDockable replacement)
    {
        if (!ReferenceEquals(replaced, replacement) && replaced is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static void ActivateInOwner(IFactory factory, IDockable dockable)
    {
        factory.SetActiveDockable(dockable);

        if (dockable.Owner is IDock owner)
        {
            factory.SetFocusedDockable(owner, dockable);
        }
    }
}
