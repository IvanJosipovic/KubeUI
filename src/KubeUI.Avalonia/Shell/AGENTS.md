# Shell Subtree

## Ownership
- Shell folders own application chrome, navigation, home content, and static documents such as About and Settings.
- Keep docking, navigation, and document-opening behavior in view models or shell services.

## Constraints
- Shell views stay thin.
- Shared shell helpers should stay in `Shell/` or shared infrastructure, not in cluster or resource features.
- The application menu is defined once as a `NativeMenu` built in `Main/MainWindow.cs` and attached to the window. `Main/MainView.cs` only hosts a `NativeMenuBar`, which presents that same menu in-window wherever the platform does not export a system menu bar (macOS exports it, so the in-window presenter hides itself). Never build a second menu in the view; add menu items to the `NativeMenu` only.
