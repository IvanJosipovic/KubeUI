# Resource-Kind Features

## Ownership
- Each Kubernetes resource kind is a feature root under `Resources/<Category>/<Version>/<Kind>/`.
- Resource-kind folders own their config plus any local views, controls, view models, converters, and helpers.
- Keep only generic resource infrastructure at `Resources/` root.

## Constraints
- Do not introduce category-level shared views or view models for behavior owned by one resource kind.
- If a helper is reused by multiple resource kinds, keep it shared and document why.
- Typed built-in resource configs derive from `ResourceConfig<T>` so initialization registers their model and typed seeder.
- Dynamically discovered CRDs use `CustomResourceConfig : ResourceConfig<GenericKubernetesObject>`; override typed model registration because CRD models are registered from live definitions. Kind origin is described by `ResourceOrigin` plus `CustomResourceDefinition`/`IsCustomResource`. `V1CustomResourceDefinitionConfig` remains a typed built-in config.
- Kind-specific initial templates return fresh typed resource objects; dynamically discovered CRDs may use YAML templates.
- Keep shared actions, columns, and permission behavior in `ResourceConfigBase<T>`. Express custom-resource identity through the computed `ResourceOrigin`/`IsCustomResource` contract; do not add a stored boolean discriminator to `IResourceConfig`.
- Configure built-in kinds directly on their `ResourceConfig<T>` subclasses through template, column, menu, and properties overrides.
- Configure special CRD kinds through `CustomResourceConfig` subclasses that override `ApiGroup`, `KindName`, and optional `ApiVersion`. The workspace supplies shared services when it initializes parameterless kind configs, selects by those identity values, and falls back to unscoped generic `CustomResourceConfig`. Avoid a separate kind customizer/definition registry.
- Keep canonical placement on `IResourceConfig.Navigation`; aliases and section documents live in `INavigationSectionProvider`.
- Keep built-in typed columns in `CreateDefaultColumns()` overrides.
