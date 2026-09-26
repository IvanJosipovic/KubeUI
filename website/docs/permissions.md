---
sidebar_position: 7
---

# Permissions and access

KubeUI uses the Kubernetes identity and permissions associated with the selected cluster context. It can work with reduced permissions: resource navigation is based on what the identity can access, and individual resource types may be unavailable without preventing access to others.

Some clusters restrict listing or watching namespaces. KubeUI has access-aware workflows for these cases, including namespace-scoped access. The objects and actions shown still depend on the permissions granted by the cluster administrator.

If expected resources are missing, check the active kubeconfig context and ask your cluster administrator to review the identity's Kubernetes RBAC rules. Do not assume that a missing row means the object does not exist.
