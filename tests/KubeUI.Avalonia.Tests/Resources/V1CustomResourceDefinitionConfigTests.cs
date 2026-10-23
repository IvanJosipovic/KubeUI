using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Features.Clusters.Workspace;
using KubeUI.Kubernetes.Serialization;
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
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
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
    public void custom_resource_config_uses_yaml_template_for_new_objects()
    {
        var services = Application.Current.GetTestServices();
        const string template = """
            apiVersion: kubeui.com/v1beta1
            kind: Test
            metadata:
              name: sample-test
              namespace: sample-namespace
            spec:
              value: hello
            """;
        var config = new TestKubeUiCustomResourceConfig(services, template, configurePresentation: false);
        config.Configure(ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("kubeui.com", "widgets", "value"));

        var first = config.CreateNewResource();
        var second = config.CreateNewResource();

        first.ShouldBeOfType<GenericKubernetesObject>();
        first.ShouldNotBeSameAs(second);
        first.ApiVersion.ShouldBe(config.Kind.GroupApiVersion);
        first.Kind.ShouldBe(config.Kind.Kind);
        first.Metadata.Name.ShouldBe("sample-test");
        first.Metadata.NamespaceProperty.ShouldBe("sample-namespace");
        first.Properties["spec"].GetProperty("value").GetString().ShouldBe("hello");
    }

    [AvaloniaFact]
    public void custom_resource_config_selects_matching_specialized_kind_and_falls_back_for_other_kind()
    {
        var services = Application.Current.GetTestServices();
        var fallback = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
        var specialized = new TestKubeUiCustomResourceConfig(services, "", configurePresentation: false);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("kubeui.com", "widgets", "value");

        CustomResourceConfig.Select(crd, [fallback, specialized]).ShouldBeSameAs(specialized);
        specialized.ApiGroup.ShouldBe("kubeui.com");
        specialized.KindName.ShouldBe("Test");
        specialized.ApiVersion.ShouldBeNull();

        var versioned = new CustomResourceConfig(services, "kubeui.com", "Test", "v1beta1");
        CustomResourceConfig.Select(crd, [fallback, versioned]).ShouldBeSameAs(versioned);
        var unavailableVersion = new CustomResourceConfig(services, "kubeui.com", "Test", "v9");
        CustomResourceConfig.Select(crd, [fallback, unavailableVersion]).ShouldBeSameAs(fallback);

        var otherCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("kubeui.com", "widgets", "value");
        otherCrd.Spec.Group = "other.com";
        CustomResourceConfig.Select(otherCrd, [fallback, specialized]).ShouldBeSameAs(fallback);
    }

    [AvaloniaFact]
    public void custom_resource_config_rejects_multiple_specialized_kind_matches()
    {
        var services = Application.Current.GetTestServices();
        var fallback = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
        var first = new TestKubeUiCustomResourceConfig(services, "", configurePresentation: false);
        var second = new DuplicateTestKubeUiCustomResourceConfig(services);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("kubeui.com", "widgets", "value");

        Should.Throw<InvalidOperationException>(() => CustomResourceConfig.Select(crd, [fallback, first, second]));
    }

    [AvaloniaFact]
    public void custom_resource_config_reports_a_definition_without_a_served_storage_version()
    {
        var services = Application.Current.GetTestServices();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("kubeui.com", "widgets", "value");

        // A definition whose storage version is not served cannot be listed or watched. The
        // failure must be reported explicitly instead of surfacing as an unhandled
        // sequence access.
        crd.Spec.Versions = [new V1CustomResourceDefinitionVersion { Name = "v1", Served = false, Storage = true }];

        Should.Throw<InvalidOperationException>(() => config.Configure(crd));
        config.Definition.ShouldBeNull();
    }

    [AvaloniaFact]
    public void registered_crd_configuration_replaces_printer_columns_and_adds_resource_actions()
    {
        var services = Application.Current.GetTestServices();
        var customColumn = new ResourceListColumn<GenericKubernetesObject, string>
        {
            Key = "configured",
            Name = "Configured",
            Field = _ => "value",
        };
        var config = new TestKubeUiCustomResourceConfig(services, "", configurePresentation: true, customColumn);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("kubeui.com", "widgets", "value");
        crd.Spec.Versions[0].AdditionalPrinterColumns =
        [
            new V1CustomResourceColumnDefinition { Name = "Printer", JsonPath = ".spec.value", Type = "string" },
        ];

        config.Configure(crd);

        config.Columns().Select(static column => column.Name).ShouldContain("Configured");
        config.Columns().ShouldNotContain(static column => column.Name == "Printer");
        config.GetCustomMenuItems(Array.Empty<GenericKubernetesObject>()).Single().Title.ShouldBe("Configured action");
        config.Properties(new GenericKubernetesObject()).ShouldContain(static control => control is Border);
        config.ShouldBeAssignableTo<ResourceConfig<GenericKubernetesObject>>();
    }

    private sealed class TestKubeUiCustomResourceConfig(
        IServiceProvider serviceProvider,
        string template,
        bool configurePresentation,
        IResourceListColumn? customColumn = null)
        : CustomResourceConfig(serviceProvider, "kubeui.com", "Test")
    {
        protected override bool IncludeAdditionalPrinterColumns => !configurePresentation;

        protected override IEnumerable<IResourceListColumn> CreateAdditionalColumns(V1CustomResourceDefinition crd)
            => customColumn is null ? [] : [customColumn];

        protected override GenericKubernetesObject CreateInitialResourceTemplate()
            => string.IsNullOrWhiteSpace(template)
                ? new GenericKubernetesObject()
                : Kubernetes.Serialization.KubernetesYaml.Deserialize<GenericKubernetesObject>(template);

        protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<GenericKubernetesObject>? selectedItems)
            => configurePresentation ? [new MenuItemViewModel { Title = "Configured action" }] : [];

        public override Control[] Properties(GenericKubernetesObject resource)
            => configurePresentation ? [new Border()] : [];
    }

    private sealed class DuplicateTestKubeUiCustomResourceConfig(IServiceProvider serviceProvider)
        : CustomResourceConfig(serviceProvider, "kubeui.com", "Test");

    [AvaloniaFact]
    public async Task configure_uses_humanized_plural_kind_for_display_name()
    {
        var services = Application.Current.GetTestServices();

        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
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
    public void resource_list_column_displays_empty_for_missing_nullable_values()
    {
        var column = new ResourceListColumn<NullableValueResource, int>
        {
            Key = "value",
            Name = "Value",
            Field = resource => resource.Value!.Value,
        };

        Should.NotThrow(() => column.DisplayValue(new NullableValueResource()));
        column.DisplayValue(new NullableValueResource()).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task crd_printer_column_returns_empty_value_for_missing_annotation_key()
    {
        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
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

        Should.NotThrow(() => column.DisplayValue(resource));
        column.DisplayValue(resource).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task crd_printer_column_reads_values_from_json_documents()
    {
        var services = Application.Current.GetTestServices();
        var clusterConfig = services.GetRequiredService<TestClusterConfig>();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
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
        column.DisplayValue(KubernetesJson.Deserialize<GenericKubernetesObject>("""{"spec":{"revision":42}}""")).ShouldBe("42");
        column.DisplayValue(KubernetesJson.Deserialize<GenericKubernetesObject>("""{"spec":{}}""")).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void custom_resource_config_supports_all_printer_column_value_types_and_skips_invalid_columns()
    {
        var services = Application.Current.GetTestServices();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
        var crd = new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Scope = "Cluster",
                Names = new V1CustomResourceDefinitionNames { Kind = "Example", Plural = "examples" },
                Versions =
                [
                    new V1CustomResourceDefinitionVersion
                    {
                        Name = "v1",
                        Storage = true,
                        Served = true,
                        AdditionalPrinterColumns =
                        [
                            new V1CustomResourceColumnDefinition { Name = "Long", JsonPath = ".spec.longValue", Type = "integer", Format = "int64" },
                            new V1CustomResourceColumnDefinition { Name = "Integer", JsonPath = ".spec.intValue", Type = "integer" },
                            new V1CustomResourceColumnDefinition { Name = "Number", JsonPath = ".spec.numberValue", Type = "number" },
                            new V1CustomResourceColumnDefinition { Name = "Boolean", JsonPath = ".spec.boolValue", Type = "boolean" },
                            new V1CustomResourceColumnDefinition { Name = "Date", JsonPath = ".spec.dateValue", Type = "date" },
                            new V1CustomResourceColumnDefinition { Name = "String", JsonPath = ".spec.stringValue", Type = "string" },
                            new V1CustomResourceColumnDefinition { Name = "Invalid", JsonPath = "[", Type = "integer" },
                            new V1CustomResourceColumnDefinition { Name = "Creation Time", JsonPath = ".metadata.creationTimestamp", Type = "date" },
                        ]
                    }
                ]
            }
        };

        Should.NotThrow(() => config.Configure(crd));

        config.Definition.ShouldBeSameAs(crd);
        config.IsNamespaced.ShouldBeFalse();
        var columns = config.Columns().ToDictionary(column => column.Name, StringComparer.Ordinal);
        columns.Keys.ShouldContain("Long");
        columns.Keys.ShouldContain("Integer");
        columns.Keys.ShouldContain("Number");
        columns.Keys.ShouldContain("Boolean");
        columns.Keys.ShouldContain("Date");
        columns.Keys.ShouldContain("String");
        columns.Keys.ShouldNotContain("Invalid");
        columns.Keys.ShouldNotContain("Creation Time");
        columns["Long"].ValueType.ShouldBe(typeof(long?));
        columns["Integer"].ValueType.ShouldBe(typeof(int?));
        columns["Number"].ValueType.ShouldBe(typeof(double?));
        columns["Boolean"].ValueType.ShouldBe(typeof(bool?));
        columns["Date"].ValueType.ShouldBe(typeof(DateTime?));

        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>(
            """{"spec":{"longValue":42,"intValue":7,"numberValue":2.5,"boolValue":true,"dateValue":"2026-10-15T00:00:00Z","stringValue":"sample"}}""");
        columns["Long"].DisplayValue(resource).ShouldBe("42");
        columns["Integer"].DisplayValue(resource).ShouldBe("7");
        columns["Number"].DisplayValue(resource).ShouldBe("2.5");
        columns["Boolean"].DisplayValue(resource).ShouldNotBeEmpty();
        columns["Date"].DisplayValue(resource).ShouldNotBeEmpty();
        columns["String"].DisplayValue(resource).ShouldBe("sample");

        using var specDocument = JsonDocument.Parse("""{"longValue":42,"intValue":7,"numberValue":2.5,"boolValue":true,"dateValue":"2026-10-15T00:00:00Z","stringValue":"sample"}""");
        var jsonResource = new GenericKubernetesObject
        {
            ApiVersion = "example.com/v1",
            Kind = "Example",
            Metadata = new V1ObjectMeta(),
            Properties = new Dictionary<string, JsonElement>
            {
                ["spec"] = specDocument.RootElement.Clone(),
            },
        };
        columns["Long"].DisplayValue(jsonResource).ShouldBe("42");
        columns["Integer"].DisplayValue(jsonResource).ShouldBe("7");
        columns["Number"].DisplayValue(jsonResource).ShouldBe("2.5");
    }

    [AvaloniaFact]
    public void custom_resource_config_requires_a_served_storage_version()
    {
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(Application.Current.GetTestServices());
        var invalidCrd = new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Names = new V1CustomResourceDefinitionNames { Kind = "Example", Plural = "examples" },
                Versions =
                [
                    new V1CustomResourceDefinitionVersion { Name = "v1", Served = false, Storage = false },
                ]
            }
        };

        Should.Throw<InvalidOperationException>(() => config.Configure(invalidCrd));
    }

    [AvaloniaFact]
    public void crd_printer_columns_exclude_default_creation_timestamp_column()
    {
        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
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

    [AvaloniaFact]
    public void builtin_configuration_reports_builtin_origin()
    {
        var services = Application.Current.GetTestServices();
        IResourceConfig resource = ActivatorUtilities.CreateInstance<V1CustomResourceDefinitionConfig>(services);

        resource.Origin.ShouldBe(ResourceOrigin.BuiltIn);
        resource.CustomResourceDefinition.ShouldBeNull();
        resource.IsCustomResource.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void custom_resource_configuration_reports_its_crd_origin_and_definition()
    {
        var services = Application.Current.GetTestServices();
        var config = ActivatorUtilities.CreateInstance<CustomResourceConfig>(services);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("example.com", "examples", "someString");
        config.Configure(crd);

        IResourceConfig resource = config;

        resource.Origin.ShouldBe(ResourceOrigin.CustomResourceDefinition);
        resource.CustomResourceDefinition.ShouldBeSameAs(crd);
        resource.IsCustomResource.ShouldBeTrue();
    }
}
