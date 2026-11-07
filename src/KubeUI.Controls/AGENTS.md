# KubeUI.Controls engineering guide

This project owns reusable Avalonia controls and UI contracts shared across features.

- Promote a feature control here only when another project needs the same behavior and API.
- Keep feature-specific controls in their owning `KubeUI.Features.*` project.
- Reference `KubeUI.Foundation` for the shared foundation. Do not reference feature projects or Kubernetes runtime projects.
- Keep external markup-extension generation declarations public and assign each external assembly one owner.
- Document and test public control APIs. Keep control tests under `tests/KubeUI.Avalonia.Tests/Controls/`.

## Shared presentation components

- `Layout/CompactToolbar` owns compact header layout only. Callers provide action, count, and trailing content; keep localized labels, commands, namespace policy, and table processing in the feature. Trailing content must remain reachable in constrained docks.
- `Presentation/ExpandableSection` owns the compact Expander resource overrides shared by resource property panels and Settings. Preserve the standard Expander style key and caller-controlled header/content/expansion.
- `Presentation/RefreshingCellTextBlock` owns refresh-clock subscription and text/tooltip updates for reusable cells. Detaching must unsubscribe; reattaching must resume exactly one subscription. Timestamp extraction, Pod status, and metric calculation stay in features.
- `Presentation/MenuPresenter` owns generic `MenuItemViewModel` rendering. Context-menu containers use compiled bindings for live model properties; flyouts are explicit snapshots recreated by their feature. Input gestures are display hints, never global shortcuts. Resource action/permission composition stays in Resources.
- `Presentation/RelativeAgeFormatter` owns generic relative-age and last-seen text formatting. Pass the current UTC time explicitly; Kubernetes Event timestamp precedence remains in Resources.
- `Behaviors/TreeViewItemContextMenuBehavior` owns generic TreeView row context-menu targeting and popup cleanup. Navigation actions/menus remain in App.
- `Editors/YamlIndentationFoldingStrategy` creates syntax-agnostic indentation folds. Resources owns Kubernetes-specific noisy-field identification and selection.
- `Editors/Behaviors/YamlFoldingBehavior` and `YamlSyntaxHighlightingBehavior` own generic editor attachment, TextMate setup, theme updates, fold updates, and cleanup. `Editors/YamlEditorDefaults` owns common TextEditor options only; callers retain text/document, font, readonly, wrap, sizing, and workflow policy.
- `Presentation/TableStateRestoreCoordinator<TState>` owns deferred restore scheduling and stale view/table guards only. Callers retain snapshot capture and domain-specific selection save/restore callbacks.
- `Styles/SharedControlStyles` is opt-in shared Ursa multi-select styling and toggle behavior. Hosts load it after the Ursa theme; it does not supply application palettes, feature resources, or require App startup.
- See `docs/architecture/shared-ui-components.md` for consumers and test ownership. Shared components must not depend on App startup or Kubernetes services.

## View-factory discovery

- `ViewFactoryDiscovery` (`DependencyInjection/ViewFactoryDiscovery.cs`) is the single shared `ScanForTypes` handler and registration helper for view-factory discovery. Every view-bearing feature points its `[ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(ViewFactoryDiscovery.Create))]` stub at it and registers the result with `AddViewFactories`; do not copy a local `CreateViewFactory` handler per feature.
- Views resolved through the generated view locator inherit `KubeUIView<TViewModel>`, which combines the declarative view base with `IViewFactoryDefinition` matching. Keep the polymorphic matching seam (view-model interfaces as `TViewModel`) working when changing the definition.
