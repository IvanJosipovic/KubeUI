# KubeUI.Avalonia engineering guide

This project owns the shared Avalonia foundation: framework dependencies, shared assets, resources, and the public Avalonia markup extensions.

- Do not reference `KubeUI.App`, `KubeUI.Controls`, `KubeUI.Features.*`, or `KubeUI.Kubernetes`.
- Keep feature-specific controls, views, view models, and services in their owning projects.
- Keep the root Avalonia markup extension provider public with `[assembly: GenerateMarkupExtensionsForAvalonia(true)]`.
- Keep shared asset URIs rooted at `avares://KubeUI.Avalonia`.
