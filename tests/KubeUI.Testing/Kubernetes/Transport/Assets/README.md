# Kubernetes OpenAPI fixture

`pod-openapi-v1-schemas.json` contains the Pod schema and its transitive schema references, fetched from the default-namespace `microk8s` kubeconfig context on 2026-09-25. Cluster version was Kubernetes `v1.36.2`.

The snapshot came from the cluster's `/openapi/v3` index and `/openapi/v3/api/v1` document using `kubectl --context microk8s get --raw`. Keep the referenced schemas together because Pod's fields use local OpenAPI schema references.
