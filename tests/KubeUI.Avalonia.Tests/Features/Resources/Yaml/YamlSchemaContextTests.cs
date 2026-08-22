using AvaloniaEdit.Document;
using k8s.Models;
using Microsoft.OpenApi;
using KubernetesClient.Informer.Client;
using Shouldly;
using System.Text.Json.Nodes;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public class YamlSchemaContextTests
{
    private static readonly ClusterModelCatalog s_modelCache = CreateModelCache();

    [Fact]
    public void Resolve_UsesJsonPropertyNameForRootCompletions()
    {
        var document = new TextDocument("met");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("Pod");
        context.CompletionItems.Select(item => item.Text).ShouldContain("metadata");
        context.CompletionItems.Select(item => item.Text).ShouldContain("apiVersion");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForCurrentProperty()
    {
        var document = new TextDocument("spec:");

        var context = YamlSchemaContext.Resolve(document, 2, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Specification of the desired behavior of the pod.");
        context.Documentation.TypeName.ShouldBe("object");
    }

    [Fact]
    public void Resolve_MapsPodSpecAndStatusReferencesToObjects()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
            status:
            """);

        var specOffset = document.Text.IndexOf("spec", StringComparison.Ordinal) + 2;
        var statusOffset = document.Text.IndexOf("status", StringComparison.Ordinal) + 2;

        var spec = YamlSchemaContext.Resolve(document, specOffset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);
        var status = YamlSchemaContext.Resolve(document, statusOffset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        spec.Documentation.ShouldNotBeNull();
        spec.Documentation.TypeName.ShouldBe("object");
        status.Documentation.ShouldNotBeNull();
        status.Documentation.TypeName.ShouldBe("object");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForMetadataNameAndNamespace()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              name: pod
              namespace: default
            """);

        var nameOffset = document.Text.IndexOf("name", StringComparison.Ordinal) + 2;
        var nameContext = YamlSchemaContext.Resolve(document, nameOffset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);
        nameContext.Documentation.ShouldNotBeNull();
        nameContext.Documentation.Label.ShouldBe("name");

        var namespaceOffset = document.Text.IndexOf("namespace", StringComparison.Ordinal) + 2;
        var namespaceContext = YamlSchemaContext.Resolve(document, namespaceOffset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);
        namespaceContext.Documentation.ShouldNotBeNull();
        namespaceContext.Documentation.Label.ShouldBe("namespace");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForContainerFieldsAfterEnvironmentSequence()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
              - env:
                - name: ENVIRONMENT
                  value: production
                image: example/image:latest
                imagePullPolicy: IfNotPresent
                name: app
            """);

        var policy = YamlSchemaContext.Resolve(
            document,
            document.Text.IndexOf("imagePullPolicy", StringComparison.Ordinal) + 2,
            GroupApiVersionKind.From<V1Pod>(),
            s_modelCache);
        var name = YamlSchemaContext.Resolve(
            document,
            document.Text.LastIndexOf("name: app", StringComparison.Ordinal) + 2,
            GroupApiVersionKind.From<V1Pod>(),
            s_modelCache);

        policy.ContainerType.Name.ShouldBe("containers");
        policy.Documentation.ShouldNotBeNull();
        policy.Documentation.Label.ShouldBe("imagePullPolicy");
        name.Documentation.ShouldNotBeNull();
        name.Documentation.Label.ShouldBe("name");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationAfterNestedManagedFields()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              managedFields:
              - fieldsV1:
                  f:metadata:
                    f:annotations:
                      .: {}
                  f:spec:
                    f:containers:
                      k:{"name":"alloy"}: {}
                manager: kubelet
                operation: Update
              name: alloy
            spec:
              containers:
              - image: example/alloy:latest
                imagePullPolicy: IfNotPresent
                name: alloy
            """);

        var policy = YamlSchemaContext.Resolve(
            document,
            document.Text.IndexOf("imagePullPolicy", StringComparison.Ordinal) + 2,
            GroupApiVersionKind.From<V1Pod>(),
            s_modelCache);
        var name = YamlSchemaContext.Resolve(
            document,
            document.Text.LastIndexOf("name: alloy", StringComparison.Ordinal) + 2,
            GroupApiVersionKind.From<V1Pod>(),
            s_modelCache);

        policy.Documentation.ShouldNotBeNull();
        policy.Documentation.Label.ShouldBe("imagePullPolicy");
        name.Documentation.ShouldNotBeNull();
        name.Documentation.Label.ShouldBe("name");
    }

    [Fact]
    public void Resolve_UsesOpenApiPropertyDescription()
    {
        var cache = CreateModelCacheWithOpenApi("Widget desired state.");

        var document = new TextDocument("spec:");
        var context = YamlSchemaContext.Resolve(document, 2, new GroupApiVersionKind(string.Empty, "v1", "TestYamlDocRoot", string.Empty), cache);

        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Widget desired state.");
    }

    public void Resolve_NormalizesOpenApiDocumentationWhitespace()
    {
        var cache = CreateModelCacheWithOpenApi("An opaque value that represents the internal version\n of this object.");

        var document = new TextDocument("spec:");
        var context = YamlSchemaContext.Resolve(document, 2, new GroupApiVersionKind(string.Empty, "v1", "TestYamlDocRoot", string.Empty), cache);

        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("An opaque value that represents the internal version of this object.");
    }

    [Fact]
    public void Resolve_ReturnsSequenceItemSuggestionsForNestedCollections()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - na
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.CompletionItems.Select(item => item.Text).ShouldContain("name");
        context.CompletionItems.Select(item => item.Text).ShouldContain("image");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForCollectionItemField()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - name: demo
            """);

        var offset = document.Text.LastIndexOf("name", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Name of the container");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForPodContainersProperty()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
            """);

        var offset = document.Text.IndexOf("containers", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.Documentation.ShouldNotBeNull();
        context.Documentation.Label.ShouldBe("containers");
        context.Documentation.TypeName.ShouldBe("array");
    }

    [Fact]
    public void Resolve_OffersCompletionsUnderPodAffinityTerm()
    {
        var document = new TextDocument(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "spec:\n"
            + "  affinity:\n"
            + "    podAffinity:\n"
            + "      preferredDuringSchedulingIgnoredDuringExecution:\n"
            + "        - weight: 2\n"
            + "          podAffinityTerm:\n"
            + "            ");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("podAffinityTerm");
        context.CompletionItems.Select(item => item.Text).ShouldContain("labelSelector");
    }

    [Fact]
    public void Resolve_OffersEnumCompletionsForNestedMatchExpressionOperator()
    {
        var document = new TextDocument(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "spec:\n"
            + "  affinity:\n"
            + "    podAffinity:\n"
            + "      preferredDuringSchedulingIgnoredDuringExecution:\n"
            + "        - weight: 1\n"
            + "          podAffinityTerm:\n"
            + "            labelSelector:\n"
            + "              matchExpressions:\n"
            + "                - operator: ");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.CompletionItems.Select(item => item.Text).ShouldBe(
            ["In", "NotIn", "Exists", "DoesNotExist"]);
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForContainerImagePullPolicyField()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - imagePullPolicy: Always
            """);

        var offset = document.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Image pull policy");
    }

    [Fact]
    public void Resolve_ReturnsCompletionItemsForEnumValuesAtValuePosition()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - imagePullPolicy: I
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.Key.Prefix.ShouldBe("I");
        context.CompletionItems.Select(item => item.Text)
            .ShouldBe(["Always", "IfNotPresent", "Never"]);
        context.CompletionItems.Select(item => item.InsertionText)
            .ShouldBe(["Always", "IfNotPresent", "Never"]);
    }

    [Fact]
    public void Resolve_MapsEveryOpenApiSchemaType()
    {
        var cache = new ClusterModelCatalog(new KubernetesModelCatalog());
        cache.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.TypeOptionsRoot"] = new OpenApiSchema
                    {
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["nullValue"] = new OpenApiSchema { Type = JsonSchemaType.Null },
                            ["booleanValue"] = new OpenApiSchema { Type = JsonSchemaType.Boolean },
                            ["integerValue"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
                            ["numberValue"] = new OpenApiSchema { Type = JsonSchemaType.Number },
                            ["stringValue"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["objectValue"] = new OpenApiSchema { Type = JsonSchemaType.Object },
                            ["arrayValue"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchema { Type = JsonSchemaType.String },
                            },
                        },
                    },
                },
            },
        });

        var expected = new Dictionary<string, string>
        {
            ["nullValue"] = "null",
            ["booleanValue"] = "boolean",
            ["integerValue"] = "integer",
            ["numberValue"] = "number",
            ["stringValue"] = "string",
            ["objectValue"] = "object",
            ["arrayValue"] = "array",
        };

        foreach (var pair in expected)
        {
            var document = new TextDocument($"{pair.Key}:");
            var context = YamlSchemaContext.Resolve(
                document,
                pair.Key.Length / 2,
                new GroupApiVersionKind(string.Empty, "v1", "TypeOptionsRoot", string.Empty),
                cache);

            context.Documentation.ShouldNotBeNull();
            context.Documentation.TypeName.ShouldBe(pair.Value);
        }
    }

    [Fact]
    public void Resolve_TreatsComposedObjectWithPrimitiveWrapperTypeAsObject()
    {
        var cache = new ClusterModelCatalog(new KubernetesModelCatalog());
        cache.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.WrappedRoot"] = new OpenApiSchema
                    {
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["spec"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.String,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["containers"] = new OpenApiSchema { Type = JsonSchemaType.Array },
                                },
                            },
                        },
                    },
                },
            },
        });

        var document = new TextDocument("spec:");
        var specContext = YamlSchemaContext.Resolve(
            document,
            2,
            new GroupApiVersionKind(string.Empty, "v1", "WrappedRoot", string.Empty),
            cache);

        specContext.Documentation.ShouldNotBeNull();
        specContext.Documentation.TypeName.ShouldBe("object");

        document.Text = "spec:\n  ";
        var nestedContext = YamlSchemaContext.Resolve(
            document,
            document.TextLength,
            new GroupApiVersionKind(string.Empty, "v1", "WrappedRoot", string.Empty),
            cache);
        nestedContext.CompletionItems.Select(item => item.Text).ShouldContain("containers");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForImagePullPolicyInLargePodManifest()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 62f8bc4d09674a1a1f17adea4d3d7e2b4a25d654a3e56580564b67b165a59497
                cni.projectcalico.org/podIP: 10.1.43.163/32
                cni.projectcalico.org/podIPs: 10.1.43.163/32
                kubectl.kubernetes.io/restartedAt: 2025-12-05T19:58:57Z
              creationTimestamp: "2026-01-25T08:11:42Z"
              generateName: test-9f4855bcc-
              generation: 1
              labels:
                app.kubernetes.io/instance: test
                pod-template-hash: 9f4855bcc
              name: test-9f4855bcc-v9h7j
              namespace: test
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: test-9f4855bcc
                uid: e068e727-f25c-4316-94d4-0d8212b0f50f
              resourceVersion: "778397528"
              uid: 362b089e-e215-4f15-a9ff-512f566f5e4d
            spec:
              containers:
              - env:
                - name: SECURE_CONNECTION
                  value: "1"
                - name: VNC_PASSWORD
                  valueFrom:
                    secretKeyRef:
                      key: asdf
                      name: test
                - name: GROUP_ID
                  value: "1010"
                - name: FORCE_LATEST_UPDATE
                  value: "true"
                image: test/test@sha256:fffffffffffffffffffffffffffffffffffff
                imagePullPolicy: IfNotPresent
                name: test
            """);

        var offset = document.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Image pull policy");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForMetadataNameAndNamespaceAfterManagedFields()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: ConfigMap
            metadata:
              annotations:
                meta.helm.sh/release-name: cloudnative-pg
              managedFields:
              - apiVersion: v1
                fieldsType: FieldsV1
                fieldsV1:
                  f:metadata:
                    f:annotations:
                      .: {}
              name: cnpg-controller-manager-config
              namespace: cnpg-system
            """);

        var nameOffset = document.Text.LastIndexOf("name: cnpg", StringComparison.Ordinal) + 1;
        var namespaceOffset = document.Text.LastIndexOf("namespace: cnpg", StringComparison.Ordinal) + 1;

        var nameContext = YamlSchemaContext.Resolve(document, nameOffset, GroupApiVersionKind.From<V1ConfigMap>(), s_modelCache);
        var namespaceContext = YamlSchemaContext.Resolve(document, namespaceOffset, GroupApiVersionKind.From<V1ConfigMap>(), s_modelCache);

        nameContext.ContainerType.Name.ShouldBe("metadata");
        nameContext.Documentation.ShouldNotBeNull();
        nameContext.Documentation.Label.ShouldBe("name");
        namespaceContext.Documentation.ShouldNotBeNull();
        namespaceContext.Documentation.Label.ShouldBe("namespace");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForLatePodSpecFieldInLargePodManifest()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: fa2328c666789a14eecd7a5ad558b972b510008d547a5d745bd10ccf00e16fb0
                cni.projectcalico.org/podIP: 10.1.43.176/32
                cni.projectcalico.org/podIPs: 10.1.43.176/32
                kubectl.kubernetes.io/default-container: alertmanager
                kubectl.kubernetes.io/restartedAt: 2024-12-21T11:27:54Z
              creationTimestamp: "2025-12-18T03:18:16Z"
              generateName: alertmanager-prometheus-kube-prometheus-alertmanager-
              generation: 1
              labels:
                alertmanager: prometheus-kube-prometheus-alertmanager
                app.kubernetes.io/instance: prometheus-kube-prometheus-alertmanager
                app.kubernetes.io/managed-by: prometheus-operator
                app.kubernetes.io/name: alertmanager
                app.kubernetes.io/version: 0.27.0
                apps.kubernetes.io/pod-index: "0"
                controller-revision-hash: alertmanager-prometheus-kube-prometheus-alertmanager-7bfd55984
                statefulset.kubernetes.io/pod-name: alertmanager-prometheus-kube-prometheus-alertmanager-0
              name: alertmanager-prometheus-kube-prometheus-alertmanager-0
              namespace: monitoring
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: StatefulSet
                name: alertmanager-prometheus-kube-prometheus-alertmanager
                uid: b8a36710-6e1d-4391-b059-e2cf435acc99
              resourceVersion: "801283915"
              uid: 2aeb93fe-692d-41e1-a62c-69fccb4fceef
            spec:
              containers:
              - args:
                - --config.file=/etc/alertmanager/config_out/alertmanager.env.yaml
                - --storage.path=/alertmanager
                - --data.retention=120h
                - --cluster.listen-address=
                - --web.listen-address=:9093
                - --web.external-url=http://prometheus-kube-prometheus-alertmanager.monitoring:9093
                - --web.route-prefix=/
                - --cluster.label=monitoring/prometheus-kube-prometheus-alertmanager
                - --cluster.peer=alertmanager-prometheus-kube-prometheus-alertmanager-0.alertmanager-operated:9094
                - --cluster.reconnect-timeout=5m
                - --web.config.file=/etc/alertmanager/web_config/web-config.yaml
                env:
                - name: POD_IP
                  valueFrom:
                    fieldRef:
                      apiVersion: v1
                      fieldPath: status.podIP
                image: quay.io/prometheus/alertmanager:v0.27.0
                imagePullPolicy: IfNotPresent
                name: alertmanager
              serviceAccountName: prometheus-kube-prometheus-alertmanager
            """);

        var offset = document.Text.LastIndexOf("serviceAccountName", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("spec");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("ServiceAccountName");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForImagePullPolicyInCalicoControllerManifest()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """);

        var offset = document.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Image pull policy");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForImagePullPolicyInCalicoControllerManifestWithoutTrailingNewline()
    {
        var document = new TextDocument((
            """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """).TrimEnd('\r', '\n'));

        var offset = document.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Image pull policy");
    }

    [Fact]
    public void Resolve_KeepsCollectionItemScopeAfterScalarCollectionField()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              name: ubuntu-sleep-deployment-566b5954cf-pvd57
              namespace: default
            spec:
              containers:
              - command:
                - sleep
                - infinity
                image: ubuntu:latest
                imagePullPolicy: Always
                name: ubuntu-sleep
            """);

        var offset = document.Text.LastIndexOf("name:", StringComparison.Ordinal) + 2;
        var context = YamlSchemaContext.Resolve(document, offset, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.Documentation.ShouldNotBeNull();
        context.Documentation.PropertySummary.ShouldBe("Name of the container");
    }

    [Fact]
    public void Resolve_DoesNotOfferCompletionsWhileTypingAValue()
    {
        var document = new TextDocument("metadata: default");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.CompletionItems.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_OffersCompletionsOnIndentedBlankLine()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("spec");
        context.CompletionItems.Select(item => item.Text).ShouldContain("containers");
        context.Key.StartOffset.ShouldBe(document.TextLength);
        context.Key.EndOffset.ShouldBe(document.TextLength);
        context.Key.Prefix.ShouldBe(string.Empty);
    }

    [Fact]
    public void Resolve_OffersNestedCompletionsOnBlankLineWithoutIndentation()
    {
        var document = new TextDocument(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "spec:\n");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("spec");
        context.CompletionItems.Select(item => item.Text).ShouldContain("containers");
    }

    [Fact]
    public void Resolve_OffersRootCompletionsAfterCompletedMetadata()
    {
        var document = new TextDocument(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "\n");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("Pod");
        context.CompletionItems.Select(item => item.Text).ShouldContain("spec");
    }

    [Fact]
    public void Resolve_OffersCompletionsForBlankSequenceEntry()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                -
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.CompletionItems.Select(item => item.Text).ShouldContain("name");
        context.Key.StartOffset.ShouldBe(document.TextLength);
        context.Key.EndOffset.ShouldBe(document.TextLength);
        context.Key.Prefix.ShouldBe(string.Empty);
    }

    [Fact]
    public void Resolve_DoesNotOfferCompletionsForScalarSequenceEntryUnderNestedProperty()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - command:
                  -
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.TypeName.ShouldBe("string");
        context.CompletionItems.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_DoesNotOfferCompletionsWhileTypingScalarSequenceEntryUnderNestedProperty()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - command:
                  - sl
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.TypeName.ShouldBe("string");
        context.CompletionItems.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_FiltersAlreadyUsedRootKeysFromCompletions()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
              
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("Pod");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("apiVersion");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("kind");
        context.CompletionItems.Select(item => item.Text).ShouldContain("metadata");
    }

    [Fact]
    public void Resolve_FiltersAlreadyUsedNestedKeysFromCompletions()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - name: demo
                  image: nginx
                  
            """);

        var context = YamlSchemaContext.Resolve(document, document.TextLength - 1, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("name");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("image");
        context.CompletionItems.Select(item => item.Text).ShouldContain("ports");
    }

    [Fact]
    public void Resolve_FiltersFieldsDeclaredBelowCurrentCaretFromCompletions()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            
            metadata:
              name: demo
            """);

        var context = YamlSchemaContext.Resolve(document, document.Text.IndexOf('\n', document.Text.IndexOf("kind: Pod", StringComparison.Ordinal)) + 1, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("Pod");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("metadata");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("apiVersion");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("kind");
    }

    [Fact]
    public void Resolve_FiltersScalarFieldsDeclaredBelowCurrentCaretFromNestedCompletions()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
              name: ubuntu-sleep-deployment-566b5954cf-pvd57
              namespace: default
            spec:
              containers:
              - command:
                - sleep
                - infinity
                image: ubuntu:latest
                
                imagePullPolicy: Always
                name: ubuntu-sleep
            """);

        var context = YamlSchemaContext.Resolve(document, document.Text.IndexOf("\n    imagePullPolicy", StringComparison.Ordinal) + 1, GroupApiVersionKind.From<V1Pod>(), s_modelCache);

        context.ContainerType.Name.ShouldBe("containers");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("image");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("imagePullPolicy");
        context.CompletionItems.Select(item => item.Text).ShouldNotContain("name");
    }

    [Fact]
    public void TryCreateSequenceEntryInsertion_ReturnsDashPrefixForSequenceProperty()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
            """);

        var result = YamlSchemaContext.TryCreateSequenceEntryInsertion(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache, out var insertionText);

        result.ShouldBeTrue();
        insertionText.ShouldBe("\n    - ");
    }

    [Fact]
    public void TryCreateSequenceEntryInsertion_AlignsNestedSequenceUnderSequenceItemProperty()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - command:
            """);

        var result = YamlSchemaContext.TryCreateSequenceEntryInsertion(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache, out var insertionText);

        result.ShouldBeTrue();
        insertionText.ShouldBe("\n        - ");
    }

    [Fact]
    public void TryCreateSequenceEntryInsertion_ReturnsFalseForObjectProperty()
    {
        var document = new TextDocument(
            """
            apiVersion: v1
            kind: Pod
            metadata:
            """);

        var result = YamlSchemaContext.TryCreateSequenceEntryInsertion(document, document.TextLength, GroupApiVersionKind.From<V1Pod>(), s_modelCache, out var insertionText);

        result.ShouldBeFalse();
        insertionText.ShouldBeEmpty();
    }

    private static ClusterModelCatalog CreateModelCache()
    {
        var cache = new ClusterModelCatalog(new KubernetesModelCatalog());
        var pod = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["apiVersion"] = new OpenApiSchema(),
                ["kind"] = new OpenApiSchema(),
                ["metadata"] = Schema(null, ("name", null), ("namespace", null)),
                ["spec"] = new OpenApiSchema(),
            },
        };
        var podSpec = Schema(null, ("containers", null), ("serviceAccountName", "ServiceAccountName"));
        var affinity = Schema(null, ("podAffinity", null));
        var podAffinity = Schema(null, ("preferredDuringSchedulingIgnoredDuringExecution", null));
        var weightedPodAffinityTerm = Schema(null, ("weight", null), ("podAffinityTerm", null));
        var podAffinityTerm = Schema(null, ("labelSelector", null), ("namespaceSelector", null), ("namespaces", null), ("topologyKey", null));
        var labelSelector = Schema(null, ("matchExpressions", null));
        var matchExpression = Schema(null, ("key", null), ("operator", null), ("values", null));
        matchExpression.Properties!["operator"] = new OpenApiSchema
        {
            Enum = [
                JsonValue.Create("In"),
                JsonValue.Create("NotIn"),
                JsonValue.Create("Exists"),
                JsonValue.Create("DoesNotExist")],
        };
        labelSelector.Properties!["matchExpressions"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = matchExpression,
        };
        affinity.Properties!["podAffinity"] = podAffinity;
        podAffinity.Properties!["preferredDuringSchedulingIgnoredDuringExecution"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = weightedPodAffinityTerm,
        };
        weightedPodAffinityTerm.Properties!["podAffinityTerm"] = podAffinityTerm;
        podAffinityTerm.Properties!["labelSelector"] = labelSelector;
        podSpec.Properties!["affinity"] = affinity;
        var container = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["command"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String },
                },
                ["name"] = new OpenApiSchema { Description = "Name of the container" },
                ["image"] = new OpenApiSchema(),
                ["imagePullPolicy"] = new OpenApiSchema
                {
                    Description = "Image pull policy",
                    Enum = [JsonValue.Create("Always"), JsonValue.Create("IfNotPresent"), JsonValue.Create("Never")],
                },
                ["ports"] = new OpenApiSchema { Type = JsonSchemaType.Array, Items = new OpenApiSchema { Type = JsonSchemaType.Object } },
            },
        };
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.Pod"] = pod,
                    ["io.k8s.api.core.v1.PodSpec"] = podSpec,
                    ["io.k8s.api.core.v1.PodStatus"] = Schema(null, ("phase", null)),
                    ["io.k8s.api.core.v1.Container"] = container,
                },
            },
        };
        pod.Properties!["spec"] = new OpenApiSchema
        {
            AllOf = [new OpenApiSchemaReference("io.k8s.api.core.v1.PodSpec", document)],
            Description = "Specification of the desired behavior of the pod.",
        };
        pod.Properties!["status"] = new OpenApiSchema
        {
            AllOf = [new OpenApiSchemaReference("io.k8s.api.core.v1.PodStatus", document)],
        };
        podSpec.Properties!["containers"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = new OpenApiSchemaReference("io.k8s.api.core.v1.Container", document),
        };
        document.RegisterComponents();
        cache.RegisterOpenApiSchema(document);
        return cache;
    }

    private static OpenApiSchema Schema(string? description, params (string Name, string? Description)[] properties)
    {
        return new OpenApiSchema
        {
            Description = description,
            Properties = properties.ToDictionary(
                property => property.Name,
                property => (IOpenApiSchema)new OpenApiSchema { Description = property.Description }),
        };
    }

    private static ClusterModelCatalog CreateModelCacheWithOpenApi(string? propertyDescription)
    {
        var cache = new ClusterModelCatalog(new KubernetesModelCatalog());
        cache.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.TestYamlDocRoot"] = new OpenApiSchema
                    {
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["spec"] = new OpenApiSchema { Description = propertyDescription },
                        },
                    },
                },
            },
        });
        return cache;
    }

}
