# KubeUI.Controls engineering guide

This project owns reusable Avalonia controls and UI contracts shared across features.

- Promote a feature control here only when another project needs the same behavior and API.
- Keep feature-specific controls in their owning `KubeUI.Features.*` project.
- Reference `KubeUI.Avalonia` for the shared foundation. Do not reference feature projects or Kubernetes runtime projects.
- Keep external markup-extension generation declarations public and assign each external assembly one owner.
- Document and test public control APIs. Keep control tests under `tests/KubeUI.Avalonia.Tests/Controls/`.
