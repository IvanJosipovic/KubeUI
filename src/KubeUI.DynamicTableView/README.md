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
