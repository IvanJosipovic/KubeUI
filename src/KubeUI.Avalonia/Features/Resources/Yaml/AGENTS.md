# Resource YAML

## Current Behavior
- YAML views always serialize the complete current resource.
- The Hide Noisy Fields option folds `metadata.managedFields` and multiline `metadata.annotations.kubectl.kubernetes.io/last-applied-configuration` without removing data.
- Other annotations remain visible and are never hidden as a group.
- Folding is generated for nested YAML structures.
- Fold state is preserved across dock activation changes.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Features/Resources/Yaml/`.
- Keep action-result banners with balanced outer spacing and a clear bottom inset for wrapped messages.
- Show structured Kubernetes server validation causes at their YAML field values after dry-run or save; clear those diagnostics when the document changes.
- Keep the hover documentation popup closed while YAML completion and its documentation popup are open.
- Show hover documentation only when the pointer is over rendered YAML key text, never over blank space on a highlighted line.
- Use the editor code font family and size for completion entries, completion documentation, hover and diagnostic text, and YAML toolbar tooltips.
- Mark schema-required property suggestions and hover documentation labels with `*`; never include marker in inserted YAML.
