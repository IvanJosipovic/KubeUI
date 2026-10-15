# Shell Subtree

## Ownership
- Shell folders own application chrome, navigation, home content, and static documents such as About and Settings.
- Keep docking, navigation, and document-opening behavior in view models or shell services.

## Constraints
- Shell views stay thin.
- Shared shell helpers should stay in `Shell/` or shared infrastructure, not in cluster or resource features.

## Main window chrome
- Windows and Linux reuse Fluent's drawn-decoration template, center its title, and add an Options button at left. Keep Fluent caption controls and decoration roles intact.
- Keep File, Window, and Help actions in one shared definition for the Options flyout and native application menu on macOS.
- macOS retains native title-bar controls and its native application menu. Dialogs and floating dock windows retain their existing decorations.
- Bind client padding to `WindowDecorationMargin` when extending the client area into managed decorations so docking content cannot overlap the frame.
- Global `MenuItem` typography uses the shared app font resources so context menus and the Options flyout follow app settings without per-menu bindings.
- Headless tests cover centered title layout, Options command parity, and native macOS menu contents. Native decoration interactions should be verified on desktop platforms.
