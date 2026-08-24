---
name: kube-ui-testing
description: Run and target KubeUI tests from the command line with Microsoft.Testing.Platform, including project, solution, class, and method selection.
---

# KubeUI testing

Use this skill for KubeUI test runs, focused test selection, test-result diagnosis, and adding tests for new behavior.

Run commands from the repository root. KubeUI targets .NET 10 and opts into Microsoft.Testing.Platform through `global.json`; tests use xUnit v3.

## Required test workflow

1. Build before running tests:

   ```text
   dotnet build KubeUI.slnx --tl:off -clp:ErrorsOnly
   ```

2. Run the narrowest relevant test scope first, then widen to the project or solution when the focused run passes.
3. Treat a run that executes zero tests as invalid validation. Use `--minimum-expected-tests 1` for focused runs and verify the output reports a nonzero count.
4. For a bug, add the failing regression first, confirm it fails, implement the fix, and rerun the same regression before broader validation.

## CLI examples

The examples use `KubeUI.Avalonia.Tests` as “project A”,
`KubeUI.Avalonia.Tests.Converters.PropertyItemValueConverterTests` as “class A”,
and `converts_utc_datetime_to_local_time` as “test A”. Replace them with the
real project, fully qualified class, and method names for the change.

### Test project A

```text
dotnet test --project tests/KubeUI.Avalonia.Tests/KubeUI.Avalonia.Tests.csproj --no-restore --minimum-expected-tests 1
```

### Test all projects

```text
dotnet test --no-restore --minimum-expected-tests 1
```

### Test project A, class A

Use the project target with a fully qualified class filter:

```text
dotnet test --project tests/KubeUI.Avalonia.Tests/KubeUI.Avalonia.Tests.csproj --no-restore --filter-class KubeUI.Avalonia.Tests.Converters.PropertyItemValueConverterTests --minimum-expected-tests 1
```

### Test project A, test A

List tests from the project when the exact generated name is uncertain:

```text
dotnet test --project tests/KubeUI.Avalonia.Tests/KubeUI.Avalonia.Tests.csproj --no-restore --list-tests
```

Use a fully qualified method filter:

```text
dotnet test --project tests/KubeUI.Avalonia.Tests/KubeUI.Avalonia.Tests.csproj --no-restore --filter-method KubeUI.Avalonia.Tests.Converters.PropertyItemValueConverterTests.converts_utc_datetime_to_local_time --minimum-expected-tests 1
```

Use the fully qualified class or method name returned by `--list-tests` when selecting another target.

## Coverage and hang detection

Coverage collection and hang detection are enabled by the repository’s `testconfig.json`, which is automatically added to test projects by `Directory.Build.props`.

- Coverage is collected in Cobertura format under `TestResults`.
- Hang dumps are enabled with a one-minute timeout. Crash dumps and TRX reports are enabled too.

Inspect the generated `*.cobertura.xml` for line and branch rates. Collection being enabled does not by itself enforce a threshold.

Every new feature must include behavior-focused tests that cover at least 90% of the affected code and contribute to keeping the codebase at or above 90% coverage. Do not inflate the number with implementation-only assertions; cover meaningful branches, failure paths, and user-visible behavior. Report any uncovered code and the measured line/branch percentages.

## KubeUI test rules

- Use `[AvaloniaFact]` only for tests that require Avalonia; use `[Fact]` or `[Theory]` for pure model, serialization, transport, and service tests.
- Do not use `Task.Delay` or `Thread.Sleep`. Wait on an observable state, event, informer completion, or another bounded predicate.
- Pass `TestContext.Current.CancellationToken` through cancellable async operations and bound every wait.
- Use the production Kubernetes client with the shared fake transport for deterministic local tests. Use the shared fake/Kind matrix for backend parity; Kind cases require `KUBEUI_RUN_KIND_TESTS=1`.
- Dispose test-owned scopes, workspaces, and view models in the test that creates them.
