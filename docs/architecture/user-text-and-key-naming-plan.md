# User-facing text extraction and key naming consistency plan

**Status:** Implemented
**Decisions:** MCP `[McpServerTool]` attribute `Title`/`Description` stay English (compile-time attribute arguments; SDK registers tools by attribute scanning) — the only allowed hardcoded user-facing text besides `#if DEBUG` design-time strings and product names. The two runtime `McpToolAuditFilter` messages are extracted. App gets its own `Assets/Resources.json`; `{EnumTypeName}_{Value}` is a fourth allowed key pattern; the `{Kind}PropertiesView` prefix pattern is codified.

## As-built notes

- Phase 1: `KubeUI.App/Assets/Resources.json` (135 keys) created; 133 App-shell keys moved out of Foundation, which now holds only `Shared_` keys plus Foundation-owned keys. Foundation's 16 stale feature-view duplicates (`PodLogsView_*` etc.) were dead and were deleted rather than moved. App call sites read its keys through a new `L10n` alias (`KubeUI.App.Assets.LanguageManager`); tests use `AppL10n`.
- Phase 2 renames: `Cluster_Missing_Namespace_Permission_*` → `ClusterSettingsViewModel_Missing_Namespace_Permission_*`; `PodContainerCell_*` → `PodContainerCellView_*`; `PodStatusCell_*` → `PodStatusCellView_*`; `Hubble_*` → `HubbleNavigationSectionProvider_*`; `NavigationView_Cilium/Crossplane/Helm` → owning section-provider `SectionTitle` keys; `NavigationView_GatewayApi[Experimental]` → `GatewayApiCustomResourceConfig_{Gateway,Experimental}Group`.
- Phase 3 extractions: `ViewLocator_UnableToLoadView` (App), `Shared_NotAvailable` (Foundation), `ServiceMapGraphBuilder_{WorldNodeName,IdentityDisplayName}` (Cilium), `DockFactory_FloatingWindowTitle` (App), `McpToolAuditFilter_{OutputLimitMessage,RequestDeniedMessage}` (AI).
- The dead-key and owner-prefix checks found 29 additional dead keys (Cilium 14, Helm 6, AI 6, Clusters 2, Crossplane 1) and 18 mis-homed or mis-prefixed keys; all fixed.

## Background

The JSON resources migration (see [json-resources-migration-plan.md](json-resources-migration-plan.md)) moved all resources from `.resx` to per-project `Assets/Resources.json` behind Lingua managers. This plan covers what remains: user-facing text still hardcoded in source, and key names that do not follow the `{ViewOrControlName}_{Name}` / `{ResourceConfigName}_{Name}` / `Shared_{Name}` convention from the root AGENTS.md.

## Audit findings

### Remaining hardcoded user-facing text

| Location | Text | Notes |
|---|---|---|
| `src/KubeUI.App/Infrastructure/Presentation/ViewLocator.cs:45` | `Unable to load View for ViewModel: ...` | Shown in the UI as fallback |
| `src/KubeUI.App/Infrastructure/Mcp/McpTools.cs` | 12 `[McpServerTool(Title, Description)]` | Agent-facing; decided to extract |
| `src/KubeUI.Features.AI/Mcp/McpToolAuditFilter.cs` | 2 tool-output messages (`1 MiB limit`, `denied`) | Agent-facing; decided to extract |
| `src/KubeUI.Features.Resources/ResourceConfigs/Workloads/v1/Pod/PropertiesView.cs` | `"N/A"` fallback | User-facing empty-state placeholder |
| `src/KubeUI.Features.Cilium/Hubble/ServiceMapGraphBuilder.cs` | `"World"`, `"Identity {n}"` | Node display names |
| `src/KubeUI.App/Infrastructure/Docking/DockFactory.cs:254` | `$"KubeUI {n}"` floating window title | Brand + counter |

Out of scope (decided exclusions):

- `#if DEBUG` design-time literals (`ExpandableSection`, `HeaderItem`, `ClusterErrorView`) — never ship.
- Proper nouns (`GitHub Copilot`, `Claude Code` in `AcpAgentDefaults`) — product names, not translatable.
- Log and exception messages, DynamicTableView column IDs, document/dock IDs — not user-facing.
- DynamicTableView column headers, dialog strings, and other object-initializer `CurrentValue()` sites — already extracted; converting them to `ToBinding()` is the pre-existing Phase 4 reactive backlog of the JSON migration plan, not this plan.

### Key naming inconsistencies

