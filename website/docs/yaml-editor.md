---
sidebar_position: 4
description: Inspect and edit Kubernetes YAML in KubeUI with completion, validation, and server-side dry runs.
---

# Edit YAML

KubeUI's YAML view is for inspecting and changing Kubernetes objects in place. Open it from a resource's actions or import YAML from the cluster navigation actions.

## Make a change

1. Open a resource's YAML view and choose **Edit**.
2. Change the manifest. The editor provides Kubernetes-aware completion and field documentation when schema information is available.
3. Review inline validation feedback and fix any reported issues.
4. Choose **Dry run** to ask the API server to validate the proposed change without saving it.
5. Choose **Save** to apply the edit after the dry run succeeds.

The editor also provides line numbers, word wrap controls, and YAML-oriented indentation and list editing behavior. The dry run is server-side, so it checks the manifest against the connected cluster's API and admission rules.

<video className="theme-video theme-video--light" controls preload="none" width="100%">
  <source src="/video/inspect-pod-yaml-light.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

<video className="theme-video theme-video--dark" controls preload="none" width="100%">
  <source src="/video/inspect-pod-yaml-dark.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

## Create or import YAML

KubeUI can import YAML into a connected cluster. Use the import action in the cluster navigation area, then review the result reported by the application. Treat imported manifests with the same care as changes made with `kubectl apply`.
