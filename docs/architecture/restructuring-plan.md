# KubeUI Restructuring Plan

**Status:** **Phase 1 Complete; Phase 2 Active; Phase 3 Split Complete** — based on architectural review of current state vs. `avalonia-project-structure-plan.md`

---

## Executive Summary

The project structure largely matches the target architecture, but **9 structural issues** compromise encapsulation, test isolation, and maintainability. This plan addresses them in priority order.

---

## Issue Inventory

| ID | Title | Severity | Category | Status |
|----|-------|----------|----------|--------|
| R-01 | Feature tests share one test project | High | Test Architecture | ✅ Split into feature test projects |
| R-02 | App.csproj leaks 30+ feature-internal namespaces via `Using` | High | Encapsulation | ✅ Done |
| R-03 | Cross-feature `Using` creates compile-time coupling | High | Dependency Hygiene | 🟡 In Progress |
| R-04 | Desktop references Kubernetes directly (bypasses App) | Medium | Layering | ✅ Done |
| R-05 | Helm.NET package in Kubernetes (shared runtime) | Medium | Layering | ✅ Done |
| R-06 | No central package version management | Medium | Build Hygiene | ✅ Done |
| R-07 | Feature projects export implementation namespaces | Medium | Encapsulation | ✅ Done |
| R-08 | App owns feature-specific packages | Medium | Ownership | 🟡 In Progress |
| R-09 | Test project declares AvaloniaResources | Low | Test Hygiene | ✅ Done |

---

## Detailed Remediation Plan

### R-01: Reduce Test Project Coupling

**Problem:** `KubeUI.Avalonia.Tests` references all 11 source projects + `KubeUI.Testing` + `KubeUI.Desktop`. This increases rebuild scope and exposes unrelated internals to one test assembly.

**Decision:** Split feature tests into projects matching `src/KubeUI.Features.*`, while keeping cross-feature and App-shell tests in `KubeUI.Avalonia.Tests`. Share common build and test setup; avoid copying test infrastructure.

This can cause:
- Full rebuild on any change
- Tests can reach internals of any project
- No isolation for feature test runs

**Implementation:** Move feature tests into matching test projects. Keep reusable headless and Kubernetes fixtures in shared test support. Keep App-shell and cross-feature integration tests in `KubeUI.Avalonia.Tests`.

**Status:** Split complete; local test execution remains blocked before test discovery by Microsoft.Testing.Extensions.HangDump startup timeout.

Seven feature test projects now cover AI, Cilium, Clusters, Crossplane, Gateway API, Helm, and Resources. Shared headless/Kubernetes setup is imported from `tests/FeatureTests.props`; App-shell, Controls, Desktop startup, and cross-feature tests remain in `KubeUI.Avalonia.Tests`. All new projects are included in `KubeUI.slnx` and CI's solution-level test run.

Removed seven direct feature project references and two feature-only package references from `KubeUI.Avalonia.Tests.csproj`; those feature tests now compile in their matching projects. The remaining test project references are App, Controls, Desktop, Kubernetes, and shared `KubeUI.Testing`.

---

### R-02: Remove Feature-Internal `Using` from KubeUI.App.csproj

**Problem:** App.csproj has 30+ `Using` statements exposing feature internals:
```xml
<Using Include="KubeUI.Features.Clusters.Assets" Alias="ClusterAssets" />
<Using Include="KubeUI.Features.Resources.Infrastructure" />
<Using Include="KubeUI.Features.Resources.Settings" />
<Using Include="KubeUI.Features.Resources.Icons" />
<Using Include="KubeUI.Features.Resources.Converters" />
```

**Target State:** App references only **public contracts** from features:
- Feature service registration extensions (e.g., `KubeUIResourcesFeatureServiceCollectionExtensions`)
- Navigation section providers (public interfaces)
- Public view model / view base types

**Implementation:**
1. Audit each `Using` in App.csproj — classify as:
   - ✅ Public contract (keep)
   - ❌ Internal implementation (remove)
2. For each removed `Using`, identify the public API needed and:
   - Either: add to feature's public surface (interface/abstract class)
   - Or: move to `KubeUI.Controls` if truly shared
