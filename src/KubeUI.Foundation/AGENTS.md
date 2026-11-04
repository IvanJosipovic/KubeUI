# KubeUI.Foundation engineering guide

This project owns the shared Avalonia foundation: framework dependencies, shared assets, resources, and the public Avalonia markup extensions.

- Do not reference `KubeUI.App`, `KubeUI.Controls`, `KubeUI.Features.*`, or `KubeUI.Kubernetes`.
- Keep feature-specific controls, views, view models, and services in their owning projects.
- Keep the root Avalonia markup extension provider public with `[assembly: GenerateMarkupExtensionsForAvalonia(true)]`.
- Keep shared asset URIs rooted at `avares://KubeUI.Foundation`.
- Shared localized strings live in `Assets/Resources.json` behind `Assets/LanguageManager` (`KubeUI.Foundation.Assets.LanguageManager`, the only public Lingua manager) and `KubeUI.Foundation.Localization`. This project's JSON holds only `Shared_` keys and text owned by Foundation types; view-owned keys live in the project that declares the owning view (the App shell has its own `Assets/Resources.json`). Feature resources live in each feature's `Assets/Resources.json` behind an internal manager.
- Bind localized text on live controls with `L10n.Instance.Key.ToBinding()`; use `CurrentValue()` only for structural strings, one-shot messages, comparisons, and URIs — every such call is a non-reactive site.
