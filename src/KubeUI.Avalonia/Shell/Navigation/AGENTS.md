# Navigation Feature

## Current Behavior
- The navigation tree reflects the current cluster catalog.
- Cluster resource nodes populate only after cluster connect and workspace initialization complete.
- Connection failures open or update a shared cluster error document.
- Missing namespace list permission opens cluster settings and shows a prompt.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/`.
