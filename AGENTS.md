# KubeUI Engineering Guide

## Scope
- Keep changes behavior-preserving and scoped to the requested feature or refactor.
- Prefer feature-local code and instructions over broad shared buckets.
- Keep `AGENTS.md` guidance close to the code and avoid duplicating parent guidance in child files.

## Architecture
- `src/KubeUI.Kubernetes` contains non-UI Kubernetes/runtime behavior and contracts.
- `src/KubeUI.Avalonia` contains Avalonia views, view models, behaviors, controls, shell glue, and resource UI.
- `src/KubeUI.Desktop` is the executable composition root.
- Keep reusable cross-feature code shared only when it is used by multiple features or resource kinds.

## Avalonia MVVM
- Keep AXAML declarative and code-behind limited to `InitializeComponent()` plus required view wiring.
- Put behavior in view models or services, not in views.
- Keep view models testable and UI-framework-agnostic where practical.
- Use compiled bindings with explicit `x:DataType`.

## Validation
- Prefer ViewModel and service tests for behavior coverage.
- Run `dotnet build`, `dotnet test tests/KubeUI.Avalonia.Tests/KubeUI.Avalonia.Tests.csproj`, and `dotnet test tests/KubeUI.Kubernetes.Tests/KubeUI.Kubernetes.Tests.csproj` after production changes when feasible.
- If behavior is uncertain, preserve the current implementation and leave a `TODO` instead of guessing.
