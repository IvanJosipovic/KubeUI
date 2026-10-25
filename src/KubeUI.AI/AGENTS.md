# KubeUI.AI engineering guide

- Use `SvcSystems.AI.AgentClientProtocol` for ACP transport, wire dispatch, and session events; do not add a second JSON-RPC or process transport implementation.
- Preserve tool-call identity across `ToolCall` and `ToolCallUpdate`, and invoke `authenticate` only for agent-handled authentication methods.
- Treat ACP permission requests as approval gated when tool identity is incomplete. The library permission callback must expose raw input, tool metadata, and tool kind before KubeUI may classify non-MCP operations as safe.
- Treat agent runtimes as configurable processes; never hardcode runtime-specific behavior into the transport.
- Keep prompts, responses, credentials, kubeconfigs, and Kubernetes Secret contents out of telemetry by default.
- Add session event handling and host adapter tests for every supported ACP update variant.
- Gate HTTP MCP session entries on the agent's negotiated HTTP MCP capability.
- KubeUI-owned MCP sessions use a per-session in-memory bearer credential forwarded only in the MCP HTTP headers; revoke it on unsupported negotiation, session creation failure, and session disposal.
- Keep MCP request identity, tool-call audit categories, and tool-result size limits in this project. Audit only client ID, tool name, result category, and duration; exclude tokens, prompts, cluster data, and tool results.
- Keep MCP credentials out of prompts, command arguments, record dumps, diagnostic events, exceptions, and telemetry.
- Treat ACP file line numbers as 1-based and enforce configured `AgentSessionOptions.FileSystemRoots`.
- Terminal output limits retain newest output, omit exit status until process exit, and observe capture-task failures during release and disposal.
- Unsupported ACP extension methods and notifications fail explicitly; fake ACP tests remain external-process free.
