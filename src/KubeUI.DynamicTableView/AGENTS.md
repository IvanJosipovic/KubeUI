# KubeUI.DynamicTableView

- Keep the library independent from KubeUI application projects. It may depend on Avalonia and DynamicData only.
- Organize code by responsibility under `Controls/`, `Columns/`, `Data/`, `Filtering/`, `Infrastructure/`, `Selection/`, `State/`, `Themes/`, and `Assets/`.
- Keep one top-level class, record, struct, enum, or interface per C# file.
- Compose visible controls with overridable templates in `Themes/DynamicTableViewTheme.axaml`. The default templates use standard Avalonia controls so they pick up the application's Fluent theme; avoid inline dimensions or visual styling in control logic.
- Declare Avalonia properties with `PropertyGenerator.Avalonia` (`GeneratedDirectProperty` by default); use generated styled properties only when theme styling must set the property. Keep property-change behavior in generated partial callbacks or `OnPropertyChangedOverride`.
- Consumers should include `Themes/DynamicTableViewTheme.axaml` through compiled application XAML after `FluentTheme`, which keeps theme loading trim-safe and lets later application styles override it.
- Keep the library trim-safe: typed delegates, compiled Avalonia templates, no reflection-based row or enum discovery.
- Saved table state includes column order and widths, sorts, filters, and horizontal and vertical scroll offsets.
- Source options choose stable-key or reference-based selection identity and Replace versus Remove/Add update notifications; defaults retain key identity and Replace notifications.
- Every public API and retained feature must have focused tests in `tests/KubeUI.DynamicTableView.Tests`.
- Keep benchmarks isolated in `benchmarks/KubeUI.DynamicTableView.Benchmarks` and reference this library only.
