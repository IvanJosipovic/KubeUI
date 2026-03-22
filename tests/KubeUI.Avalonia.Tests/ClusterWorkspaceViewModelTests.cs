using System.Reflection;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public class ClusterWorkspaceViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];

    public override void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    [AvaloniaFact]
    public async Task added_crd_adds_resource_config_and_model_cache_entry()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(crd);

        var resourceType = await WaitForValueAsync(() => GetCustomResourceType(runtime, crd));
        resourceType.ShouldNotBeNull();

        var resourceConfig = await WaitForValueAsync(() => GetCustomResourceConfig(workspace, crd));
        resourceConfig.ShouldNotBeNull();
        resourceConfig.Type.ShouldBe(resourceType);
        resourceConfig.IsCustomResource.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task updated_crd_replaces_resource_config_model_cache_entry_and_seeded_informer()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(originalCrd);

        var originalType = await WaitForValueAsync(() => GetCustomResourceType(runtime, originalCrd));
        originalType.ShouldNotBeNull();
        await SeedResourceAsync(runtime, originalType);

        var originalContainer = GetSeededContainer(runtime, originalType);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        await runtime.AddOrUpdateResource(updatedCrd);

        var updatedType = await WaitForValueAsync(() => GetCustomResourceType(runtime, updatedCrd));
        updatedType.ShouldNotBeNull();
        updatedType.ShouldNotBe(originalType);

        var updatedResourceConfig = await WaitForValueAsync(() => GetCustomResourceConfig(workspace, updatedCrd));
        updatedResourceConfig.ShouldNotBeNull();
        updatedResourceConfig.Type.ShouldBe(updatedType);

        var updatedContainer = await WaitForValueAsync(() => GetSeededContainer(runtime, updatedType));
        updatedContainer.ShouldNotBeNull();
        updatedContainer.ShouldNotBeSameAs(originalContainer);
        GetInformers(originalContainer).Count.ShouldBe(0);
        GetInformers(updatedContainer).Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task deleted_crd_removes_resource_config_model_cache_entry_and_seeded_informer()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.AddOrUpdateResource(crd);

        var resourceType = await WaitForValueAsync(() => GetCustomResourceType(runtime, crd));
        resourceType.ShouldNotBeNull();
        await SeedResourceAsync(runtime, resourceType);

        var seededContainer = GetSeededContainer(runtime, resourceType);
        seededContainer.ShouldNotBeNull();
        GetInformers(seededContainer).Count.ShouldBe(1);

        await runtime.DeleteResource(crd);

        await WaitForAsync(() => GetCustomResourceConfig(workspace, crd) == null);
        GetCustomResourceType(runtime, crd).ShouldBeNull();
        runtime.Objects.ContainsKey(GroupApiVersionKind.From(resourceType)).ShouldBeFalse();
        GetInformers(seededContainer).Count.ShouldBe(0);
    }

    private ClusterWorkspaceViewModel CreateWorkspace(TestCluster runtime)
    {
        var workspace = runtime.CreateWorkspace();
        _disposables.Add(workspace);
        return workspace;
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static async Task<T?> WaitForValueAsync<T>(Func<T?> valueFactory, int timeoutMs = 10000) where T : class
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            var value = valueFactory();
            if (value != null)
            {
                return value;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        return valueFactory();
    }

    private static Type? GetCustomResourceType(TestClusterRuntime runtime, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        return version == null ? null : runtime.ModelCache.GetResourceType(crd.Spec.Group, version, crd.Spec.Names.Kind);
    }

    private static IResourceConfig? GetCustomResourceConfig(ClusterWorkspaceViewModel workspace, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        if (version == null)
        {
            return null;
        }

        return workspace.GetResourceConfigs().FirstOrDefault(x =>
            x.IsCustomResource
            && string.Equals(x.Kind.Group, crd.Spec.Group, StringComparison.Ordinal)
            && string.Equals(x.Kind.ApiVersion, version, StringComparison.Ordinal)
            && string.Equals(x.Kind.Kind, crd.Spec.Names.Kind, StringComparison.Ordinal));
    }

    private static async Task SeedResourceAsync(TestClusterRuntime runtime, Type resourceType)
    {
        var method = runtime.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(TestClusterRuntime.SeedResource) && x.IsGenericMethodDefinition && x.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        await (Task)method.Invoke(runtime, [false])!;
    }

    private static object? GetSeededContainer(TestClusterRuntime runtime, Type resourceType)
    {
        return runtime.Objects.TryGetValue(GroupApiVersionKind.From(resourceType), out var container)
            ? container
            : null;
    }

    private static IList<IResourceInformer> GetInformers(object container)
    {
        return (IList<IResourceInformer>)(container.GetType().GetProperty("Informers")?.GetValue(container)
            ?? throw new InvalidOperationException("Container does not expose Informers."));
    }
}

internal static class ClusterWorkspaceTestCustomResourceDefinitionFactory
{
    public static V1CustomResourceDefinition Create(string name, string plural, string schemaProperty)
    {
        return new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = name
            },
            Spec = new()
            {
                Group = "kubeui.com",
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = plural,
                    Singular = "test",
                    Kind = "Test",
                    ListKind = "TestList"
                },
                Versions =
                [
                    new()
                    {
                        Name = "v1beta1",
                        Served = true,
                        Storage = true,
                        Schema = new()
                        {
                            OpenAPIV3Schema = new()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, V1JSONSchemaProps>
                                {
                                    ["apiVersion"] = new() { Type = "string" },
                                    ["kind"] = new() { Type = "string" },
                                    ["metadata"] = new() { Type = "object" },
                                    ["spec"] = new()
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, V1JSONSchemaProps>
                                        {
                                            [schemaProperty] = new() { Type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ]
            }
        };
    }
}
