# Gateway API Navigation

- Match Gateway API CRDs by their exact API group and kind.
- Use the CRD `gateway.networking.k8s.io/channel` annotation to select Standard versus Experimental. Both install bundles can contain the same group and kind with different channel membership.
- Place each Gateway API resource under Network → Gateway from its matching kind config. Each kind config must set `NavigationSectionId`; shared `GatewayApiCustomResourceConfig` logic adds Experimental beneath Gateway when the CRD channel annotation is experimental.
- Keep each Gateway API kind config in a folder named for its kind. Put that kind's YAML directly in its config's `Template` property. Each config declares its exact API group + kind + storage version.
- Keep one config per CRD group + kind. Stable and experimental bundle variants of the same CRD share the config; channel membership comes from the CRD annotation.
