# Cluster Settings

## Current Behavior
- Cluster settings are initialized from a selected cluster workspace.
- This screen is the current destination when namespace list permission is missing.

## Cluster Metrics Authentication
- Prometheus settings support persisted custom request headers and Basic Auth credentials; the view must clearly disclose that settings contain plaintext secrets and mask secret inputs.
- Allow authenticated Prometheus requests over HTTP. Show a visible warning when credentials would be sent without transport encryption; do not add a separate HTTPS enforcement toggle.
- Changes to custom header collections and their items must trigger the same debounced settings save and metrics reinitialization as other metrics settings.
- Manual Prometheus configuration separates Kubernetes Service port-forward and Direct URL in expandable sections. A populated Direct URL selects direct mode and requires no service details; keep authentication in its own expandable section.
