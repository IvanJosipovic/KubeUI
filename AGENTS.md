# Engineering Guide (AGENTS)

This document defines the architectural and coding rules for the project. It is
authoritative for all new code and refactors.

## 0) Pre-requisites
.Net Core SDK installed based on global.json version
Docker

## 1) Core principles (non-negotiable)

### SOLID (strict)
- Single Responsibility: every class has exactly one reason to change.
- Open/Closed: extend behavior via composition and interfaces; avoid modifying
  stable code paths when adding features.
- Liskov Substitution: derived types must be safely substitutable without
  altering expected behavior or contract.
- Interface Segregation: prefer small, focused interfaces; avoid "god" interfaces.
- Dependency Inversion: depend on abstractions; wire concrete types in the
  composition root only.

### MVVM (strict)
- Views are passive. No UI logic in code-behind beyond `InitializeComponent()`.
- All inputs are routed to ViewModels via bindings, commands, and behaviors.
- ViewModels are UI-framework agnostic and unit-testable.
- Models and services contain business logic and data access; ViewModels orchestrate
  them via DI.
- Prefer composition in ViewModels/services/code over inheritance wherever possible,
  except where framework base types are required (e.g., `ViewModelBase` for ViewModels).

## 2) Architecture

### Layering
- KubeUI.Kubernetes: all non-UI Kubernetes/runtime/shared contracts
- KubeUI.Avalonia: all Avalonia UI, view models, resource configs, shell glue
- KubeUI.Desktop: executable host and composition root

### Boundaries
- KubeUI.Desktop depends on KubeUI.Avalonia depends on KubeUI.Kubernetes

## 3) Avalonia UI best practices (aligned with Avalonia codebase)

References:
- https://github.com/AvaloniaUI/Avalonia
- https://docs.avaloniaui.net

### Views and styling
- Use XAML for layout and visuals; avoid creating controls in code.
- Define styles and resources in dedicated resource dictionaries and merge them
  in `App.axaml` to keep styling consistent and maintainable.
- Prefer `StaticResource` for immutable resources and `DynamicResource` when
  runtime updates are required.

### Data binding
- Use compiled bindings only (no reflection bindings) with explicit `x:DataType` on
  all binding scopes (views, DataTemplates, control themes, and resources).
- Keep bindings one-way unless user input must update the ViewModel.
- Use `DataTemplates` or a `ViewLocator` (custom, non-reflection) for view lookup.

### Custom controls
- Use `StyledProperty` only for values that must participate in styling.
- Prefer `DirectProperty` for non-styled properties to avoid extra overhead.
- For best UI/UX, prefer custom control creation or re-templating using control themes
  instead of CRUD-style UI.

## 4) ViewModel base
- All ViewModels inherit from `ViewModelBase`.

## 5) Input and interaction via Xaml.Behaviors (required)

Reference: https://github.com/wieslawsoltes/Xaml.Behaviors

- All UI input and events are handled via behaviors/triggers.
- Prefer source-generator-based behaviors/actions (no reflection) wherever available.
- Use trigger behaviors (property, data, loaded/unloaded, routed event) for
  lifecycles and state transitions.
- Code-behind must not contain event handlers or direct ViewModel calls.

## 6) Docking layout with Dock for Avalonia (required)

Reference: https://github.com/wieslawsoltes/Dock

- Use Dock.Model.* to represent the docking layout state.
- Use Dock.Avalonia for the view layer and Dock.Model.MVVM
  integration.
- Persist layout state to user settings and restore on startup.
- Keep layout logic in ViewModels; Views only render the Dock model.

## 7) Text editing with AvaloniaEdit (required)

Reference: https://github.com/AvaloniaUI/AvaloniaEdit

- Use AvaloniaEdit `TextEditor` for all code/text editing surfaces.
- Enable syntax highlighting using TextMate grammars/themes.
- Keep editor configuration in ViewModels (options, text, selection) and bind to
  the view.

## 8) Data presentation with ProDataGrid (required)

Reference: https://github.com/wieslawsoltes/ProDataGrid

- Use ProDataGrid `DataGrid` for all tabular data, tree views, and list displays.
- Always use the ProDataGrid model approach with code-based column bindings and
  fast paths.
- Always enable full filtering, searching, and sorting support.

## 9) Dependency Injection (Microsoft.Extensions.DependencyInjection)

- Configure services in a single composition root (App startup).
- Use `AddSingleton`, `AddScoped`, `AddTransient` correctly:
  - Singleton: thread-safe, shared, expensive-to-create services.
  - Scoped: per-document or per-operation services created within explicit scopes.
  - Transient: stateless lightweight services.
- Never resolve scoped services from singletons without creating a scope.
- Do not dispose services resolved from the container manually.

## 10) Performance (required)

- Prefer allocation-free APIs: `Span<T>`, `ReadOnlySpan<T>`, `Memory<T>`,
  `ValueTask`, `ArrayPool<T>`, and `System.Buffers`.
- Use SIMD (`System.Numerics.Vector<T>` or hardware intrinsics) where it provides
  measurable wins and keeps code maintainable.
- Avoid LINQ in hot paths; use loops and pre-sized collections.
- Minimize boxing, virtual dispatch in tight loops, and avoid unnecessary
  allocations in render/update loops.
- Profile before and after optimizations; document expected gains.

## 11) Reflection and source generation (required)

- Avoid reflection whenever possible.
- Prefer source generators (incremental generators required) before any reflection-based
  approach.
- If reflection is the only viable option, ask the user explicitly before introducing it.

## 12) Testing and validation

References:
- https://github.com/AvaloniaUI/Avalonia
- https://docs.avaloniaui.net/docs/testing/setting-up-the-headless-platform

- All production code must be covered by unit tests; xUnit is required for unit testing.
- UI tests must use Avalonia Headless (xUnit integration) and follow the headless testing guidance and helpers for input simulation.
- Unit-test ViewModels and Domain services.
- Use integration tests for parsing, IO, and docking layout persistence.
- UI tests should validate navigation flows, docking, and editor behaviors.

## 13) Code conventions

- No code-behind event handlers.
- Avoid static state (except truly immutable constants).
- Prefer explicit types where clarity is improved; avoid `var` in public APIs.
- All public APIs must be documented and unit-tested.
- No, 'hacks' or weird workarounds, if you think you have to, ask for guidance
- UI Text should be added to resources.