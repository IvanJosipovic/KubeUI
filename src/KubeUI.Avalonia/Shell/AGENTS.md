# Shell Subtree

## Ownership
- Shell folders own application chrome, navigation, home content, and static documents such as About and Settings.
- Keep docking, navigation, and document-opening behavior in view models or shell services.

## Constraints
- Shell views stay thin.
- Shared shell helpers should stay in `Shell/` or shared infrastructure, not in cluster or resource features.
- Update discovery opens one docked Updates document, reusing it when already open. Render release notes with the shared Markdown control and require confirmation before starting an update.
- The main menu exists twice: the in-window `Menu` in `Main/MainView.cs` (hidden on macOS) and the macOS `NativeMenu` in `Main/MainWindow.cs`. Every item added to one must be added to the other in the same order; `MainWindowMenuTests` enforces this.
