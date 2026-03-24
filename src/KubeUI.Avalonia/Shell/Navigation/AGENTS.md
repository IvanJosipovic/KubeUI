# Navigation Feature

## Current Behavior
- The navigation tree reflects the current cluster catalog.
- Cluster resource nodes populate only after cluster connect and workspace initialization complete.
- Connection failures open or update a shared cluster error document.
- Missing Namespace Resource list/watch permission opens cluster settings and shows a prompt.
- Selecting a resource navigation link should keep the badge count visible, including `0` when the opened runtime list is empty.
- When building resource navigation items:
  - Require permissions to be loaded for the resource config.
  - Check whether the user has global list/watch permissions for that resource.
  - If not, for namespaced resources, require list/watch in the some known workspace namespace.
  - Known namespaces come from the runtime namespace list after workspace initialization materializes any settings-backed fallback namespaces.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/`.
- Add or update navigation tests when changing resource badge count behavior.
