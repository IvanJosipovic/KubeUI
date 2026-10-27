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
- Preview Features contains the restart-required Helm and Hubble integration toggles. Both default off, persist under `Settings.Features`, and only affect the next application session.
- Feature toggles bind directly to `Settings.Features`; `SettingsService` owns nested feature-change persistence.
