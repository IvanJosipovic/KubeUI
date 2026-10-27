# Feature Flags

## Architecture

- Keep the contracts OpenFeature-shaped: `IFeatureClient`, `IFeatureProvider`, `EvaluationContext`, `ResolutionDetails`, reason constants, and error types.
- Keep the client boolean-only and synchronous until a remote or asynchronous provider is required. The settings-backed provider is in-memory and must not introduce async or blocking work.
- Do not add the OpenFeature SDK dependency without a migration plan. `FeatureClient` is the single adapter to replace when pivoting.
- `SettingsFeatureProvider` maps stable keys in `FeatureFlagKeys` to `Settings.Features`. Unknown flags fail closed to the caller-provided default with `FlagNotFound`.
- Feature flags are restart-required. Consumers that must not change during a session must capture the evaluated value when they are constructed.

## Adding a flag

- Add a key to `FeatureFlagKeys`.
- Add a default-off property to `FeatureSettings`.
- Add a provider mapping and a Settings UI toggle with localized text.
- Gate the consuming behavior and cover the enabled, disabled, default, and unknown-flag paths with tests.
- Update this file and the closest feature `AGENTS.md`.
