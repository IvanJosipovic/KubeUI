# Cluster Features

## Ownership
- Cluster features own cluster catalog screens, workspace/session state, overview screens, cluster settings, and cluster error presentation.
- Session-scoped cluster state should live in workspace or cluster services, not in views.

## Validation
- Preserve existing cluster connection, permission, and CRD-driven behaviors backed by tests.
- Cluster catalog Delete is scoped to the focused list and uses the selected cluster's normal delete command and confirmation flow.
