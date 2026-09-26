---
sidebar_position: 5
description: Visualize relationships and references between Kubernetes resources with KubeUI.
---

# Visualize resource relationships

KubeUI can build a graph of Kubernetes objects and their discovered references. Open **Visualize** from a resource's actions to inspect relationships starting from that object, or open visualization from the cluster workspace to explore a wider set.

Use the visualization filters to focus on resource types and namespaces. The graph reflects references KubeUI can resolve from the objects visible to your identity; missing permissions or unavailable objects can leave relationships unresolved.

Visualization is a companion to the resource tables: use it to understand connections, then return to a resource list or object details to inspect or edit a specific object.

<video className="theme-video theme-video--light" controls preload="none" width="100%">
  <source src="/video/resource-relationships-light.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

<video className="theme-video theme-video--dark" controls preload="none" width="100%">
  <source src="/video/resource-relationships-dark.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>
