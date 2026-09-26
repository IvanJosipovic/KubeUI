# DynamicTableView Tests

- Keep these tests independent from KubeUI application projects; reference only the DynamicTableView library and its test dependencies.
- Place unit and headless UI tests in separate folders. Use one top-level type per C# file.
- Test each retained feature alone and in combinations that affect query results, selection, templates, and state.
- State tests cover scroll offset capture and restore, including when the virtualized extent grows after restore.
- Selection tests compare Avalonia's native SelectionModel behavior with the minimal identity preservation KubeUI needs.
- Use virtual schedulers and explicit completion events. Do not use `Task.Delay` or `Thread.Sleep`.
- Build `KubeUI.slnx` with `dotnet build --tl:off -clp:ErrorsOnly` before running tests.
- UI tests use `[AvaloniaFact]`; data-source tests use `[Fact]` or `[Theory]`.
