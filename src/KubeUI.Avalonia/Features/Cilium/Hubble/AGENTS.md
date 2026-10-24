# Hubble feature requirements

## Architecture

- Keep Hubble Relay transport, connection options, flow mapping, and graph data in `KubeUI.Cilium`.
- Keep navigation, document lifecycle, filtering, and Avalonia rendering in `KubeUI.Avalonia`.
- Resolve Hubble connections through `IHubbleConnectionFactory`; do not construct transport from a view or view model.
- Treat each Hubble document as the owner of its connection and subscriptions. Cancel initialization and dispose subscriptions and connections when the document closes.

## Flow and graph behavior

- Preserve individual Hubble flow events by UUID; events without UUIDs need distinct per-event cache identities.
- Keep flow-list filtering live so changing criteria also changes already received rows.
- Keep service-map history bounded by both age and event count. Apply selected time window, verdict, and namespace scopes when building the graph.
- Do not present unavailable Hubble fields, such as byte counts, as measured values.

## UI and tests

- Use compiled bindings and resource strings for visible UI text. Keep pointer and keyboard interactions out of view code-behind.
- Keep graph rendering in a custom Avalonia control and graph construction in the Cilium layer.
- Add focused tests for flow mapping and identity, filter changes, time and namespace scoping, graph aggregation, retention, DI registration, and document disposal.
- Follow `src/KubeUI.Avalonia/AGENTS.md` for Avalonia UI and headless testing rules.
