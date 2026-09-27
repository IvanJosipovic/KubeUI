# KubeUI.DynamicTableView sample

Run the sample from the repository root:

```powershell
dotnet run --project samples/KubeUI.DynamicTableView.Sample
```

Both tabs use the same `SampleRow` model and the same 1,000-row collection. Each shows the same six columns: text, `int`, `bool`, `decimal`, `float`, and `DateTimeOffset`.

The first tab declares the control and columns in `MainWindow.axaml`. The second builds them in `MainWindow.axaml.cs`. Both use typed selectors; no reflection.

XAML uses a small adapter because the library columns need typed C# selectors. The app loads the library theme after `FluentTheme`.