1. **Cross-project leakage in `KubeUI.Foundation` JSON.** Foundation holds App-shell keys (`SettingsView_` 58, `MainView_` 19, `ClusterView_` 17, `NavigationView_` 12, `AboutView_` 10, `HomeView_` 2, `MainWindow_` 2) and feature-view keys (`PodLogsView_` 10, `ResourceYamlView_` 2, `ResourceEventsView_` 2, `VisualizationView_` 2). App has no own JSON; Features.Resources keys do not belong in Foundation.
2. **Dead keys.** `CloudClusterConnectView_Description/SubscriptionFirst/AksOnly` and `UpdateView_Title/Header` in Foundation have zero call sites.
3. **Prefix does not match class name.** `PodContainerCell_` / `PodStatusCell_` own classes `PodContainerCellView` / `PodStatusCellView`.
4. **`{Kind}PropertiesView` pattern** (~31 prefixes in Features.Resources, e.g. `PodPropertiesView_`) is owned by classes named `PropertiesView`, one per resource kind. Decided: codify as an allowed pattern.
5. **Enum value labels.** `FlowDirection_` and `FlowVerdict_` in Cilium have no owning class. Decided: add `{EnumTypeName}_{Value}` as a fourth allowed pattern.
6. **Non-owner prefixes.** `Hubble_` (section titles; owner is the Hubble navigation section provider class), `Cluster_Missing_Namespace_Permission_*` (used by App's `NavigationViewModel`, stored in the Clusters JSON).
7. **Contract test is loose.** `JsonResourcesContractTests` only enforces `^[A-Z][A-Za-z0-9]+(_[A-Za-z0-9]+)+$`; it does not enforce the owner-prefix rule.

## Phases

### Phase 1 — Re-home keys to owning projects

1. Create `src/KubeUI.App/Assets/Resources.json` + `Assets/LanguageManager.cs` (alias `L10n`; `SharedL10n` alias for Foundation stays).
2. Move App-shell keys (`SettingsView_`, `MainView_`, `ClusterView_`, `NavigationView_`, `AboutView_`, `HomeView_`, `MainWindow_`) from Foundation to App. Foundation keeps only `Shared_` and its own views.
3. Move `PodLogsView_`, `ResourceYamlView_`, `ResourceEventsView_`, `VisualizationView_` from Foundation to `KubeUI.Features.Resources`.
4. Add `InternalsVisibleTo` grants for the new App manager (tests, and any feature that reads it).
5. Update call-site aliases; compiler catches misses.
6. Update root AGENTS.md and the JSON migration doc.

### Phase 2 — Rename inconsistent prefixes

All key renames are internal API; the generated manager members and call sites rename together and the compiler verifies.

1. `Cluster_Missing_Namespace_Permission_*` → `NavigationView_Missing_Namespace_Permission_*` (App JSON after Phase 1).
2. `PodContainerCell_` → `PodContainerCellView_`; `PodStatusCell_` → `PodStatusCellView_`.
3. `Hubble_` → Hubble section-provider class prefix.
4. Delete the 5 dead keys.

### Phase 3 — Extract remaining hardcoded text

1. `ViewLocator` fallback → `Shared_` key in App JSON (App-only text; decide prefix per owner rule) or `Shared_ViewLocator_...`.
2. `"N/A"` → `Shared_NotAvailable` (Foundation, shared placeholder).
3. `"World"` / `"Identity {n}"` → Cilium JSON under the owning builder prefix.
4. `DockFactory` floating window title → format key in App JSON.
5. MCP text: `McpTools_*` keys in App JSON; `McpToolAuditFilter` messages → AI feature JSON (e.g. `McpToolAuditFilter_*`). Tool metadata reads `CurrentValue()` (structural, one-shot registration).

### Phase 4 — Enforce naming in the contract test

Extend `tests/KubeUI.Avalonia.Tests/Foundation/Localization/JsonResourcesContractTests.cs`:

1. First key segment must be `Shared`, or match an existing View/Control/ResourceConfig/enum type declared in the same project, or match the codified `{Kind}PropertiesView` pattern. Build the type-name set from `src/<project>/**/*.cs` declarations (same mechanism style as `ProjectBoundaryTests`).
2. Dead-key detection: every key must have at least one call site in `src/` or `tests/` (generated manager member names derive from keys; a source scan per key prefix is sufficient).
3. JSON placement check: a key's owner type must be declared in the project that owns the JSON file.

Update AGENTS.md: document `{EnumTypeName}_{Value}` and `{Kind}PropertiesView` patterns and the per-project key-homing rule.

### Phase 5 — Validation

1. `dotnet build --tl:off -clp:ErrorsOnly` (Debug and Release) — 0 errors.
2. Full test suite (Release, hangdump, matching CI workflow).
3. The extended contract test is the regression for this plan; extend it before the moves so each phase lands against a red/green check.
4. Update `docs/architecture/json-resources-migration-plan.md` status notes with the as-built state of this plan.

## Risk notes

- Phase 1 is the risky step (DI wiring, `InternalsVisibleTo`, alias churn). Key content stays byte-identical; only location changes.
- Lingua generates one manager member per key; renames in Phase 2 also rename generated properties — verify no external consumer (e.g. test data) references old member names.
- MCP metadata registered once at startup: `CurrentValue()` is a known non-reactive site per AGENTS.md; document it at the call site.
