# KubeUI.Controls engineering guide

This project owns reusable Avalonia controls and UI contracts shared across features.

- Promote a feature control here only when another project needs the same behavior and API.
- Keep feature-specific controls in their owning `KubeUI.Features.*` project.
- Reference `KubeUI.Avalonia` for the shared foundation. Do not reference feature projects or Kubernetes runtime projects.
- Keep external markup-extension generation declarations public and assign each external assembly one owner.
- Document and test public control APIs. Keep control tests under `tests/KubeUI.Avalonia.Tests/Controls/`.

## View-factory discovery

- `ViewFactoryDiscovery` (`DependencyInjection/ViewFactoryDiscovery.cs`) is the single shared `ScanForTypes` handler and registration helper for view-factory discovery. Every view-bearing feature points its `[ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(ViewFactoryDiscovery.Create))]` stub at it and registers the result with `AddViewFactories`; do not copy a local `CreateViewFactory` handler per feature.
- Views resolved through the generated view locator inherit `KubeUIView<TViewModel>`, which combines the declarative view base with `IViewFactoryDefinition` matching. Keep the polymorphic matching seam (view-model interfaces as `TViewModel`) working when changing the definition.
