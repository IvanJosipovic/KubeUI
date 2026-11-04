# Azure Feature

## Ownership
- Own Azure-specific authentication, management-plane integrations, and Azure service contracts.
- Keep cluster-management screens and workflows in `KubeUI.Features.Clusters`.
- Keep Azure SDK dependencies out of `KubeUI.Kubernetes`; depend on its Kubernetes serialization and runtime contracts as needed.

## Validation
- Test Azure service transformations without requiring a live Azure account or subscription.
- Preserve AKS discovery, credential selection, kubeconfig normalization, and import behavior.
