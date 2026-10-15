---
sidebar_position: 3
description: Browse, search, filter, and inspect Kubernetes resources and custom resources in KubeUI.
---

import ThemeVideo from '@site/src/components/ThemeVideo';

# Browse and inspect resources

After a cluster connects, choose a resource type in its navigation tree. KubeUI opens a table for that Kubernetes resource and loads the objects permitted to the current identity.

## Find an object

Resource tables support sorting, filtering, and search. Use the namespace selection and table filters to narrow a large list, then select a row to open its resource details. The available columns and actions depend on the resource type.

KubeUI includes views for built-in Kubernetes resources and can discover custom resources from installed CRDs. A resource that your identity cannot list may be absent even while other resource types remain available.

<ThemeVideo name="browse-pods" />

## Inspect and open YAML

Use the resource actions to open an object's properties or YAML view. The YAML view shows the manifest and offers edit actions; see [YAML editor](./yaml-editor) for the edit and validation workflow.

## Cluster overview

The cluster overview presents pod, CPU, and memory summaries along with cluster events. CPU and memory metrics depend on metrics being available from the cluster. Empty or partial charts can therefore reflect cluster data availability rather than a connection problem.

For Pod CPU and memory history in the resource list and properties pane, see [Monitoring](./monitoring).
