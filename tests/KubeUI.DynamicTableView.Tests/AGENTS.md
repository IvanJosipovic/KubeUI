# DynamicTableView Tests

- Keep these tests independent from KubeUI application projects; reference only the DynamicTableView library and its test dependencies.
- Place unit and headless UI tests in separate folders. Use one top-level type per C# file.
- Test each retained feature alone and in combinations that affect query results, selection, templates, and state.
- State tests cover column order and width, scroll offset capture and restore, including when the virtualized extent grows after restore and when a restored table leaves and re-enters the visual tree.
- Selection tests compare Avalonia's native SelectionModel behavior with the minimal identity preservation KubeUI needs. `DynamicTableView` disables selected-item auto-scroll; test source refreshes and selection changes without viewport jumps.
- Selection-change tests cover identity preservation across replace, add, remove, move, sort, and updates; filter and search tests verify hidden rows leave selection and do not return when query is cleared.
- Grid-line UI tests cover None, Horizontal, Vertical, and All on both data cells and column headers.
- Use virtual schedulers and explicit completion events. Do not use `Task.Delay` or `Thread.Sleep`.
- Build `KubeUI.slnx` with `dotnet build --tl:off -clp:ErrorsOnly` before running tests.
- UI tests use `[AvaloniaFact]`; data-source tests use `[Fact]` or `[Theory]`.
