# Prompt
caveman mode

# Engineering Guide (AGENTS)

This document defines the repository-wide architectural and coding rules. For Avalonia app guidance, see `src/KubeUI.App/AGENTS.md`; feature-specific guidance lives with each feature.

## 0) Prerequisites
- Install the .NET SDK pinned by `global.json`.
- Docker is required only for Kind-backed tests.

## 1) Core principles

### SOLID
- Single Responsibility: every class has exactly one reason to change.
- Open/Closed: extend behavior via composition and interfaces; avoid modifying stable code paths when adding features.
- Liskov Substitution: derived types must be safely substitutable without altering expected behavior or contract.
- Interface Segregation: prefer small, focused interfaces; avoid "god" interfaces.
- Dependency Inversion: depend on abstractions; wire concrete types in the composition root only.

## 2) Architecture

### Layering
- KubeUI.Kubernetes: non-UI Kubernetes/runtime/shared contracts
- KubeUI.Foundation: Avalonia framework dependencies, generated extensions, shared assets and theme foundation
- KubeUI.Controls: reusable controls and shared UI contracts
- KubeUI.Features.*: feature runtime and feature UI
- KubeUI.App: main shell, cross-feature UI composition and application services
- KubeUI.Desktop: desktop executable and platform startup

### Boundaries
- KubeUI.Desktop depends on KubeUI.App
- KubeUI.App and KubeUI.Features.* depend on KubeUI.Controls
- KubeUI.Controls depends on KubeUI.Foundation
- Feature projects may depend on KubeUI.Kubernetes and acyclic feature capabilities

## 3) Dependency Injection

- Configure services in a single composition root (App startup).
- Use `AddSingleton`, `AddScoped`, `AddTransient` correctly.
- Never resolve scoped services from singletons without creating a scope.
- Do not dispose services resolved from the container manually.

## 4) Performance

- Prefer allocation-free APIs where practical.
- Avoid LINQ in hot paths; use loops and pre-sized collections.
- Minimize boxing, virtual dispatch in tight loops, and avoid unnecessary allocations in render/update loops.
- Profile before and after optimizations; document expected gains.

## 5) Reflection and source generation

- Never introduce reflection or reflection-dependent dynamic dispatch without asking the user and getting explicit approval first.
- This includes indirect reflection through third-party actions, string-named methods/events, runtime member lookup, `Activator`, and `MethodInfo.Invoke`.
- Prefer direct calls/delegates, interfaces, static registration, or source generation.
- Release builds use full trimming. A method invoked only through reflection may be removed even when Debug works; linker annotations or assembly roots do not replace user approval.

## 6) Testing and validation

- All production code must be covered by unit tests; xUnit is required for unit testing.
- Use integration tests for parsing, IO, and docking layout persistence.
- Avalonia UI tests and headless UI guidance live in `src/KubeUI.App/AGENTS.md`.
- For bugs, write the failing regression test first, confirm the failure reproduces, then implement the fix and rerun the same test before widening scope.
- Every requested new feature implies corresponding tests; implementation is incomplete until those tests are added or updated and pass.
- Tests should not use Task.Sleep or Delay but wait for specific scenarios or use events
- Before running tests run a dotnet build with --tl:off -clp:ErrorsOnly
- Each Test Method should dispose all objects created, there should never be shared state between tests

## 7) Code conventions

- Avoid static state except truly immutable constants.
- Prefer explicit types where clarity is improved; avoid `var` in public APIs.
- All public APIs must be documented and unit-tested.
- Resource keys in each project's `Assets/Resources.json` must use `{ViewOrControlName}_{Name}` for view/control-owned UI text, `{ResourceConfigName}_{Name}` for resource-list column headers and resource-config actions, `{EnumTypeName}_{Value}` for enum display labels, `<Kind>PropertiesView_{Name}` for per-kind generated property views (the view class is always `PropertiesView`), or `Shared_{Name}` for labels shared across multiple views, controls, or configs. The first key segment must be `Shared`, a type declared in the same project, or a documented pattern above, and the key must live in the project that declares its owner type — `JsonResourcesContractTests` enforces this and rejects unreferenced (dead) keys.
- `KubeUI.App` owns its shell UI text in its own `Assets/Resources.json` behind `KubeUI.App.Assets.LanguageManager` (alias `AppL10n` in test projects). Exception: `[McpServerTool]` `Title`/`Description` attribute literals stay English because attribute arguments are compile-time constants and the SDK registers tools by attribute scanning.
- Bind localized text on live controls with `L10n.Instance.Key.ToBinding()` (aliases: `SharedL10n` for `KubeUI.Foundation.Assets.LanguageManager`, `ResourceL10n` for the Resources feature). Use `CurrentValue()` only for structural strings, one-shot messages, comparisons, and URIs; every such call is a known non-reactive site.
- No hacks or weird workarounds; if you think you need one, ask for guidance.
- When a new feature is added, update the closest Agents.md with the feature requirements
- Use `System.Threading.Lock` for mutual exclusion. As a project convention, do not reacquire a lock already held by the current thread, even though the lock is reentrant. Never await while holding a lock; release it before any `await` that could re-enter guarded code.

## 8) Trimmed Release publishing
- Desktop Release publishes use full trimming and must retain visible linker diagnostics.
- Fix KubeUI-owned trim warnings with static registration, source generation, or correct annotations.
- Keep a third-party assembly whole only when its used runtime feature depends on reflection or dynamic loading and a focused test covers that feature.
- Release trim analysis is enabled for KubeUI libraries; analyzer warnings fail Release builds for KubeUI.Features.AI and KubeUI.Kubernetes, and IL2091 fails KubeUI.Foundation builds.
- Avalonia reflection warnings remain visible for features whose assemblies are rooted by Desktop; review new diagnostics and record the specific fix or assembly keep.
- Do not suppress trim warnings project-wide.

## 9) Documentation website
- The Docusaurus site lives in `website/`; keep app screenshots and source assets in `docs/` and copy only site assets into `website/static/`.
- Pin Docusaurus packages to one version and commit `website/package-lock.json`.
- Production URL is `https://KubeUI.com` with root `baseUrl: '/'`; `website/static/CNAME` must contain `KubeUI.com`.
- `.github/workflows/cicd.yml` publishes the site to GitHub Pages only after a successful semantic release.
- Validate site changes with `npm ci` and `npm run build` from `website/`.
