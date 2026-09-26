---
sidebar_position: 7
title: Pod logs
---

# Follow Pod logs

Open logs directly from Pods, Deployments, ReplicaSets, StatefulSets, DaemonSets, Jobs, and CronJobs. A Pod view follows that Pod; a controller view streams logs from its matching Pods. Start with one Pod, then move up its owner chain. If a Pod restarts or is replaced, KubeUI reconnects to its current log stream.

<video className="theme-video theme-video--light" controls preload="none" width="100%">
  <source src="/video/pod-logs-light.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

<video className="theme-video theme-video--dark" controls preload="none" width="100%">
  <source src="/video/pod-logs-dark.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

## Open a Pod log stream

1. Connect to a cluster and select the namespace containing the Pod.
2. Open **Pods**, right-click the Pod, and choose **View Logs** > **Open New Logs View**.
3. Choose the Pod's container in the sources selector if you want to limit which container streams appear.

The selected Pod's logs stream into the view as they arrive. You need permission to read Pod logs in the selected namespace.

## Follow logs up the controller chain

Use the upward-arrow controller action in the logs toolbar to change scope:

- **Pod:** stream logs for the selected Pod.
- **ReplicaSet:** include logs from Pods owned by that ReplicaSet.
- **Deployment:** include logs from Pods managed by that Deployment.

The source selector lets you include or exclude individual Pod and container streams. When viewing a controller, resource names can distinguish output from different Pods.

Controller scope keeps the view useful as Pods restart or get replaced: KubeUI reconnects to the current streams for Pods in scope.
