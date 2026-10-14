---
sidebar_position: 8
description: Enable KubeUI's local MCP server and authorize trusted host tools with per-client bearer tokens.
---

# MCP server

KubeUI can expose selected cluster information through an opt-in Model Context Protocol (MCP) server. The server listens on IPv4 loopback, so clients must run on the same computer as KubeUI.

## Enable MCP

Open **Settings → AI**, enable the MCP server, choose its port if needed, and restart KubeUI. The default endpoint is:

```text
http://127.0.0.1:62888/mcp
```

Keep the endpoint on loopback. KubeUI blocks browser cross-origin requests and does not support LAN or reverse-proxy access.

## Authorize an external host tool

In **Settings → AI → External MCP clients**:

1. Enter a unique label for the client. Names are case-insensitive; leading and trailing spaces are ignored.
2. Choose cluster access. Leave all boxes unchecked to allow all clusters, including clusters added later. Check one or more clusters to limit this token to those exact cluster names.
3. Select **Generate external client token**.
4. Copy the token while it is displayed. KubeUI stores only a verifier, so the token cannot be shown again. Generate a replacement if it is lost.
5. Configure the MCP client to send this HTTP header on requests to KubeUI:

   ```text
   Authorization: Bearer <token>
   ```

For clients whose configuration uses JSON, the connection details commonly look like this; follow the client's own configuration format:

```json
{
  "url": "http://127.0.0.1:62888/mcp",
  "headers": {
    "Authorization": "Bearer <token>"
  }
}
```

The external client must support Streamable HTTP and per-server custom headers. Clients that only support OAuth sign-in or cannot set an Authorization header cannot use this local token setup. Keep the raw token in the client's secret store or an environment variable; do not commit it to a shared config file.

### Codex CLI

Set `KUBEUI_MCP_TOKEN` in the environment used to launch Codex, then add this to `~/.codex/config.toml`:

```toml
[mcp_servers.kubeui]
url = "http://127.0.0.1:62888/mcp"
bearer_token_env_var = "KUBEUI_MCP_TOKEN"
```

### Claude Code

Set `KUBEUI_MCP_TOKEN` in the environment used to launch Claude Code. Add this server to `~/.claude.json` or the project's `.mcp.json`:

```json
{
  "mcpServers": {
    "kubeui": {
      "type": "http",
      "url": "http://127.0.0.1:62888/mcp",
      "headers": {
        "Authorization": "Bearer ${KUBEUI_MCP_TOKEN}"
      }
    }
  }
}
```

Use a custom variable such as `KUBEUI_MCP_TOKEN`; Claude Code intentionally filters provider credential variables from remote MCP headers.

### VS Code with Copilot

Add this server to `.vscode/mcp.json`. VS Code prompts for the token and marks the input as secret:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "kubeui-token",
      "description": "KubeUI external MCP token",
      "password": true
    }
  ],
  "servers": {
    "kubeui": {
      "type": "http",
      "url": "http://127.0.0.1:62888/mcp",
      "headers": {
        "Authorization": "Bearer ${input:kubeui-token}"
      }
    }
  }
}
```

VS Code's Agent Host does not forward servers that use interactive `${input:...}` values. Configure that host through a supported user-level secret/header mechanism instead, and avoid putting the token in a shared workspace file.

See the clients' current setup details: [Codex MCP servers](https://developers.openai.com/codex/mcp), [Claude Code MCP](https://code.claude.com/docs/en/mcp), and [VS Code MCP configuration](https://code.visualstudio.com/docs/agents/reference/mcp-configuration).

Select a registered client in Settings and choose **Revoke selected token** to stop future requests using it. Generate a separate token for each client. Check only the clusters it needs; leaving every cluster unchecked grants access to all clusters, including clusters added later. KubeUI persists token verifiers and cluster grants in its user settings; it never stores the raw external token.

KubeUI-managed ACP sessions receive a short-lived token automatically. Do not copy an external client token for an agent selected inside KubeUI.

## What an authorized client can read

When cluster boxes are checked, the token can connect only to those exact clusters. When none are checked, it can connect to all clusters known to KubeUI, including clusters added later. Kubernetes RBAC still limits the data available through each cluster identity. Current MCP tools can connect to clusters, list supported resource types and resource names, read resource YAML, read pod logs, compare YAML, inspect resource relationships, and open a resource list in KubeUI. Resource and Event listings return pages of up to 100 items by default (up to 500 when requested). When a response includes `nextCursor`, pass it to the same listing tool with the same query to get the next page. The cursor follows stable namespace, name, and UID ordering over KubeUI's live cache. Informer updates do not invalidate it, but changes during a multi-page listing can affect later pages. These tools do not apply or delete Kubernetes resources.

Secret YAML requires approval in KubeUI. ConfigMaps, custom resources, and pod logs can also contain credentials. Give tokens only to clients you trust, review their tool calls and returned cluster data, and revoke grants you no longer need.

These are local bearer tokens, not MCP OAuth credentials. KubeUI does not provide OAuth discovery or remote authorization. Do not expose this endpoint beyond the host computer.
