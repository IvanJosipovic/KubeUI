# Resource-Kind Features

## Ownership
- Each Kubernetes resource kind is a feature root under `Resources/<Category>/<Version>/<Kind>/`.
- Resource-kind folders own their config plus any local views, controls, view models, converters, and helpers.
- Keep only generic resource infrastructure at `Resources/` root.

## Constraints
- Do not introduce category-level shared views or view models for behavior owned by one resource kind.
- If a helper is reused by multiple resource kinds, keep it shared and document why.

## Gateway API
- Gateway API kinds are CRD-backed and managed through `CRDResourceConfig` (list, YAML edit/save, delete, new).
- `GatewayApiResources` (KubeUI.Kubernetes) classifies the channel: the `x-k8s.io` group or kinds without a served non-alpha version are experimental. Do not rely on the `gateway.networking.k8s.io/channel` annotation, because the experimental bundle also puts it on stable kinds.
- New Gateway API resources start from the kind's spec template in `GatewayApiResources.TryGetSpecTemplate`; add a template when adding support for a new kind.
