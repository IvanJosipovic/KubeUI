# Settings Document

## Current Behavior
- Settings opens as a document from the shell.
- Theme and appearance changes are persisted through the shared settings service.
- MCP server enablement and localhost port are persisted settings; changes require an application restart.
- External MCP tokens are shown once, while generated; settings persist only token verifiers and cluster grants. Cluster access uses checkboxes: no checked clusters means all clusters, otherwise only checked names are allowed.
- Keep client-name validation in Avalonia's `INotifyDataErrorInfo` pipeline so `TextBox` shows native validation state and accessible error help.
- External MCP client names must be unique, ignoring case and leading or trailing whitespace.
- Token creation and revocation must fail closed when settings cannot be saved. Keep the UI clear that ConfigMaps, custom resources, and pod logs may contain credentials, and Secret reads require approval.
- Settings are grouped under Application, Appearance, AI, and Preview Features headings; keep new options in the closest category.
- PreviewFeatures contains the restart-required Helm and Hubble integration toggles (default off) and Disable JSON Serialization Reflection and Disable YAML Serialization Reflection flags (default off; reflection enabled). All persist under the existing `Settings.Features` JSON key; changes only affect the next application session.
- Feature toggles bind directly to `Settings.PreviewFeatures`; `SettingsService` owns nested feature-change persistence.
- Leaving JSON reflection enabled adds a reflection fallback to the Kubernetes JSON resolver and requires runtime support for reflection-based JSON serialization. YAML reflection switches only its YamlDotNet builder; generated YAML converters continue using generated JSON metadata for selected Kubernetes models.

- Capture serialization flags into application-owned JSON and YAML services during startup. Independent application hosts must support different settings in the same process. Apply JSON options to every Kubernetes client configuration; do not mutate the SDK global defaults or the legacy static YAML mode.
