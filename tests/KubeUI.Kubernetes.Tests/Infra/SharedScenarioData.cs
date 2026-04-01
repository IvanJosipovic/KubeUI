namespace KubeUI.Kubernetes.Tests.Infra;

internal static class SharedScenarioData
{
    public const string CustomResourceDefinitionYaml = """
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata:
  name: tests.kubeui.com
spec:
  group: kubeui.com
  names:
    plural: tests
    singular: test
    kind: Test
    listKind: TestList
  scope: Namespaced
  versions:
    - name: v1beta1
      served: true
      storage: true
      schema:
        openAPIV3Schema:
          type: object
          properties:
            apiVersion:
              type: string
            kind:
              type: string
            metadata:
              type: object
            spec:
              type: object
              properties:
                someString:
                  type: string
""";

    public const string CustomResourceYaml = """
apiVersion: kubeui.com/v1beta1
kind: Test
metadata:
  name: test1
  namespace: default
spec:
  someString: myValue
""";

    public const string LimitedAccessYaml = """
---
apiVersion: v1
kind: Namespace
metadata:
  name: my-app
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: my-serviceaccount
  namespace: my-app
secrets:
  - name: my-serviceaccount
---
apiVersion: v1
kind: Secret
metadata:
  name: my-serviceaccount
  namespace: my-app
  annotations:
    kubernetes.io/service-account.name: my-serviceaccount
type: kubernetes.io/service-account-token
data:
  token: ZmFrZS10b2tlbg==
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: my-serviceaccount
rules: []
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-additional
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: my-serviceaccount
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-viewer
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: viewer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: my-serviceaccount-developer
  namespace: my-app
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: developer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: developer
rules:
- verbs:
    - get
    - list
    - watch
  apiGroups:
    - '*'
  resources:
    - '*'

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - pods/attach
    - pods/exec
    - pods/portforward
    - pods/proxy

- verbs:
    - create
  apiGroups:
    - 'batch'
  resources:
    - job

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - services/proxy

- verbs:
    - delete
  apiGroups:
    - ''
  resources:
    - pods
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: viewer
rules:
- apiGroups:
    - ''
  resources:
    - namespaces
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - ''
  resources:
    - nodes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - apiextensions.k8s.io
  resources:
    - customresourcedefinitions
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - storage.k8s.io
  resources:
    - storageclasses
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - ''
  resources:
    - persistentvolumes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - metrics.k8s.io
  resources:
    - nodes
    - pods
  verbs:
    - get
    - list

- verbs:
    - get
    - list
    - watch
  nonResourceURLs:
    - '*'
""";

    public const string LimitedAccessNoNamespaceYaml = """
---
apiVersion: v1
kind: Namespace
metadata:
  name: my-app
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: my-serviceaccount
  namespace: my-app
secrets:
  - name: my-serviceaccount
---
apiVersion: v1
kind: Secret
metadata:
  name: my-serviceaccount
  namespace: my-app
  annotations:
    kubernetes.io/service-account.name: my-serviceaccount
type: kubernetes.io/service-account-token
data:
  token: ZmFrZS10b2tlbg==
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: my-serviceaccount
rules: []
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-additional
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: my-serviceaccount
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-viewer
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: viewer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: my-serviceaccount-developer
  namespace: my-app
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: developer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: developer
rules:
- verbs:
    - get
    - list
    - watch
  apiGroups:
    - '*'
  resources:
    - '*'

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - pods/attach
    - pods/exec
    - pods/portforward
    - pods/proxy

- verbs:
    - create
  apiGroups:
    - 'batch'
  resources:
    - job

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - services/proxy

- verbs:
    - delete
  apiGroups:
    - ''
  resources:
    - pods
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: viewer
rules:
- apiGroups:
    - ''
  resources:
    - nodes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - apiextensions.k8s.io
  resources:
    - customresourcedefinitions
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - storage.k8s.io
  resources:
    - storageclasses
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - ''
  resources:
    - persistentvolumes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - metrics.k8s.io
  resources:
    - nodes
    - pods
  verbs:
    - get
    - list

- verbs:
    - get
    - list
    - watch
  nonResourceURLs:
    - '*'
""";
}
