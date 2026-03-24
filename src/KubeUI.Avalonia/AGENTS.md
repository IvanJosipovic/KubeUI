# Avalonia Subtree

## Structure
- `Shell/` owns app shell, navigation, and static documents.
- `Features/` owns user-facing screens and view models.
- `Resources/` owns Kubernetes resource-kind features.
- `Controls/`, `Behaviors/`, `Services/`, `Styles/`, and `Assets/` remain shared when reused across multiple features.

## Implementation
- Prefer folder-first feature organization with minimal namespace churn.
- Preserve `View`/`ViewModel` naming pairs so `ViewLocator` continues to resolve views.
- Update `x:Class`, `x:DataType`, and `xmlns` imports when a move requires it.
