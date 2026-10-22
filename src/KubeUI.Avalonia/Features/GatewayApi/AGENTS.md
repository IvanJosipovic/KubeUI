# Gateway API Navigation

- Match Gateway API CRDs by their exact API group and kind.
- Place stable Gateway API kinds (`gateway.networking.k8s.io`) under Network → Gateway.
- Place experimental-only kinds (`gateway.networking.x-k8s.io`) under Network → Gateway → Experimental. Select this folder from the exact API group, not the `gateway.networking.k8s.io/channel` annotation: the experimental install bundle marks all included CRDs with that annotation, including stable-group kinds.
- Each kind config derives from `GatewayApiCustomResourceConfig`, which owns the `Navigation` placement: stable API group configs use Network → Gateway; `x-k8s.io` configs use Network → Gateway → Experimental. A kind config declares only its API group, kind, and template.
- Keep each Gateway API kind config in a folder named for its kind. Put that kind's YAML directly in its config's `Template` property. Each config declares its exact API group + kind and matches any served CRD version; `Configure` always uses the storage version.
- Keep one config per CRD group + kind. Stable and experimental bundle variants that share a group and kind share the config; bundle channel annotations do not change its navigation placement.
