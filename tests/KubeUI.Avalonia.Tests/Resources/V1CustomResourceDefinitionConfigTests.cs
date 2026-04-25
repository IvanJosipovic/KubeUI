using Avalonia;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Features.Clusters.Workspace;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources;

public class V1CustomResourceDefinitionConfigTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void list_crd_command_does_not_throw_when_type_is_unavailable()
    {
        var cluster = new TestCluster().CreateWorkspace();
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = ActivatorUtilities.CreateInstance<V1CustomResourceDefinitionConfig>(services);
        config.Initialize(cluster);

        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("example.com", "examples", "someString");

        Should.NotThrow(() =>
        {
            config.ListCRDCommand.CanExecute(crd).ShouldBeFalse();
        });
    }

    [AvaloniaFact]
    public void generate_uses_humanized_plural_kind_for_display_name()
    {
        var cluster = new TestCluster().CreateWorkspace();
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig<TestCustomResource>>(services);
        config.Initialize(cluster);

        var crd = new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Names = new V1CustomResourceDefinitionNames
                {
                    Kind = "IngressClass",
                    Plural = "ingressclasses",
                },
                Versions =
                [
                    new V1CustomResourceDefinitionVersion
                    {
                        Name = "v1",
                        Storage = true,
                        Served = true,
                    }
                ]
            }
        };

        config.Generate(crd);

        config.Name.ShouldBe("Ingress Classes");
    }

    [AvaloniaFact]
    public void resource_list_column_value_accessor_returns_null_for_missing_nullable_values()
    {
        var column = new ResourceListColumn<NullableValueResource, int>
        {
            Name = "Value",
            Field = resource => resource.Value!.Value,
        };

        var accessor = column.ValueAccessor;

        Should.NotThrow(() => accessor.GetValue(new NullableValueResource()));
        accessor.GetValue(new NullableValueResource()).ShouldBeNull();
        column.DisplayValue(new NullableValueResource()).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void crd_printer_column_returns_empty_value_for_missing_annotation_key()
    {
        var cluster = new TestCluster().CreateWorkspace();
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig<TestCustomResource>>(services);
        config.Initialize(cluster);

        var crd = new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Scope = "Namespaced",
                Names = new V1CustomResourceDefinitionNames
                {
                    Kind = "IngressClass",
                    Plural = "ingressclasses",
                },
                Versions =
                [
                    new V1CustomResourceDefinitionVersion
                    {
                        Name = "v1",
                        Storage = true,
                        Served = true,
                        AdditionalPrinterColumns =
                        [
                            new V1CustomResourceColumnDefinition
                            {
                                Name = "External Name",
                                JsonPath = ".metadata.annotations['crossplane.io/external-name']",
                                Type = "string",
                            }
                        ]
                    }
                ]
            }
        };

        config.Generate(crd);

        var column = config.Columns().Single(x => x.Name == "External Name");
        var resource = new TestCustomResource
        {
            Metadata = new V1ObjectMeta
            {
                Annotations = new Dictionary<string, string>(),
            },
        };

        Should.NotThrow(() => column.ValueAccessor.GetValue(resource));
        column.ValueAccessor.GetValue(resource).ShouldBeNull();
        column.DisplayValue(resource).ShouldBeEmpty();
        column.SortKey(resource).ShouldBeNull();
    }

    [AvaloniaFact]
    public void crd_generator_uses_nullable_value_types_for_optional_printer_columns()
    {
        var cluster = new TestCluster().CreateWorkspace();
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig<TestCustomResourceWithSpec>>(services);
        config.Initialize(cluster);

        var crd = new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Scope = "Namespaced",
                Names = new V1CustomResourceDefinitionNames
                {
                    Kind = "Example",
                    Plural = "examples",
                },
                Versions =
                [
                    new V1CustomResourceDefinitionVersion
                    {
                        Name = "v1",
                        Storage = true,
                        Served = true,
                        AdditionalPrinterColumns =
                        [
                            new V1CustomResourceColumnDefinition
                            {
                                Name = "Revision",
                                JsonPath = ".spec.revision",
                                Type = "integer",
                            }
                        ]
                    }
                ]
            }
        };

        config.Generate(crd);

        var column = config.Columns().Single(x => x.Name == "Revision");
        column.ValueType.ShouldBe(typeof(int?));
        column.ValueAccessor.GetValue(new TestCustomResourceWithSpec()).ShouldBeNull();
        column.DisplayValue(new TestCustomResourceWithSpec()).ShouldBeEmpty();
    }
}

[KubernetesEntity(Group = "example.com", ApiVersion = "v1", Kind = "IngressClass")]
internal sealed class TestCustomResource : k8s.IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "example.com/v1";
    public string Kind { get; set; } = "IngressClass";
    public V1ObjectMeta Metadata { get; set; } = new();
}

internal sealed class NullableValueResource : k8s.IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "v1";
    public string Kind { get; set; } = "Test";
    public V1ObjectMeta Metadata { get; set; } = new();
    public int? Value { get; set; }
}

[KubernetesEntity(Group = "example.com", ApiVersion = "v1", Kind = "Example")]
internal sealed class TestCustomResourceWithSpec : k8s.IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "example.com/v1";
    public string Kind { get; set; } = "Example";
    public V1ObjectMeta Metadata { get; set; } = new();
    public TestCustomResourceSpec Spec { get; set; } = new();
}

internal sealed class TestCustomResourceSpec
{
    public int? Revision { get; set; }
}
