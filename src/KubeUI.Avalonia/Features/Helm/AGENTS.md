# Helm Feature

- Helm integrations live under `Features/Helm/`; Kubernetes-layer contracts and Helm client operations live in `KubeUI.Kubernetes/Helm/`.
- Use `Helm.NET` for Helm actions; never launch the Helm executable.
- Run synchronous native calls off the UI thread and serialize native Helm calls because the shim and default repository configuration are process-wide.
- Helm navigation owns the `Helm` section with `Repositories` and `Releases` documents; Helm data is not a Kubernetes `ResourceConfig`.
- Repository forms must include name, URL, and optional basic-auth credentials. Repository rows support refresh, index update, add, edit, and delete.
- Release rows support namespace-scoped or all-namespace listing, install, upgrade, status, values, history, rollback to a chosen revision, uninstall, and chart template preview.
- Keep Helm UI text in Resources.resx and cover service, navigation, and ViewModel behavior with tests.

## Helm release visualization preview

- Offer **Preview in Visualization** from both the release editor and the release-list context menu. Render the chart with Helm.NET using the release's chart and values, then open the existing Resource Visualization view with a read-only preview of every resource Helm would deploy.
- Parse the rendered multi-document manifest into Kubernetes resource objects and use the existing `IResourceRelationshipBuilder`/visualization graph pipeline to show relationships among the rendered resources.
- Keep preview resources isolated from the live cluster resource store and informer feed: preview must not apply, seed, or mutate Kubernetes resources. Preserve visualization filters and make unresolved references to resources outside the rendered release visible.
- Test manifest parsing, preview-resource isolation, relationship construction, editor/list preview actions, and error handling for Helm rendering or invalid manifests.
