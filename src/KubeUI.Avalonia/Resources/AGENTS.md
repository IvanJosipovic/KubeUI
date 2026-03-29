# Resource-Kind Features

## Ownership
- Each Kubernetes resource kind is a feature root under `Resources/<Category>/<Version>/<Kind>/`.
- Resource-kind folders own their config plus any local views, controls, view models, converters, and helpers.
- Keep only generic resource infrastructure at `Resources/` root.

## Constraints
- Do not introduce category-level shared views or view models for behavior owned by one resource kind.
- If a helper is reused by multiple resource kinds, keep it shared and document why.
