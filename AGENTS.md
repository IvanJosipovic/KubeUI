# Engineering Guide (AGENTS)

This document defines the repository-wide architectural and coding rules. For Avalonia-specific UI guidance, see `src/KubeUI.Avalonia/AGENTS.md`.

## 0) Pre-requisites
.Net Core SDK installed based on `global.json`
Docker

## 1) Core principles

### SOLID
- Single Responsibility: every class has exactly one reason to change.
- Open/Closed: extend behavior via composition and interfaces; avoid modifying stable code paths when adding features.
- Liskov Substitution: derived types must be safely substitutable without altering expected behavior or contract.
- Interface Segregation: prefer small, focused interfaces; avoid "god" interfaces.
- Dependency Inversion: depend on abstractions; wire concrete types in the composition root only.

## 2) Architecture

### Layering
- KubeUI.Kubernetes: all non-UI Kubernetes/runtime/shared contracts
- KubeUI.Avalonia: all Avalonia UI, view models, resource configs, shell glue
- KubeUI.Desktop: executable host and composition root

### Boundaries
- KubeUI.Desktop depends on KubeUI.Avalonia depends on KubeUI.Kubernetes

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

- Avoid reflection whenever possible.
- Prefer source generators before any reflection-based approach.
- If reflection is the only viable option, ask the user explicitly before introducing it.

## 6) Testing and validation

- All production code must be covered by unit tests; xUnit is required for unit testing.
- Use integration tests for parsing, IO, and docking layout persistence.
- Avalonia UI tests and headless UI guidance live in `src/KubeUI.Avalonia/AGENTS.md`.

## 7) Code conventions

- Avoid static state except truly immutable constants.
- Prefer explicit types where clarity is improved; avoid `var` in public APIs.
- All public APIs must be documented and unit-tested.
- No hacks or weird workarounds; if you think you need one, ask for guidance.

## 8) File conventions

Use CRLF line endings for KubeUI source and project files.

Treat these as CRLF:
- .cs
- .axaml
- .axaml.cs
- .xaml
- .csproj
- .props
- .targets
- .sln
- .json
- .md
- .yml
- .yaml
- .xml

Treat shell scripts as LF:
- .sh

Respect `.editorconfig` and `.gitattributes`.
Do not rewrite line endings unless required by those files or explicitly requested.
