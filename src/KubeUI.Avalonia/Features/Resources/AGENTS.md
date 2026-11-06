# Cross-Resource Screens

## Ownership
- These folders own screens that work across many Kubernetes resource kinds: list, yaml, properties, and visualization.
- Resource-kind-specific presentation belongs under `Resources/<Category>/<Version>/<Kind>/`.

## Constraints
- Keep resource-kind logic out of shared cross-resource views unless it is truly generic.

## List and editor shortcuts

- Resource list Enter invokes the default View action; Delete invokes the default Delete action. Both remain available with context menus closed, track current selection, and honor command eligibility.
- YAML editor shortcuts are scoped to the focused editor. Menu gesture labels are display-only; editor behavior must not be registered as a window hotkey.
