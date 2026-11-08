# Cluster Settings

## Current Behavior
- Cluster settings are initialized from a selected cluster workspace.
- This screen is the current destination when namespace list permission is missing.

## Cluster Metrics Authentication
- Prometheus settings support persisted custom request headers and Basic Auth credentials; the view must clearly disclose that settings contain plaintext secrets and mask secret inputs.
- Require HTTPS for authenticated Prometheus requests by default. Users may explicitly opt into HTTP, with a visible warning when credentials would be sent without transport encryption.
- Changes to custom header collections and their items must trigger the same debounced settings save and metrics reinitialization as other metrics settings.
