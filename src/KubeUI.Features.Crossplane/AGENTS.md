# Crossplane Features

- `CrossplaneNavigationSectionProvider` owns the Crossplane section: the section is available only when a CRD has the exact API group `crossplane.io`. It matches those CRDs and dot-boundary subgroups ending in `.crossplane.io`, preserving their group hierarchy as the alias path; a subgroup alone must not activate the section. Surface MR Diff Detection only when Provider list/watch access exists.
- Preserve the existing CRD group hierarchy when placing matching resources in the Crossplane section; canonical CRD navigation remains available.
- Keep section registrations stateless and in the Crossplane feature area; section-specific documents belong to the relevant child feature.
