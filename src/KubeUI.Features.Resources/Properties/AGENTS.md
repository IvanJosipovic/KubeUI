# Resource Properties

## Current Behavior
- There is at most one Properties view (fixed dock id `ResourcePropertiesViewModel`), docked (right tool dock by default) or auto-hidden in the right pinned strip.
- `ResourceConfigBase.View` opens it explicitly; replacing it disposes the previous instance so its cluster change subscription is released.
- `ResourcePropertiesCoordinator` owns opening, finding, and replacing the Properties dockable; background list selections do not retarget it.
- While it is open, list selection retargets it in place through `ResourcePropertiesViewModel.ShowResource` (same resource type) or replaces it in the same dock position / auto-hide strip (other resource type) without focusing or expanding it. `ShowResource` moves the change subscription when the cluster changes and rejects queued updates from earlier selections or subscriptions. Reattached views resume observing the view model.
- Cross-kind replacement preserves the auto-hide preview state: a previewed (temporarily shown) pinned view stays visible, a collapsed one stays collapsed.
- `ResourcePropertiesViewModel.IsShown` matches kind, API version, name, namespace and `Metadata.Uid`, so a stale informer callback for a recreated object with the same name cannot replace the current one.
- Dock's pin button means auto-hide; "keeping Properties open" (the opt-in from issue #1561) covers both docked and auto-hidden states.

## Validation
- Keep `OpenPropertiesFollowSelectionTests` covering docked and auto-hidden following, no implicit opening, multi-selection, cross-kind replacement (including preserved preview state) and disposal behavior.
