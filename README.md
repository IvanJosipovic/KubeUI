# KubeUI

![cicd](https://github.com/IvanJosipovic/KubeUI/workflows/CICD/badge.svg)
[![codecov](https://codecov.io/gh/IvanJosipovic/KubeUI/branch/alpha/graph/badge.svg?token=E05HWW1QYR)](https://codecov.io/gh/IvanJosipovic/KubeUI)
![GitHub all releases](https://img.shields.io/github/downloads/IvanJosipovic/KubeUI/total)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FIvanJosipovic%2FKubeUI.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FIvanJosipovic%2FKubeUI?ref=badge_shield)

## What is this?

KubeUI is a user interface for Kubernetes.

## Features
- Automatic updates
- Connect to Cluster
- Create/View/Edit Resources as Yaml
- Custom Resource Definition support
- Multi Monitor Support
- Limited Permission Support
  - No access to List/Watch Namespaces
  - Namespace specific Resource Permissions

### Resource Specific Features
- Node
  - Codon/UnCordon
  - Drain
- Pod
  - View Logs
  - View Console
  - Port Forwarding
  - CPU/Memory Usage
- Secret
  - View Certificate details (expiry etc)


## How to run?

Go to [Releases](https://github.com/IvanJosipovic/KubeUI/releases) and download the version for your OS.

Supported Platforms (AMD64 and ARM64):

- Linux
- Mac ([*Note Issue](https://github.com/IvanJosipovic/KubeUI/issues/688))
- Windows

## Example

![screenshot](docs/Screenshot.png)

## How to build?

1. [Download .Net SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
2. [Download an IDE](https://dotnet.microsoft.com/platform/tools)
3. Build away!

## Stats

![Alt](https://repobeats.axiom.co/api/embed/db926eb668f71f8de3314f03022de6bb35797d5d.svg "Repobeats analytics image")


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FIvanJosipovic%2FKubeUI.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FIvanJosipovic%2FKubeUI?ref=badge_large)
