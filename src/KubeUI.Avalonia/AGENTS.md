# Engineering Guide (AGENTS) - KubeUI.Avalonia

This guide supplements the repository root `AGENTS.md` and defines the Avalonia-specific rules for `src/KubeUI.Avalonia`.

## 1) Folder structure

- `Features/<FeatureName>/` and `Shell/<Area>/` must each follow the same internal structure when they contain UI code:
  - `Views/` for XAML views and their code-behind
  - `ViewModels/` for view models and supporting presentation models
  - `Controls/` for reusable controls scoped to that feature or shell area
  - `Behaviors/` for behaviors scoped to that feature or shell area
- Keep shared, cross-feature UI helpers out of feature folders only when they are truly shared across multiple areas.

## 2) Avalonia UI best practices

References:
- https://github.com/AvaloniaUI/Avalonia
- https://docs.avaloniaui.net

### Views and styling
- Use XAML for layout and visuals; avoid creating controls in code.
- Define styles and resources in dedicated resource dictionaries and merge them in `App.axaml`.
- Prefer `StaticResource` for immutable resources and `DynamicResource` when runtime updates are required.

### Data binding
- Use compiled bindings only with explicit `x:DataType` on all binding scopes.
- Keep bindings one-way unless user input must update the ViewModel.
- Use `DataTemplates` or a custom `ViewLocator` for view lookup.

### Custom controls
- Use `StyledProperty` only for values that must participate in styling.
- Prefer `DirectProperty` for non-styled properties to avoid extra overhead.
- Prefer custom control creation or re-templating using control themes instead of CRUD-style UI.

## 3) ViewModel base

- All ViewModels inherit from `ViewModelBase`.

## 4) Input and interaction via Xaml.Behaviors

Reference: https://github.com/wieslawsoltes/Xaml.Behaviors

- All UI input and events are handled via behaviors/triggers.
- Prefer source-generator-based behaviors/actions wherever available.
- Use trigger behaviors for lifecycles and state transitions.
- Code-behind must not contain event handlers or direct ViewModel calls.

## 5) Docking layout with Dock for Avalonia

Reference: https://github.com/wieslawsoltes/Dock

- Use Dock.Model.* to represent the docking layout state.
- Use Dock.Avalonia for the view layer and Dock.Model.MVVM integration.
- Persist layout state to user settings and restore on startup.
- Keep layout logic in ViewModels; Views only render the Dock model.

## 6) Text editing with AvaloniaEdit

Reference: https://github.com/AvaloniaUI/AvaloniaEdit

- Use AvaloniaEdit `TextEditor` for all code/text editing surfaces.
- Enable syntax highlighting using TextMate grammars/themes.
- Keep editor configuration in ViewModels and bind to the view.

## 7) Data presentation with ProDataGrid

Reference: https://github.com/wieslawsoltes/ProDataGrid

- Use ProDataGrid `DataGrid` for all tabular data, tree views, and list displays.
- Always use the ProDataGrid model approach with code-based column bindings and fast paths.
- Always enable full filtering, searching, and sorting support.

## 8) Testing and validation

References:
- https://github.com/AvaloniaUI/Avalonia
- https://docs.avaloniaui.net/docs/testing/setting-up-the-headless-platform

- For UI bugs, add a failing headless regression test first, verify it reproduces the issue, then make the production change and rerun the same test before doing broader validation.
- UI tests must use Avalonia Headless and follow the headless testing guidance and helpers for input simulation.
- Unit-test ViewModels and UI-facing services.
- UI tests should validate navigation flows, docking, and editor behaviors.

## 9) Code conventions

- No code-behind event handlers.
- Avoid static state except truly immutable constants.
- Prefer explicit types where clarity is improved; avoid `var` in public APIs.
- All public APIs must be documented and unit-tested.
- No hacks or weird workarounds; if you think you need one, ask for guidance.
- UI text should be added to resources.
