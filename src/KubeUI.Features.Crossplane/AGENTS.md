# Crossplane Features

- `CrossplaneNavigationSectionProvider` owns the Crossplane section: the section is available when a CRD has the exact API group `crossplane.io` or a dot-boundary subgroup ending in `.crossplane.io`. Preserve the matching CRD group hierarchy as the alias path. Surface MR Diff Detection only when Provider list/watch access exists.
- Preserve the existing CRD group hierarchy when placing matching resources in the Crossplane section; canonical CRD navigation remains available.
- Keep section registrations stateless and in the Crossplane feature area; section-specific documents belong to the relevant child feature.
