# Configuration Resources

## Subtree Guidance
- Configuration resource kinds own their own columns, property views, and resource-specific converters.
- Secret-specific behavior stays local to `v1/Secret/`; do not generalize it without evidence of reuse.
- Secret and ConfigMap `data` editing uses shared properties UI, cached patch permission, and a data-only JSON Merge Patch; `binaryData`, certificate presentation, and other resource fields remain read-only.
