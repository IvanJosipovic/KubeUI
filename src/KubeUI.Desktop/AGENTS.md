# Engineering Guide (AGENTS) - KubeUI.Desktop

This guide supplements the repository root `AGENTS.md` for the executable host and composition root (`Program.cs`).

## Host startup

- The application host must always start, even when the embedded MCP server cannot bind its configured port.
- MCP port resolution order: configured `Settings.McpServerPort` is probed via `McpServerConfiguration.ResolveAvailablePort`; if it cannot be bound, Kestrel binds an operating system assigned ephemeral port instead. If a bind still fails at start time (race), `StartHost` rebuilds the host on an ephemeral port once before failing.
- The live bound endpoint is published to `McpServerState` after a successful start. Consumers that report or connect to the MCP endpoint (`McpTools.GetEndpoint`, `AgentChatViewModel`) must prefer `IMcpServerState.Endpoint` over the endpoint derived from settings.
- Persisted settings are never rewritten by the fallback; the user's configured port remains untouched.
