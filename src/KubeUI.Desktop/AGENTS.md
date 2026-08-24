# Engineering Guide (AGENTS) - KubeUI.Desktop

This guide supplements the repository root `AGENTS.md` and defines rules for `src/KubeUI.Desktop`.

## 1) Host startup

- The desktop host is the composition root; all DI wiring is configured here (`Program.CreateHostBuilder`) and resolved by `KubeUI.Avalonia`.
- Host startup must be resilient to an unavailable MCP server port:
  - Use `Program.StartHost` to start the host. When binding the configured MCP port fails with `AddressAlreadyInUse` or `AccessDenied` (for example Windows excluded/reserved port ranges or a stale listener), the host is disposed and restarted on an ephemeral port instead of crashing the app.
  - After a successful start, `Settings.McpServerPort` is synchronized with the actually bound port so `McpServerConfiguration.GetEndpoint` always reports the listening URL.
  - Dynamic (port `0`) MCP endpoints must use `Listen(IPAddress.Loopback, 0)`/`Listen(IPAddress.IPv6Loopback, 0)`; Kestrel does not support dynamic ports via `ListenLocalhost`.
- Regression coverage for MCP host startup lives in `tests/KubeUI.Avalonia.Tests/Infrastructure/Mcp/McpServerHostedServiceTests.cs`.
