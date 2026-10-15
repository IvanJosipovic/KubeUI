# Resource Properties

## Current Behavior
- There is at most one Properties view (fixed dock id `ResourcePropertiesViewModel`) in the right tool dock or the right pinned strip.
- `ResourceConfigBase.View` opens it explicitly; replacing it disposes the previous instance so its cluster change subscription is released.
- While it is pinned, list selection retargets it in place through `ResourcePropertiesViewModel.ShowResource` (same resource type) or replaces it with a pinned, non-previewed view (other resource type). `ShowResource` moves the change subscription when the cluster changes.

## Validation
- Keep `PinnedPropertiesFollowSelectionTests` covering pinned/unpinned, multi-selection, cross-kind replacement and disposal behavior.
