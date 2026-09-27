# KubeUI.DynamicTableView

`DynamicTableView` is a read-only Avalonia table control backed by a DynamicData change stream. The library uses typed delegates for row values and filters, so row members do not need reflection metadata.

## Theme setup

Add the library theme after Avalonia Fluent in the application's compiled XAML. Keep application overrides after this include.

```xml
<Application.Styles>
  <FluentTheme />
  <StyleInclude Source="avares://KubeUI.DynamicTableView/Themes/DynamicTableViewTheme.axaml" />
  <!-- application styles can override DynamicTableView templates below -->
</Application.Styles>
```

Use the compiled XAML include in trim-enabled applications. Loading the library theme from a runtime `StyleInclude` in C# uses Avalonia's runtime XAML loader.

`DynamicTableView.GridLinesVisibility` controls separators between cells and headers. It defaults to `None`; use `Horizontal`, `Vertical`, or `All` to show lines. Grid lines use Avalonia Fluent's `TableViewColumnHeaderSeparatorBackground` resource, so they follow the active Fluent theme. Override that Fluent resource in the host application to customize the color.

Filter flyouts default to 280 pixels wide, matching ProDataGrid's distinct-value filter width. Override the width on an individual table through its resources:

```xml
<DynamicTableView xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <DynamicTableView.Resources>
    <x:Double x:Key="DynamicTableView.FilterFlyoutWidth">360</x:Double>
  </DynamicTableView.Resources>
</DynamicTableView>
```

## Input and actions

The table does not assign meaning to Enter, Delete, taps, or double-taps. Add Avalonia `KeyBinding` instances to `DynamicTableView.KeyBindings` and subscribe to its native `Tapped` or `DoubleTapped` routed events in the caller. This keeps navigation and row actions specific to the application and lets consumers choose any gestures.
