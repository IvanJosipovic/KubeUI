using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Tests.Features.Clusters.Workspace;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources;

public class V1CustomResourceDefinitionConfigTests
{
    [AvaloniaFact]
    public async Task list_crd_command_does_not_throw_when_type_is_unavailable()
    {
        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<V1CustomResourceDefinitionConfig>(services);
        config.Initialize(cluster);

        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("example.com", "examples", "someString");

        Should.NotThrow(() =>
        {
            config.ListCRDCommand.CanExecute(crd).ShouldBeFalse();
        });
    }

    [AvaloniaFact]
    public void list_items_action_has_list_icon()
    {
        var services = Application.Current.GetTestServices();
        var config = ActivatorUtilities.CreateInstance<V1CustomResourceDefinitionConfig>(services);

        var action = config.GetCustomMenuItems(Array.Empty<V1CustomResourceDefinition>()).Single();

        action.FluentIcon.ShouldBe(FluentIcons.Common.Icon.AppsList);
    }

    [AvaloniaFact]
    public async Task generic_resource_list_initializes_with_its_resource_kind()
    {
        var services = Application.Current.GetTestServices();
        var cluster = await Application.Current.CreateClusterAsync();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
        config.Initialize(cluster);
        config.Configure(ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("example.com", "examples", "someString"));
        cluster.AddResourceConfigForTest(config);
        cluster.Runtime.ModelCatalog.RegisterCustomResourceDefinition(config.Kind);

        var vm = services.GetRequiredService<ResourceListViewModel<GenericKubernetesObject>>();
        vm.InitializeResource(cluster, config.Kind);

        vm.Kind.ShouldBe(config.Kind);
        vm.ResourceConfig.ShouldBe(config);
    }

    [AvaloniaFact]
    public async Task configure_uses_humanized_plural_kind_for_display_name()
    {
        var services = Application.Current.GetTestServices();

        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
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

        config.Configure(crd);

        config.Name.ShouldBe("Ingress Classes");
    }

    [AvaloniaFact]
    public void resource_list_column_value_accessor_returns_null_for_missing_nullable_values()
    {
        var column = new ResourceListColumn<NullableValueResource, int>
        {
            Key = "value",
            Name = "Value",
            Field = resource => resource.Value!.Value,
        };

        var accessor = column.ValueAccessor;

        Should.NotThrow(() => accessor.GetValue(new NullableValueResource()));
        accessor.GetValue(new NullableValueResource()).ShouldBeNull();
        column.DisplayValue(new NullableValueResource()).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task crd_printer_column_returns_empty_value_for_missing_annotation_key()
    {
        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
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

        config.Configure(crd);

        var column = config.Columns().Single(x => x.Name == "External Name");
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""{"apiVersion":"example.com/v1","kind":"IngressClass","metadata":{"annotations":{}}}""");

        Should.NotThrow(() => column.ValueAccessor.GetValue(resource));
        column.ValueAccessor.GetValue(resource).ShouldBe("");
        column.DisplayValue(resource).ShouldBeEmpty();
        column.SortKey(resource).ShouldBe("");
    }

    [AvaloniaFact]
    public async Task crd_printer_column_reads_values_from_json_documents()
    {
        var services = Application.Current.GetTestServices();
        var clusterConfig = services.GetRequiredService<TestClusterConfig>();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
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

        config.Configure(crd);

        var column = config.Columns().Single(x => x.Name == "Revision");
        column.ValueType.ShouldBe(typeof(int?));
        column.ValueAccessor.GetValue(KubernetesJson.Deserialize<GenericKubernetesObject>("""{"spec":{"revision":42}}""")).ShouldBe(42);
        column.ValueAccessor.GetValue(KubernetesJson.Deserialize<GenericKubernetesObject>("""{"spec":{}}""")).ShouldBeNull();
        column.DisplayValue(KubernetesJson.Deserialize<GenericKubernetesObject>("""{"spec":{}}""")).ShouldBe("");
    }

    [AvaloniaFact]
    public void crd_printer_columns_exclude_default_creation_timestamp_column()
    {
        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
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
                                Name = "Created",
                                JsonPath = ".metadata.creationTimestamp",
                                Type = "date",
                            },
                            new V1CustomResourceColumnDefinition
                            {
                                Name = "Revision",
                                JsonPath = ".spec.revision",
                                Type = "integer",
                            },
                        ],
                    },
                ],
            },
        };

        config.Configure(crd);

        config.Columns().ShouldNotContain(column => column.Name == "Created");
        config.Columns().ShouldContain(column => column.Name == "Revision");
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
