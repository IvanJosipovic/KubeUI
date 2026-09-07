# Engineering Guide - AI feature

This guide supplements `src/KubeUI.Avalonia/AGENTS.md` for the AI feature.

## AI panel

- Consume only `KubeUI.AI.Agents` abstractions; do not reference ACP or `dotacp` protocol types from Avalonia views or ViewModels.
- Keep the chat composer in the final grid row so it remains anchored below the scrollable conversation.
- Configure the selected agent through persisted Settings; the chat panel consumes that setting and does not own an agent selector.
- Selected-resource context may contain multiple selected resource identities plus lightweight metadata only; agents fetch full resource data through MCP.
- All user-visible AI text, including permission-dialog text and chat formatting markers, must be defined in `Assets/Resources.resx` and referenced through generated resources.

## Embedded MCP server

- The host is started through `Program.CreateStartedHost`. MCP uses a custom `IHostedService` with Kestrel inside that same desktop host; a configured port bind failure is logged and leaves desktop startup healthy.
- After a successful bind the configured port is recorded in `McpServerState` (`IMcpServerState`). Endpoint consumers (`McpTools.GetEndpoint`, agent chat sessions) use `McpServerConfiguration.GetEndpoint(settings)`.
- When MCP is disabled, no Kestrel server is registered or started.
