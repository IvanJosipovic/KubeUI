---
sidebar_position: 1
description: Install KubeUI, load your Kubernetes configuration, and connect to your first cluster.
---

# Get started

KubeUI is a desktop Kubernetes client for exploring clusters and carrying out common resource workflows.

Short feature videos appear in the guides beside the workflows they demonstrate.

## Install KubeUI

- **Windows:** `winget install KubeUI`
- **macOS:** `brew install --cask IvanJosipovic/homebrew-repo/kubeui`
- **Linux:** download the package for your architecture from [GitHub Releases](https://github.com/IvanJosipovic/KubeUI/releases/latest).

## Connect to your first cluster

KubeUI reads the same kubeconfig files used by Kubernetes command-line tools. You can load a file from the app and then connect to a cluster from the navigation pane.

1. Make sure you can reach the cluster from this computer. If you use `kubectl`, confirm the intended context with `kubectl config current-context`.
2. Open KubeUI and choose **File → Load KubeConfig**.
3. Select the kubeconfig file. KubeUI accepts multiple files in one selection.
4. Find the cluster in the navigation pane and select it to start the connection.
5. When connected, expand the cluster to browse available resource groups. Select a resource type, such as **Pods**, to open its list.

<video className="theme-video theme-video--light" controls preload="none" width="100%">
  <source src="/video/connect-to-cluster-light.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

<video className="theme-video theme-video--dark" controls preload="none" width="100%">
  <source src="/video/connect-to-cluster-dark.mp4" type='video/mp4; codecs="avc1.640028, mp4a.40.2"' />
  Your browser does not support embedded video.
</video>

## Next

- [Manage clusters and workspaces](./clusters-and-workspaces)
- [Browse and filter resources](./resource-browsing)
- [Edit YAML safely](./yaml-editor)
- [Explore the feature guide](./features)
