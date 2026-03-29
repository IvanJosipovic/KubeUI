# Shell Subtree

## Ownership
- Shell folders own application chrome, navigation, home content, and static documents such as About and Settings.
- Keep docking, navigation, and document-opening behavior in view models or shell services.

## Constraints
- Shell views stay thin.
- Shared shell helpers should stay in `Shell/` or shared infrastructure, not in cluster or resource features.
