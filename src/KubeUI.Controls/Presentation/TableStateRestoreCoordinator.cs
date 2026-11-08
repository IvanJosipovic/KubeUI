using Avalonia.Threading;

namespace KubeUI.Controls.Presentation;

/// <summary>Coordinates deferred table-state restoration across view attachment and data-context changes.</summary>
/// <typeparam name="TState">Opaque runtime state owned by the table implementation.</typeparam>
public sealed class TableStateRestoreCoordinator<TState>
    where TState : class
{
    private object? _restoredTable;
    private object? _restoredViewModel;

    /// <summary>
    /// Restores table state once per table/view-model pair, restores selection on each request, and
    /// ignores callbacks after either identity is stale.
    /// </summary>
    /// <param name="table">Current table instance.</param>
    /// <param name="viewModel">Current view-model instance.</param>
    /// <param name="state">Opaque state to restore; null skips table-state restore but still restores selection.</param>
    /// <param name="isCurrent">Checks attachment, data-context, and source identity at callback time.</param>
    /// <param name="restoreState">Consumer-owned table-state restoration callback.</param>
    /// <param name="restoreSelection">Consumer-owned selection restoration callback.</param>
    public void Restore(
        object table,
        object viewModel,
        TState? state,
        Func<bool> isCurrent,
        Action<TState> restoreState,
        Action restoreSelection)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(isCurrent);
        ArgumentNullException.ThrowIfNull(restoreState);
        ArgumentNullException.ThrowIfNull(restoreSelection);

        var shouldRestoreState = !ReferenceEquals(_restoredTable, table)
            || !ReferenceEquals(_restoredViewModel, viewModel);
        if (shouldRestoreState)
        {
            _restoredTable = table;
            _restoredViewModel = viewModel;
        }
        else
        {
            state = null;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!isCurrent())
            {
                return;
            }

            if (state is not null)
            {
                restoreState(state);
            }

            restoreSelection();
        }, DispatcherPriority.Background);
    }
}
