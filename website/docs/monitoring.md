---
sidebar_position: 7
title: Monitoring
description: View Pod and Node CPU and memory history with Kubernetes Metrics Server or supported Prometheus providers, and configure metrics per cluster.
---

import ThemeVideo from '@site/src/components/ThemeVideo';

# Monitor cluster resources

KubeUI displays CPU and memory metrics for Pods and Nodes when a metrics backend is available. Pod lists show CPU and memory columns with recent usage history. Open a Pod's properties and expand **Metrics** for detailed charts; available charts depend on the selected backend and the data it returns.

<ThemeVideo name="monitoring" />

## Choose a metrics backend

Open **Cluster Settings** beneath the cluster in the navigation pane and find **Metrics**. Settings apply to that cluster.

| Metrics service | Behavior |
| --- | --- |
| **Auto** (default) | Try Prometheus first; use Kubernetes Metrics Server if Prometheus cannot initialize. |
| **Disabled** | Do not connect to a metrics backend. |
| **Kubernetes Metrics Server** | Use the cluster's `metrics.k8s.io` API for Pod and Node usage. |
| **Prometheus** | Use the chosen Prometheus provider. Selecting this explicitly does not fall back to Metrics Server. |

**Active Metrics Service** reports which backend is connected, or whether metrics are disabled or unavailable. If no metrics backend is available, metrics-dependent columns and charts may be absent.

## Configure Prometheus

Select **Prometheus** as **Metrics Service** to show the **Prometheus Provider** selector:

| Provider | How KubeUI connects |
| --- | --- |
| **Automatic detection** (default) | Try Prometheus Operator, OpenShift, Manual Service, External URL, then Azure Managed Prometheus. |
| **Prometheus Operator** | Discover a Prometheus service in the cluster. |
| **OpenShift** | Discover OpenShift's monitoring service. |
| **Manual Service** | Enter a service **name**, **namespace**, and **port**. Name and namespace cannot be blank; port must be greater than zero. |
| **External URL** | Enter the reachable Prometheus **URL**. |
| **Azure Managed Prometheus** | Select the Azure subscription and managed Prometheus workspace through Azure CLI. The workspace needs a query endpoint. |

For non-Azure providers, cluster settings also offer **Prometheus Direct URL**, **Prometheus Path Prefix**, **Use HTTPS**, and **Prometheus Bearer Token**. The service name, namespace, and port fields are available except when using External URL or Azure. Use the path prefix if the Prometheus API is served beneath a URL path; set HTTPS and the bearer token as needed for the endpoint. Azure instead shows **Refresh Azure subscriptions**, **Azure Subscription**, and **Azure Monitor Workspace**. The bearer token is **session-only**: KubeUI does not save it with cluster settings; enter it again in a later session. Other metrics settings are saved per cluster.

Provider discovery and successful connection still depend on endpoint availability and access permissions. For Metrics Server, the cluster must expose `metrics.k8s.io`, and your identity must be able to list both Pod and Node metrics. KubeUI samples Metrics Server every 30 seconds and keeps up to one hour of samples collected while connected; it cannot show history from before collection started.

## Read Pod metrics

1. Connect to a cluster with an active metrics service and open **Pods**.
2. Read the **CPU** and **Memory** columns for each Pod's usage and recent history. Values reflect the latest available sample for that Pod's containers.
3. Double-click a Pod or use its **View** context-menu action to open the properties pane on the right. Expand its **Metrics** section for time-series charts. Use the chart icons to switch between available metrics and the time-range selector to change the range.

With Prometheus, Pod metrics can include CPU and memory usage, requests, and limits, network receive/transmit, and filesystem usage/reads/writes. Charts without returned data are omitted. Metrics Server provides sampled CPU and memory usage history rather than the additional Prometheus series.
