# Resource Editor Requirements

- Build editor hierarchy from Kubernetes OpenAPI schema and preserve fields absent from schema.
- Render nested objects, arrays, and maps as recursively collapsible sections.
- Use typed scalar editors for strings, numbers, booleans, and enums.
- Arrays and maps must support add and remove operations without raw JSON editing.
- Show schema descriptions, required markers, and validation errors near affected fields.
- Keep edits in a draft JSON tree until Save; Reset restores original resource.
- Add pure ViewModel tests and Avalonia Headless rendering tests for each editor type.

## Schema handling

- The schema and document model lives in `KubeUI.Kubernetes` under `Resources/Editor/`. Keep it UI-free.
- `allOf` branches are conjunctions: merge their properties and required names.
- `oneOf` and `anyOf` branches are alternatives: offer their properties, but do not merge their
  required names, because only one branch may be satisfied at a time.
- Required names are matched to editor nodes by the editor tree's own path format (`a.b`, `a[0]`).
  Keep validation paths and node paths in sync.

## Editing state

- The editor tree holds references to the JSON nodes it renders. Rebuild the tree whenever the
  document is replaced, or later edits will mutate a discarded document.
- Nodes detached from the tree must not be able to write back into the document.
- YAML-backed string fields store plain text. Do not JSON-encode the parsed scalar.

## Saving

- Apply the resource as a single object so the server-assigned UID and resource version reach the
  document. A second save must replace the resource rather than create it again.
- Report save failures and `409` conflicts through the action result bar; never write an error
  only to a property that no control binds.
