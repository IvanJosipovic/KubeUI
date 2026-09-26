---
sidebar_position: 8
description: Troubleshoot common KubeUI issues with cluster connections, resource access, and Kubernetes workflows.
---

# Troubleshooting

## Cluster does not appear

Use **File → Load KubeConfig** and select the file that contains the cluster. Confirm that the intended context is present in that file and that the same context works with your Kubernetes command-line setup.

## Cluster is present but resources are missing

Check that the cluster is connected and that the active identity has permission to list the resource type. Limited namespace or resource permissions can make parts of the navigation unavailable.

## YAML dry run or save fails

Read the API server validation message in KubeUI, correct the manifest, and run the dry run again. Admission policies and schema versions can differ between clusters.

## Metrics are blank

The overview and pod metrics need metrics data from the cluster. Check whether the cluster has a metrics provider and whether your identity can read the required metrics.

## Report a problem

Include the KubeUI version, operating system, and the action that failed. Remove credentials, tokens, private host names, and other sensitive values before posting logs or manifests to [GitHub Issues](https://github.com/IvanJosipovic/KubeUI/issues).