3. Update App code to use public APIs only
4. Add `InternalsVisibleTo` only for test projects, not App

**Progress:**
- ✅ App.csproj now has only `ClusterAssets` and `ResourceAssets` feature aliases; both are used by App navigation.
- ✅ Audited App source imports; App has no project-wide feature implementation imports.

**Effort:** Low (half day)
**Risk:** Low — mechanical refactoring with compiler guidance

---

### R-03: Replace Cross-Feature `Using` with Narrow Public Contracts 🟡 In Progress

**Problem:** Features import other features' implementation namespaces:

| Consumer | Imports (Internal) | Should Use |
|----------|-------------------|------------|
| Cilium | `Resources.Visualization`, `.Icons`, `.Properties.Controls`, `.Common`; cluster lifecycle contract | Icon contract; shared controls; graph-style ownership decision |
| Resources | `Clusters.Workspace`, `.Presentation` | Cluster workspace/runtime operations and navigation composition; scope requires consumer-level contract design |
| Crossplane | Cluster resource-config contract | Already consumes `IClusterResourceConfig`; no Resources registry contract needed |
| Helm | Resources visualization contract and `IResourceConfig` navigation signature | Visualization contract exists; navigation contract ownership remains coupled to Resources |

**Target State:** Each feature exposes a **single public contract assembly** (or namespace) containing only:
- Service registration extensions
- Public interfaces for cross-feature consumption
- Navigation section providers
- Public view models / views needed for composition

**Implementation:**
1. For each feature, create `Contracts/` or `Public/` folder with public interfaces
2. Move cross-feature dependencies to reference contracts only
3. Update `Using` statements in csprojs to reference only contract namespaces
4. Use `InternalsVisibleTo` for test projects only

**Progress:**
- ✅ Moved `IResourceIconService` into `KubeUI.Features.Resources.Contracts`; Cilium now consumes the contract namespace.
- ✅ Added `IResourceVisualizationService`; Helm now uses it instead of constructing the Resources visualization ViewModel and Dock document.
- ✅ Mapped Cilium's remaining imports to concrete use: graph styling, icon rendering, generic property rows, shared context-menu items, and cluster initialization/runtime access. Removed an unused `Resources.Common` import from `ServiceMapView`.
- ✅ Moved generic `PropertyItem` into `KubeUI.Controls.Presentation`; Resources and Cilium now share the control without a feature-to-feature UI dependency.
- ⚠️ Do not remove remaining Cilium imports mechanically. `MenuItemViewModel` is a reusable presentation primitive; graph styles are coupled to the Resources GraphX model and need an explicit shared graph owner first.
- ⏳ Remaining: design cluster/workspace contracts around consumer operations rather than passing `ClusterWorkspace`; keep `IClusterResourceConfig` as the existing cluster-owned registry contract; separate Helm's navigation-provider signature from Resources if the dependency can then be removed; move `MenuItemViewModel` only if Controls becomes its actual shared owner; decide graph-style ownership.
- ⏳ R-03 stays open. Contract work is incomplete until project references and implementation imports are removed or documented as intentional, with focused tests.

**Effort:** Medium (1-2 days per feature pair)
**Risk:** Medium — requires API design for contracts

---

### R-04: Remove Kubernetes Reference from KubeUI.Desktop ✅ Done

**Problem:** `KubeUI.Desktop.csproj` references `KubeUI.Kubernetes` directly:
```xml
<ProjectReference Include="..\KubeUI.Kubernetes\KubeUI.Kubernetes.csproj" />
```

**Target State:** Desktop references only `KubeUI.App` (and platform packages).

**Implementation:**
1. Moved `ConfigureKubeUIKubernetesJsonLogging()` to `KubeUI.App.Infrastructure.DependencyInjection.KubeUIAvaloniaServiceCollectionExtensions`.
2. Exposed the Kubernetes activity source name through the App composition API; Desktop no longer imports Kubernetes runtime types.
3. Removed the direct `KubeUI.Kubernetes` project reference from Desktop.
4. Verified with `dotnet build --tl:off -clp:ErrorsOnly`.

