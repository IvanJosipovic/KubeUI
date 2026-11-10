# Cross-Resource Screens

## Ownership
- These folders own screens that work across many Kubernetes resource kinds: list, yaml, properties, and visualization.
- Resource-kind-specific presentation belongs under `ResourceConfigs/<Category>/<Version>/<Kind>/` in this project.
- `Properties/ResourcePropertiesSection` is shared by resource-kind views to keep expandable property-section layout consistent; labels, values, and section contents remain with each kind.
- `Styles/ResourceFeatureStyles` owns Kubernetes status and resource-relationship brushes in light/dark theme dictionaries; App composes the provider but does not define those feature tokens.

## Constraints
- Keep resource-kind logic out of shared cross-resource views unless it is truly generic.
- The resource-menu Edit action opens the selected resource in YAML edit mode and requires Update permission.
