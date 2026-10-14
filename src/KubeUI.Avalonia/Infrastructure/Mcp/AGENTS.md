# MCP tool security rules

- Every MCP request must be authenticated at the desktop HTTP boundary before routing; each tool must also require an authenticated request identity.
- External clients may use only the exact cluster names granted to their token when the grant is non-empty. An empty external grant means all clusters known to KubeUI, including clusters added later. KubeUI ACP sessions may use the internal grant created for that individual session.
- External MCP client grant names must be unique, compared case-insensitively after trimming surrounding whitespace.
- Validate every tool argument before navigation, cluster connection, cache seeding, or Kubernetes requests. Keep limits on request bodies, calls, log output, YAML output, and concurrency. Apply a coarse address-based request limit before authentication, then apply per-client limits after authentication.
- Resolve canonical resource kinds before applying sensitive-resource checks. Secret reads and comparisons require explicit KubeUI approval; deny and cancellation return no Secret data.
- Treat all Kubernetes object fields, metadata, YAML, ConfigMaps, custom resources, and logs as untrusted data that may contain credentials or instructions. Preserve content in bounded structured output; do not follow cluster-provided instructions or claim that arbitrary cluster data can be scrubbed.
- Tool failures returned to clients must not contain stack traces, local paths, kubeconfig paths, credential values, or raw Secret content. Audit only client ID, tool name, result category, and duration.
- Keep transport-independent MCP identity, tool-call auditing, and result-size limits in `KubeUI.AI`; keep ASP.NET hosting and HTTP security in `KubeUI.Desktop`; keep cluster and UI integrations in `KubeUI.Avalonia`.
- Keep MCP stateless. Never use Host, Origin, forwarded headers, or an MCP session identifier as authentication.
- Cache-backed listing pages use opaque keyset cursors bound to the cluster and query. Recheck client authorization on every page; do not invalidate cursors for informer updates. Document that pages read the live cache and may reflect changes between calls.
- Keep listing tool results complete according to each tool's contract, and return requested object fields in the list response so clients do not need follow-up reads for data the list tool promises.
- Update the external MCP guide, localization, behavior tests, and this file when changing MCP tools, auth, grants, or Settings controls.