**Effort:** Low (few hours)
**Risk:** Low

---

### R-05: Move Helm.NET from Kubernetes to Helm Feature ✅ Done

**Problem:** `KubeUI.Kubernetes.csproj` references `Helm.NET` — a feature-specific package.

**Target State:** Helm.NET only in `KubeUI.Features.Helm.csproj`.

**Implementation:**
1. Moved Helm types from `KubeUI.Kubernetes/Helm/` to `KubeUI.Features.Helm/Helm/`:
   - `IHelmService`, `HelmService`
   - `IHelmClientFactory`, `HelmClientFactory`
   - `HelmConnection`
2. Updated `KubeUI.Features.Helm.DependencyInjection.KubeUIHelmFeatureServiceCollectionExtensions` to register Helm services
3. Removed `Helm.NET` package reference from `KubeUI.Kubernetes.csproj`
4. Updated all test projects (`KubeUI.Kubernetes.Tests`, `KubeUI.Avalonia.Tests`, `KubeUI.Documentation.Tests`) to reference new namespace `KubeUI.Features.Helm.Helm`

**Effort:** Low (half day)
**Risk:** Low — Helm types already feature-specific

---

### R-06: Centralize Package Versions with Directory.Packages.props ✅ Done

**Problem:** 15+ projects independently declare same package versions:
- `CommunityToolkit.Mvvm 8.4.2` — 8 projects
- `Avalonia.Markup.Declarative 12.2.0` — 10 projects
- `FluentAvaloniaUI 3.1.0` — 5 projects
- `DynamicData 9.5.0` — 4 projects
- `System.Reactive 6.1.0` — 3 projects

**Target State:** Single source of truth in `Directory.Packages.props` at repo root.

**Implementation:**
1. Created `/home/coder/projects/KubeUI/Directory.Packages.props` with 65+ `PackageVersion` entries
2. Removed `<Version>` from all `PackageReference` elements in 16 csproj files
3. Added missing packages: `ServiceScan.SourceGenerator`, `BenchmarkDotNet`, `Moq`

**Effort:** Low (1-2 hours)
**Risk:** Low — standard MSBuild feature

---

### R-07: Restrict Feature Project `Using` Exports to Public Surface Only 🟡 In Progress

**Problem:** Feature csprojs export implementation namespaces via global `Using`:
- Resources exports: `.Properties.Controls`, `.Infrastructure`, `.Settings`, `.Converters`, `.Icons`, `.Visualization`
- Clusters exports: `.Workspace`, `.Presentation`
- Cilium exports: `.Assets` (OK), but also internal Hubble types

**Target State:** Each feature's global `Using` includes only:
- Its own public contract namespace(s)
- Shared foundation namespaces (`KubeUI.Controls.*`, `KubeUI.Avalonia.*`)
- No implementation namespaces

**Implementation:**
1. Audit each feature csproj's `<Using>` ItemGroup
2. Move implementation namespaces to file-scoped `using` in source files
3. Keep only public contract namespaces in global `Using`
4. Add `Internal` sub-namespaces for implementation (e.g., `KubeUI.Features.Resources.Internal.*`)

**Progress:**
- ✅ Removed implementation global usings from Resources, Cilium, Crossplane, and Helm; added explicit imports where needed.
- ✅ Audited remaining feature globals: only own feature namespaces, resource contracts, and asset aliases remain.

**Effort:** Low-Medium (mechanical, compiler-guided)
**Risk:** Low

---

### R-08: Move Feature-Specific Packages from App to Features

**Problem:** `KubeUI.App.csproj` owns packages used by specific features:
| Package | Likely Owner |
|---------|--------------|
| `Westermo.GraphX.*` | Resources, Cilium, Helm |
| `SvcSystems.UI.Terminal` | AI, Resources |
| `LiveChartsCore.SkiaSharpView.Avalonia` | Resources, Helm |
| `Irihi.Ursa*` | Resources, Helm |
| `Svg.Controls.Skia.Avalonia` | Resources, Controls? |
| `Xaml.Behaviors.Avalonia` | Resources, Helm, AI |

