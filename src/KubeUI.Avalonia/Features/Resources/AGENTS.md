# Cross-Resource Screens

## Ownership
- These folders own screens that work across many Kubernetes resource kinds: list, yaml, properties, and visualization.
- Resource-kind-specific presentation belongs under `Resources/<Category>/<Version>/<Kind>/`.

## Constraints
- Keep resource-kind logic out of shared cross-resource views unless it is truly generic.
