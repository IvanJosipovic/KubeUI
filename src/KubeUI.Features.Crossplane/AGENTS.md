# Crossplane Features

- `CrossplaneNavigationSectionProvider` owns the Crossplane section: it matches CRD groups, preserves their group hierarchy as the alias path, and surfaces MR Diff Detection only when Provider list/watch access exists. Match `crossplane.io` plus dot-boundary subgroups ending in `.crossplane.io`.
- Preserve the existing CRD group hierarchy when placing matching resources in the Crossplane section; canonical CRD navigation remains available.
- Keep section registrations stateless and in the Crossplane feature area; section-specific documents belong to the relevant child feature.
