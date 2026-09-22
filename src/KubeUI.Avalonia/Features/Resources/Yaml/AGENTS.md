# Resource YAML

## Current Behavior
- YAML views always serialize the complete current resource.
- The Hide Noisy Fields option folds `metadata.managedFields` and multiline `metadata.annotations.kubectl.kubernetes.io/last-applied-configuration` without removing data.
- Other annotations remain visible and are never hidden as a group.
- Folding is generated for nested YAML structures.
- Fold state is preserved across dock activation changes.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Features/Resources/Yaml/`.
