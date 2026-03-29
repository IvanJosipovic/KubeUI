# Configuration Resources

## Subtree Guidance
- Configuration resource kinds own their own columns, property views, and resource-specific converters.
- Secret-specific behavior stays local to `v1/Secret/`; do not generalize it without evidence of reuse.
