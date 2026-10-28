# Feature Flags

## Architecture

- Keep the contracts OpenFeature-shaped: `IFeatureClient`, `IFeatureProvider`, `EvaluationContext`, `ResolutionDetails`, reason constants, and error types.
- Keep the client boolean-only and synchronous until a remote or asynchronous provider is required. The settings-backed provider is in-memory and must not introduce async or blocking work.
- Do not add the OpenFeature SDK dependency without a migration plan. `FeatureClient` is the single adapter to replace when pivoting.
- `SettingsFeatureProvider` maps stable keys in `FeatureFlagKeys` to `Settings.PreviewFeatures`. Unknown flags fail closed to the caller-provided default with `FlagNotFound`.
- Feature flags are restart-required. Consumers that must not change during a session must capture the evaluated value when they are constructed.
- JSON and YAML reflection flags live in `Settings.PreviewFeatures` and are applied once during desktop startup. Disable-reflection flags default off, so reflection defaults on; JSON reflection requires runtime reflection support. YAML's generated converters use JSON-generated metadata for selected Kubernetes models.

## Adding a flag

- Add a key to `FeatureFlagKeys`.
- Add a `FeatureSettings` property with a default that preserves the current behavior.
- Add a provider mapping and a Settings UI toggle with localized text.
- Gate the consuming behavior and cover the enabled, disabled, default, and unknown-flag paths with tests.
- Update this file and the closest feature `AGENTS.md`.

- Capture serialization flags into application-owned JSON and YAML services during startup. Independent application hosts must support different settings in the same process. Apply JSON options to every Kubernetes client configuration; do not mutate the SDK global defaults or the legacy static YAML mode.

- Application registration takes `FeatureSettings` directly. Serialization defaults belong only to Settings; lower-layer registration requires explicit reflection policies and must not provide fallback defaults.
