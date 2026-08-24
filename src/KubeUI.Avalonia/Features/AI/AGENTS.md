# Engineering Guide - AI feature

This guide supplements `src/KubeUI.Avalonia/AGENTS.md` for the AI feature.

## AI panel

- Consume only `KubeUI.AI.Agents` abstractions; do not reference ACP or `dotacp` protocol types from Avalonia views or ViewModels.
- Keep the chat composer in the final grid row so it remains anchored below the scrollable conversation.
- Configure the selected agent through persisted Settings; the chat panel consumes that setting and does not own an agent selector.
- Selected-resource context may contain multiple selected resource identities plus lightweight metadata only; agents fetch full resource data through MCP.
- All user-visible AI text, including permission-dialog text and chat formatting markers, must be defined in `Assets/Resources.resx` and referenced through generated resources.

## Embedded MCP server

- MCP server startup must never crash the application: when the configured port cannot be bound (in use or blocked, e.g. Windows excluded port ranges), `McpServerConfiguration.ResolveAvailablePort` falls back to an ephemeral port, and a bind failure during host start disables the MCP server instead of failing (`Program.StartHost`).
- Agent sessions and `kubeui_get_mcp_endpoint` must report the live endpoint from `McpServerState.Endpoint` and fall back to `McpServerConfiguration.GetEndpoint` only when no live endpoint has been published.
