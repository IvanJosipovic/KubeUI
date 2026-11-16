# YAML serialization migration plan

## Goal

Replace KubeUI's local Kubernetes YAML codec and direct calls to KubernetesClient's static `KubernetesYaml` API with the published `SVCSystems.Text.Json.Yaml` library. Keep YAML parsing and serialization behind KubeUI's application-owned services.

Library source checkout: `/home/coder/projects/SVCSystems.Text.Json.Yaml`. Use it to inspect the package implementation and API during migration. Keep KubeUI's committed dependency on the published NuGet package; do not add a project reference to this machine-specific sibling path.

## Plan

1. **Add the NuGet dependency**
   - Pin `SVCSystems.Text.Json.Yaml` centrally in `Directory.Packages.props` (NuGet currently lists `1.0.0-alpha.2`).
   - Add the package to `KubeUI.Kubernetes` and remove YAML package references used only by the old codec.

2. **Replace the local codec implementation**
   - Keep `IKubernetesYamlSerializer` as the KubeUI DI boundary for existing consumers.
   - Reimplement its adapter using `SVCSystems.Text.Json.Yaml` and the application's configured JSON resolver/options.
   - Capture reflection policy at service registration. Use generated `JsonTypeInfo` metadata when available; allow reflection only under the existing opt-in setting.
   - Preserve strict/lenient deserialization, runtime-type handling, thread safety, and application-instance isolation. Do not mutate KubernetesClient's global JSON or YAML settings.

3. **Preserve Kubernetes document behavior**
   - Retain heterogeneous multi-document import/export. Resolve each document's `apiVersion` and `kind`, then deserialize through the corresponding registered model metadata.
   - Preserve Kubernetes-specific conversions and behavior, including `IntOrString`, resource quantities, Secret byte data, timestamps, kubeconfig extensions, and unknown-field handling.

4. **Migrate call sites**
   - Replace direct static `k8s.KubernetesYaml` calls in production, tests, and test infrastructure with the injected KubeUI serializer or the new library where appropriate.
   - Inject the serializer into kubeconfig discovery rather than calling the KubernetesClient static API.
   - Keep YamlDotNet references only in projects that still use it directly, such as YAML editor parsing or Helm representation-model handling.

5. **Remove the obsolete local implementation**
   - Delete KubeUI's old `KubernetesYaml` codec, YAML static context, serializer wrapper implementation, and YAML-only converters/generator registrations once no callers remain.
   - Remove obsolete `YamlDotNet.System.Text.Json` and static-generator references if no other project needs them.

6. **Remove YAML-only source generation and settings**
   - Remove `DisableYamlSerializationReflection` from settings models/persistence, feature flags, settings UI, localized resources, and tests. YAML will use the configured System.Text.Json resolver and its JSON reflection policy.
   - Ensure older saved settings containing the removed field still load safely.
   - Remove `KubernetesYamlStaticContext`, its `[YamlSerializable]` registrations, and `Vecc.YamlDotNet.Analyzers.StaticGenerator`.
   - Keep YamlDotNet itself only where direct parser or Helm APIs still require it.

7. **Migrate and extend tests**
   - Replace implementation-detail tests for the old codec with behavior tests for the new adapter and retain coverage for Kubernetes-specific cases.
   - Update serialization startup tests to verify generated metadata coverage from the application resource registry, generated and unregistered model round-trips in both directions, all JSON/YAML reflection setting combinations, defaults, and independent hosts.
   - Cover strict/lenient parsing, multi-document mixed-kind resources, kubeconfig discovery, resources, Helm, MCP, and test transport fixtures.

8. **Validate**
   - Build before tests; run focused serialization tests, affected projects, then the full suite.
   - Run Release trim analysis and fix new diagnostics without broad suppressions.
   - Confirm KubeUI has no direct calls to KubernetesClient's static `KubernetesYaml` API and that remaining YamlDotNet references serve non-serialization use cases.
