# Shell Subtree

## Ownership
- Shell folders own application chrome, navigation, home content, and static documents such as About and Settings.
- Keep docking, navigation, and document-opening behavior in view models or shell services.

## Constraints
- Shell views stay thin.
- Shared shell helpers should stay in `Shell/` or shared infrastructure, not in cluster or resource features.
- The main menu exists twice: the in-window `Menu` in `Main/MainView.cs` (hidden on macOS) and the macOS `NativeMenu` in `Main/MainWindow.cs`. Every item added to one must be added to the other in the same order; `MainWindowMenuTests` enforces this.
