# KubeUI.DynamicTableView

- Keep the library independent from KubeUI application projects. It may depend on Avalonia and DynamicData only.
- Organize code by responsibility under `Controls/`, `Columns/`, `Data/`, `Filtering/`, `Infrastructure/`, `Selection/`, `State/`, `Themes/`, and `Assets/`.
- Keep one top-level class, record, struct, enum, or interface per C# file.
- Compose visible controls with overridable templates in `Themes/DynamicTableViewTheme.axaml`. The default templates use standard Avalonia controls so they pick up the application's Fluent theme; avoid inline dimensions or visual styling in control logic.
- Declare Avalonia properties with `PropertyGenerator.Avalonia` (`GeneratedDirectProperty` by default); use generated styled properties only when theme styling must set the property. Keep property-change behavior in generated partial callbacks or `OnPropertyChangedOverride`.
- Consumers should include `Themes/DynamicTableViewTheme.axaml` through compiled application XAML after `FluentTheme`, which keeps theme loading trim-safe and lets later application styles override it.
- Keep the library trim-safe: typed delegates, compiled Avalonia templates, no reflection-based row or enum discovery.
- Saved table state includes column order and widths, sorts, filters, and horizontal and vertical scroll offsets.
- Source options choose stable-key or reference-based selection identity. DynamicData's default Replace update notifications remain in effect.
- Keep selection backed by Avalonia's `SelectionModel`. `DynamicTableView` disables `AutoScrollToSelectedItem` so source refreshes and explicit selection changes do not move the viewport.
- Keep column header labels and interactions available after the table detaches and reattaches; preserve restored column order and widths across that transition.
- Mouse drag across rows selects the contiguous range when starting on an unselected row and deselects it when starting on a selected row; moving back restores rows outside current range to their initial selection state.
- Keep grid separators controlled by the styleable `GridLinesVisibility` property. Only set borders when separators are enabled; use Fluent's `TableViewColumnHeaderSeparatorBackground` resource instead of defining grid-line colors in the library. Hosts can override the Fluent resource when needed.
- Keep the default header strip at least 6 pixels taller than Fluent data rows through `DynamicTableView.HeaderMinHeight`; do not alter row sizing or header colors for this distinction.
- Keep standard filter flyouts at the default 280-pixel width through the overridable `DynamicTableView.FilterFlyoutWidth` theme resource.
- Keep keyboard shortcuts and tap actions caller-owned. Consumers use Avalonia `KeyBindings`, `Tapped`, and `DoubleTapped` directly; do not bake application actions or key meanings into the control.
- Every public API and retained feature must have focused tests in `tests/KubeUI.DynamicTableView.Tests`.
- Keep benchmarks isolated in `benchmarks/KubeUI.DynamicTableView.Benchmarks` and reference this library only.
