# Kubernetes serialization

- Application instances own their JSON options and YAML serializer through singleton services registered in their own containers.
- Capture reflection policies when services are registered. Settings changes take effect in the next application instance.
- Never mutate KubernetesClient's global JSON defaults or the legacy static YAML mode during application startup.
- Apply the application JSON resolver to every Kubernetes client configuration before creating the client, including clients supplied by test transport factories.
- Preserve KubernetesClient's converters when creating JSON options. Generated model metadata must remain available in both reflection modes.
- Use the stateless `SVCSystems.Text.Json.Yaml` adapter with the owning application's JSON options; do not add YamlDotNet serializer caches here.
- Keep static YAML helpers internal to the codec implementation; application resource parsing, validation, previews, and exports use the injected serializer.
- Test through desktop startup with Settings overrides, using an unregistered Kubernetes-like model. Cover both directions, defaults, all flag combinations, and simultaneous application instances. Do not create test-only JSON options or launch child test processes.

- Application consumers and shared test infrastructure must not call the legacy static YAML or SDK JSON serializers directly. Keep backend access in serialization adapters; fixed protocol status parsing may use explicit generated JSON metadata.

- Validate generated JSON metadata coverage in tests, deriving resource model types from the application resource registry. Do not maintain a partial production checklist or scan models during serializer initialization. Test fixed protocol payloads separately.
