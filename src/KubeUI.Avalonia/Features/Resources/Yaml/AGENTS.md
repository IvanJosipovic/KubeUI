# Resource YAML

## Current Behavior
- YAML views serialize the current resource and can hide noisy metadata fields outside edit mode.
- Folding is generated for nested YAML structures.
- Fold state is preserved across dock activation changes.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Features/Resources/Yaml/`.
