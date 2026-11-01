# Core Resources

## Subtree Guidance
- Core resources own their own list styling, controls, and actions when those behaviors are resource-specific.
- Event-specific rendering stays local to `v1/Event/`.
- Node CPU and memory list histories use the shared generic metrics-history control; keep NodeMetrics sampling and allocatable/capacity limit selection in the Node adapter, and preserve capacity values for sorting/filtering.
- Test Node metrics-history cells against both retained Metrics Server samples and Prometheus series labeled for the selected node.
