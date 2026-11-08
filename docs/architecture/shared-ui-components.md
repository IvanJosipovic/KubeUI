# Shared UI component ownership

Use this inventory when adding or reusing UI. Reuse the narrowest owner that contains the behavior:

1. Avalonia or a third-party library for generic framework behavior.
2. `KubeUI.Controls` for reusable presentation behavior consumed across projects.
3. A feature project for behavior shared inside that feature, including across its resource-kind roots.
4. A specific feature folder for behavior with one domain owner.

Similar appearance is not sufficient evidence to promote a component. A shared component must have at least two real consumers, a stable domain-neutral API, and no dependency on App startup, Kubernetes services, or feature-specific types. Keep labels, commands, permissions, data shaping, and workflow policy with their owning feature.

## Current shared components

| Owner | Component | Reuse boundary |
|---|---|---|
| `KubeUI.Controls` | `Layout/CompactToolbar` | Compact header layout and common toolbar group sizing; callers own content and commands. |
| `KubeUI.Controls` | `Presentation/ExpandableSection` | Compact Expander presentation used by resource properties and Settings; callers own header, content, and expansion state. |
| `KubeUI.Controls` | `Presentation/MenuPresenter` | Generic rendering of menu item view models; callers compose domain actions and permissions. |
| `KubeUI.Controls` | `Presentation/RefreshingCellTextBlock`, `RelativeAgeFormatter` | Refresh-clock lifecycle and generic relative-time formatting; callers extract domain timestamps and status. |
| `KubeUI.Controls` | `Presentation/TableStateRestoreCoordinator<TState>` | Deferred table-state restoration and stale-view guards; callers own snapshots and selection semantics. |
| `KubeUI.Controls` | YAML editor behaviors and folding strategy | Editor lifecycle, syntax highlighting, and syntax-agnostic indentation folding; Resources owns Kubernetes-specific editor behavior. |
| `KubeUI.Features.Resources` | `Properties/ResourcePropertiesSection` | Repeated expandable property-section composition across resource kinds; each kind owns its content and localized header. |

## Keep feature-local

- Resource-kind property layouts, columns, actions, and converters remain in their kind folders unless the same domain behavior is used by multiple kinds; then keep the helper inside Resources and document its consumers.
- Pod console, logs, metrics, and port-forward workflows remain in the Pod feature folder.
- Namespace scope selection remains in Clusters; Kubernetes graph and flow controls remain in Resources or Cilium because their graph data and interaction contracts differ.
- Feature toolbar content remains with each feature. Shared toolbar layout does not imply shared command composition.

## Promotion and optimization checklist

Before extracting or promoting a control:

- Name concrete consumers and the identical behavior they share.
- Separate presentation mechanics from feature policy and data transformation.
- Prefer composition over a wrapper that only renames an existing control.
- Keep public APIs documented and tested; use the owning feature's tests for feature-local components and Controls tests for cross-project controls.
- Preserve trimming, localization, accessibility, attach/detach cleanup, and UI-thread behavior at the new boundary.
- Update this inventory and the closest `AGENTS.md` when ownership changes.

Do not add a control solely to reduce repeated markup if doing so obscures resource-kind-specific intent. Extract only the repeated layout or behavior, leaving feature-owned values visible at the call site.
