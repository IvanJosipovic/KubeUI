# Controls headless tests

- This assembly intentionally references `KubeUI.Controls` without App or feature projects.
- Keep its Avalonia host minimal: load framework/third-party themes explicitly and add Controls-owned styles deliberately.
- Add tests here when behavior must be proven independent of App startup; keep feature consumer/integration tests in their feature test projects.
- Use `[AvaloniaFact]` for UI behavior and `[Fact]` for pure presentation utilities. Own and close any test windows.