**Target State:** Feature-only packages live with their owning features. App keeps direct package references for APIs used by App-owned shell, startup, infrastructure, and cluster-overview code.

**Implementation:**
1. Audit each package in App.csproj — determine actual usage (grep for types)
2. Move to owning feature csproj(s)
3. If multiple features use it, consider `KubeUI.Controls` or accept duplication (small packages)
4. Remove from App

**Progress:**
- ✅ Removed App references to `Irihi.Ursa`, `Irihi.Ursa.Themes.Semi`, and `Xaml.Behaviors.Avalonia`; owning features reference those packages.
- ✅ Audited every remaining `PackageReference` against App source. Retained each: App uses its APIs or analyzer/source-generator functionality, including SVG application icons, LiveCharts cluster overview, and Fluent theme integration.
- ✅ No unused package references remain to remove from `KubeUI.App.csproj`.

**Effort:** Medium (half day + verification)
**Risk:** Low — build will catch missing references

---

### R-09: Remove AvaloniaResources from Test Project ✅ Done

**Problem:** `KubeUI.Avalonia.Tests.csproj` declares:
```xml
<AvaloniaResource Include="Assets\**" />
```

**Target State:** Test project has no AvaloniaResources unless tests specifically test resource loading.

**Implementation:**
1. Verified no `Assets/` folder exists in test project
2. Removed `<AvaloniaResource Include="Assets\**" />` from `KubeUI.Avalonia.Tests.csproj`

**Effort:** Trivial
**Risk:** None

---

## Execution Order & Dependencies

```
Phase 1 (Foundation - no deps): ✅ COMPLETE
  ├─ R-06: Directory.Packages.props ✅
  ├─ R-09: Test Assets cleanup ✅
  ├─ R-04: Desktop → Kubernetes reference ✅ (telemetry source name exposed by App)
  └─ R-05: Helm.NET move ✅

Phase 2 (Encapsulation - depends on Phase 1): 🟡 IN PROGRESS
  ├─ R-02: App.csproj Using cleanup ✅
  ├─ R-07: Feature Using exports ✅
  ├─ R-08: App package ownership 🟡
  └─ R-03: Cross-feature contracts 🟡 (icon and visualization contracts moved; cluster/resource boundaries remain)

Phase 3 (Test project split):
  └─ R-01: Feature test projects ✅ (build passes; local test host times out before discovery)
```

---

## Validation Checklist

After each phase, verify:
- [ ] `dotnet build --tl:off -clp:ErrorsOnly` passes
- [ ] `dotnet test --no-build` passes (for affected tests)
- [ ] No `CS0103` / `CS0246` errors from removed `Using`
- [ ] Trim analysis: `dotnet publish -c Release -r linux-x64 --self-contained` shows no new warnings
- [ ] Dependency graph matches `avalonia-project-structure-plan.md` Mermaid diagram

## AGENTS.md Guidance Review

- Reviewed all 39 repository `AGENTS.md` files for stale paths, versions, conflicts, truncation, and duplicated rules.
- Corrected confirmed issues: Helm implementation path and alpha version, shared graph-content namespace, incomplete test-infrastructure instruction, outdated SDK prerequisite wording, and test-assembly split conflict.
- Removed duplicated App/Settings and Avalonia-test guidance where a narrower owner document already covers it.
- The detailed current-behavior inventories remain useful regression guards, but their individual claims still need source/test verification when those features next change; do not treat this text review as full behavioral validation.

---

## Rollback Plan

Each phase is a independent commit/PR. Revert by:
```bash
git revert <commit-hash>
```

No phase modifies runtime behavior — only project structure and visibility.

---

## Related Documents

- `docs/architecture/avalonia-project-structure-plan.md` — Target architecture
- `src/KubeUI.App/AGENTS.md` — App engineering guide
- `src/KubeUI.Features.*/AGENTS.md` — Feature guides
- `tests/KubeUI.Avalonia.Tests/AGENTS.md` — Test guide
