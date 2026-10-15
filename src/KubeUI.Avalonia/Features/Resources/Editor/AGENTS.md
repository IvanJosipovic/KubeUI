# Resource Editor Requirements

- Build editor hierarchy from Kubernetes OpenAPI schema and preserve fields absent from schema.
- Render nested objects, arrays, and maps as recursively collapsible sections.
- Use typed scalar editors for strings, numbers, booleans, and enums.
- Arrays and maps must support add and remove operations without raw JSON editing.
- Show schema descriptions, required markers, and validation errors near affected fields.
- Keep edits in a draft JSON tree until Save; Reset restores original resource.
- Add pure ViewModel tests and Avalonia Headless rendering tests for each editor type.
