# Engineering Guide - AI feature

This guide supplements `src/KubeUI.Avalonia/AGENTS.md` for the AI feature.

## AI panel

- Consume only `KubeUI.AI.Agents` abstractions; do not reference ACP or `dotacp` protocol types from Avalonia views or ViewModels.
- Keep the chat composer in the final grid row so it remains anchored below the scrollable conversation.
- Configure the selected agent through persisted Settings; the chat panel consumes that setting and does not own an agent selector.
- Selected-resource context may contain multiple selected resource identities plus lightweight metadata only; agents fetch full resource data through MCP.
- All user-visible AI text, including permission-dialog text and chat formatting markers, must be defined in `Assets/Resources.resx` and referenced through generated resources.

## Embedded MCP server

- The host is started through `Program.CreateStartedHost`. When the configured MCP port cannot be bound (in use or blocked by the OS), the host is rebuilt once with `McpServerConfiguration.DynamicPort` so a bind failure never crashes app startup.
- After a successful bind the actual port is recorded in `McpServerState` (`IMcpServerState`). Endpoint consumers (`McpTools.GetEndpoint`, agent chat sessions) must resolve the endpoint through `McpServerConfiguration.GetEndpoint(settings, boundPort)` so they honor the port the server actually bound to.
- When MCP is disabled the host must still not claim a fixed port; Kestrel binds an OS-assigned loopback port so multiple app instances never collide at startup.
