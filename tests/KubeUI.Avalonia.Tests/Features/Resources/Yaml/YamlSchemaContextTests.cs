using System.Xml;
using System.Reflection;
using AvaloniaEdit.Document;
using k8s.Models;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Kubernetes;
using Shouldly;
using System.Text.Json.Serialization;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public class YamlSchemaContextTests
{
    private static readonly ModelCache s_modelCache = CreateModelCache();

    [Fact]
    public void Resolve_UsesJsonPropertyNameForRootCompletions()
    {
        var document = new TextDocument("met");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Pod));
        context.CompletionItems.Select(item => item.Text).ShouldContain("metadata");
        context.CompletionItems.Select(item => item.Text).ShouldContain("apiVersion");
    }

    [Fact]
    public void Resolve_ReturnsDocumentationForCurrentProperty()
    {
        var document = new TextDocument("spec:");

        var context = YamlSchemaContext.Resolve(document, 2, typeof(V1Pod), s_modelCache);

        context.CurrentProperty.ShouldNotBeNull();
        context.CurrentProperty.Name.ShouldBe(nameof(V1Pod.Spec));
        context.Documentation.ShouldNotBeNull();
        context.Documentation.DisplayText.ShouldContain("Specification of the desired behavior of the pod.");
        context.Documentation.DisplayText.ShouldContain(nameof(V1PodSpec));
        context.Documentation.TypeSummary.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_UsesOnlyPropertySummaryForFieldDocumentation()
    {
        var cache = CreateModelCacheWithXml(
            $$"""
            <doc>
              <assembly>
                <name>{{typeof(TestYamlDocRoot).Assembly.GetName().Name}}</name>
              </assembly>
              <members>
                <member name="P:{{GetXmlMemberTypeName(typeof(TestYamlDocRoot))}}.Spec">
                  <summary>Widget desired state.</summary>
                </member>
                <member name="T:{{GetXmlMemberTypeName(typeof(TestYamlDocSpec))}}">
                  <summary>Widget desired state.</summary>
                </member>
              </members>
            </doc>
            """,
            typeof(TestYamlDocRoot).Assembly);

        var document = new TextDocument("spec:");
        var context = YamlSchemaContext.Resolve(document, 2, typeof(TestYamlDocRoot), cache);

        context.Documentation.ShouldNotBeNull();
        CountOccurrences(context.Documentation.DisplayText, "Widget desired state.").ShouldBe(1);
        context.Documentation.TypeSummary.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_DoesNotIncludeTypeSummaryInFieldDocumentation()
    {
        var cache = CreateModelCacheWithXml(
            $$"""
            <doc>
              <assembly>
                <name>{{typeof(TestYamlDocRoot).Assembly.GetName().Name}}</name>
              </assembly>
              <members>
                <member name="P:{{GetXmlMemberTypeName(typeof(TestYamlDocRoot))}}.Spec">
                  <summary>Property summary.</summary>
                </member>
                <member name="T:{{GetXmlMemberTypeName(typeof(TestYamlDocSpec))}}">
                  <summary>Type summary.</summary>
                </member>
              </members>
            </doc>
            """,
            typeof(TestYamlDocRoot).Assembly);

        var document = new TextDocument("spec:");
        var context = YamlSchemaContext.Resolve(document, 2, typeof(TestYamlDocRoot), cache);

        context.Documentation.ShouldNotBeNull();
        CountOccurrences(context.Documentation.DisplayText, "Property summary.").ShouldBe(1);
        context.Documentation.DisplayText.ShouldNotContain("Type summary.");
        context.Documentation.TypeSummary.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_NormalizesXmlDocumentationWhitespace()
    {
        var cache = CreateModelCacheWithXml(
            $$"""
            <doc>
              <assembly>
                <name>{{typeof(TestYamlDocRoot).Assembly.GetName().Name}}</name>
              </assembly>
              <members>
                <member name="P:{{GetXmlMemberTypeName(typeof(TestYamlDocRoot))}}.Spec">
                  <summary>
                    An opaque value that represents the internal version
                    of this object.
                  </summary>
                </member>
              </members>
            </doc>
            """,
            typeof(TestYamlDocRoot).Assembly);

        var document = new TextDocument("spec:");
        var context = YamlSchemaContext.Resolve(document, 2, typeof(TestYamlDocRoot), cache);

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

        var context = YamlSchemaContext.Resolve(document, document.TextLength, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
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
        var context = YamlSchemaContext.Resolve(document, offset, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
        context.CurrentProperty.ShouldNotBeNull();
        context.CurrentProperty.Name.ShouldBe(nameof(V1Container.Name));
        context.Documentation.ShouldNotBeNull();
        context.Documentation.DisplayText.ShouldContain("Name of the container");
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
        var context = YamlSchemaContext.Resolve(document, offset, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
        context.CurrentProperty.ShouldNotBeNull();
        context.CurrentProperty.Name.ShouldBe(nameof(V1Container.ImagePullPolicy));
        context.Documentation.ShouldNotBeNull();
        context.Documentation.DisplayText.ShouldContain("Image pull policy");
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
        var context = YamlSchemaContext.Resolve(document, offset, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
        context.CurrentProperty.ShouldNotBeNull();
        context.CurrentProperty.Name.ShouldBe(nameof(V1Container.ImagePullPolicy));
        context.Documentation.ShouldNotBeNull();
        context.Documentation.DisplayText.ShouldContain("Image pull policy");
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
        var context = YamlSchemaContext.Resolve(document, offset, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1PodSpec));
        context.CurrentProperty.ShouldNotBeNull();
        context.CurrentProperty.Name.ShouldBe(nameof(V1PodSpec.ServiceAccountName));
        context.Documentation.ShouldNotBeNull();
        context.Documentation.DisplayText.ShouldContain("ServiceAccountName");
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
        var context = YamlSchemaContext.Resolve(document, offset, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
        context.CurrentProperty.ShouldNotBeNull();
        context.CurrentProperty.Name.ShouldBe(nameof(V1Container.Name));
        context.Documentation.ShouldNotBeNull();
        context.Documentation.DisplayText.ShouldContain("Name of the container");
        context.Documentation.DisplayText.ShouldNotContain(nameof(V1Pod.Kind));
    }

    [Fact]
    public void Resolve_DoesNotOfferCompletionsWhileTypingAValue()
    {
        var document = new TextDocument("metadata: default");

        var context = YamlSchemaContext.Resolve(document, document.TextLength, typeof(V1Pod), s_modelCache);

        context.CurrentProperty.ShouldBeNull();
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

        var context = YamlSchemaContext.Resolve(document, document.TextLength - 1, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1PodSpec));
        context.CompletionItems.Select(item => item.Text).ShouldContain("containers");
        context.KeyStartColumn.ShouldBe(2);
        context.KeyEndColumn.ShouldBe(2);
        context.KeyPrefix.ShouldBe(string.Empty);
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

        var context = YamlSchemaContext.Resolve(document, document.TextLength, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
        context.CompletionItems.Select(item => item.Text).ShouldContain("name");
        context.KeyPrefix.ShouldBe(string.Empty);
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

        var context = YamlSchemaContext.Resolve(document, document.TextLength, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(string));
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

        var context = YamlSchemaContext.Resolve(document, document.TextLength, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(string));
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

        var context = YamlSchemaContext.Resolve(document, document.TextLength - 1, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Pod));
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

        var context = YamlSchemaContext.Resolve(document, document.TextLength - 1, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
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

        var context = YamlSchemaContext.Resolve(document, document.Text.IndexOf('\n', document.Text.IndexOf("kind: Pod", StringComparison.Ordinal)) + 1, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Pod));
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

        var context = YamlSchemaContext.Resolve(document, document.Text.IndexOf("\n    imagePullPolicy", StringComparison.Ordinal) + 1, typeof(V1Pod), s_modelCache);

        context.ContainerType.ShouldBe(typeof(V1Container));
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

        var result = YamlSchemaContext.TryCreateSequenceEntryInsertion(document, document.TextLength, typeof(V1Pod), s_modelCache, out var insertionText);

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

        var result = YamlSchemaContext.TryCreateSequenceEntryInsertion(document, document.TextLength, typeof(V1Pod), s_modelCache, out var insertionText);

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

        var result = YamlSchemaContext.TryCreateSequenceEntryInsertion(document, document.TextLength, typeof(V1Pod), s_modelCache, out var insertionText);

        result.ShouldBeFalse();
        insertionText.ShouldBeEmpty();
    }

    private static ModelCache CreateModelCache()
    {
        var cache = new ModelCache();
        var xml = new XmlDocument();
        using var stream = typeof(KubernetesCRDModelGen.Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml");
        stream.ShouldNotBeNull();
        xml.Load(stream);
        cache.AddToCache(typeof(V1Pod).Assembly, xml);
        return cache;
    }

    private static ModelCache CreateModelCacheWithXml(string xmlContent, Assembly assembly)
    {
        var cache = new ModelCache();
        var xml = new XmlDocument();
        xml.LoadXml(xmlContent);
        cache.AddToCache(assembly, xml);
        return cache;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string GetXmlMemberTypeName(Type type)
    {
        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    public sealed class TestYamlDocRoot
    {
        [JsonPropertyName("spec")]
        public TestYamlDocSpec? Spec { get; set; }
    }

    public sealed class TestYamlDocSpec
    {
    }

}
