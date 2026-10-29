# Cluster Catalog

## Current Behavior

- The catalog screen shows the current cluster workspace collection through `SvcSystems.Avalonia.DynamicTableView`.
- Selection is single-row; the reusable row-selection, sorting, column-width, and context-menu targeting behavior belongs to the library.
- Cluster deletion is confirmed before removal.
- Each row has an `Enabled` checkbox and an editable `Display Name` (alias). Both are stored per cluster in `ClusterSettings` (keyed by kubeconfig path + context) by `ClusterWorkspacePreferences`; the kubeconfig file is never modified. Disabled clusters stay listed here but are hidden from navigation, and disabling a connected cluster disconnects it.
- The toolbar provides `Add Kubeconfig` (file picker, any location; non-kubeconfig files show a warning notification) and `Remove Kubeconfig` (flyout of manually added paths, excluding the default kubeconfig; confirmed; only stops tracking — the file is not deleted). Auto-discovery is controlled in App Settings.
- Manual kubeconfig selection accepts hidden files; automatic discovery skips hidden files. Discovered context names must not duplicate existing cluster names.
