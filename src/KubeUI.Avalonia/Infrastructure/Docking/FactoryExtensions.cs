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

    private static void ActivateInOwner(IFactory factory, IDockable dockable)
    {
        factory.SetActiveDockable(dockable);

        if (dockable.Owner is IDock owner)
        {
            factory.SetFocusedDockable(owner, dockable);
        }
    }
}
