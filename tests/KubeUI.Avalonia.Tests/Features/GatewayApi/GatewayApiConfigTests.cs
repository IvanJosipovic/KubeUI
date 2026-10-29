using System.Text.Json;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Features.GatewayApi;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.GatewayApi;

public sealed class GatewayApiConfigTests
{
    private const string StandardGroup = "gateway.networking.k8s.io";
    private const string ExperimentalGroup = "gateway.networking.x-k8s.io";

    [AvaloniaFact]
    public void dependency_injection_registers_one_config_per_gateway_api_crd_identity()
    {
        var actual = Application.Current.GetTestServices()
            .GetServices<CustomResourceConfig>()
            .Where(static config => config.ApiGroup is StandardGroup or ExperimentalGroup)
            .Select(static config => (config.ApiGroup, config.ApiVersion, config.KindName))
            .OrderBy(static identity => identity.ApiGroup, StringComparer.Ordinal)
            .ThenBy(static identity => identity.KindName, StringComparer.Ordinal)
            .ToArray();

        (string? ApiGroup, string? ApiVersion, string? KindName)[] expected =
        [
            (StandardGroup, null, "BackendTLSPolicy"),
            (StandardGroup, null, "GRPCRoute"),
            (StandardGroup, null, "Gateway"),
            (StandardGroup, null, "GatewayClass"),
            (StandardGroup, null, "HTTPRoute"),
            (StandardGroup, null, "ListenerSet"),
            (StandardGroup, null, "ReferenceGrant"),
            (StandardGroup, null, "TCPRoute"),
            (StandardGroup, null, "TLSRoute"),
            (StandardGroup, null, "UDPRoute"),
            (ExperimentalGroup, null, "XBackend"),
            (ExperimentalGroup, null, "XBackendTrafficPolicy"),
            (ExperimentalGroup, null, "XMesh"),
        ];

        actual.ShouldBe(expected);
    }

    [AvaloniaFact]
    public async Task kind_configs_create_their_own_parseable_starter_templates()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        (string Group, string Version, string Kind, string Property)[] expected =
        [
            (StandardGroup, "v1", "BackendTLSPolicy", "targetRefs"),
            (StandardGroup, "v1", "Gateway", "gatewayClassName"),
            (StandardGroup, "v1", "GatewayClass", "controllerName"),
            (StandardGroup, "v1", "GRPCRoute", "parentRefs"),
            (StandardGroup, "v1", "HTTPRoute", "parentRefs"),
            (StandardGroup, "v1", "ListenerSet", "parentRef"),
            (StandardGroup, "v1beta1", "ReferenceGrant", "from"),
            (StandardGroup, "v1", "TCPRoute", "parentRefs"),
            (StandardGroup, "v1", "TLSRoute", "parentRefs"),
            (StandardGroup, "v1", "UDPRoute", "parentRefs"),
            (ExperimentalGroup, "v1alpha1", "XBackend", "type"),
            (ExperimentalGroup, "v1alpha1", "XBackendTrafficPolicy", "targetRefs"),
            (ExperimentalGroup, "v1alpha1", "XMesh", "controllerName"),
        ];

        var configurations = Application.Current.GetTestServices()
            .GetServices<CustomResourceConfig>()
            .Where(static config => config.ApiGroup is StandardGroup or ExperimentalGroup)
            .ToArray();
        foreach (var (group, version, kind, property) in expected)
        {
            var config = configurations.Single(config => config.ApiGroup == group && config.KindName == kind);
            config.ApiVersion.ShouldBeNull();
            config.Configure(CreateDefinition(group, version, kind, group == ExperimentalGroup ? "experimental" : "standard"));
            config.Navigation.SectionId.ShouldBe(NavigationSections.NetworkId);
            config.Navigation.Groups.Select(static navigationGroup => navigationGroup.Id)
                .ShouldBe(group == ExperimentalGroup ? ["gateway", "experimental"] : ["gateway"]);

            config.Initialize(workspace);
            var resource = config.CreateNewResource();
            resource.ApiVersion.ShouldBe($"{group}/{version}");
            resource.Kind.ShouldBe(kind);
            resource.Properties["spec"].ValueKind.ShouldBe(JsonValueKind.Object);
            resource.Properties["spec"].TryGetProperty(property, out _).ShouldBeTrue();
        }
    }

    [AvaloniaFact]
    public void experimental_api_group_uses_nested_experimental_navigation_group()
    {
        var standard = new HTTPRouteConfig();
        standard.Configure(CreateDefinition(StandardGroup, "v1", "HTTPRoute", "standard"));
        standard.Navigation.SectionId.ShouldBe(NavigationSections.NetworkId);
        standard.Navigation.Groups.Select(static group => group.Id).ShouldBe(["gateway"]);

        var experimental = new HTTPRouteConfig();
        experimental.Configure(CreateDefinition(StandardGroup, "v1", "HTTPRoute", "experimental"));
        experimental.Navigation.SectionId.ShouldBe(NavigationSections.NetworkId);
        experimental.Navigation.Groups.Select(static group => group.Id).ShouldBe(["gateway"]);

        var xKind = new XBackendConfig();
        xKind.Configure(CreateDefinition(ExperimentalGroup, "v1alpha1", "XBackend", "experimental"));
        xKind.Navigation.SectionId.ShouldBe(NavigationSections.NetworkId);
        xKind.Navigation.Groups.Select(static group => group.Id).ShouldBe(["gateway", "experimental"]);
    }

    [AvaloniaFact]
    public void gateway_configs_match_served_versions_other_than_the_storage_version()
    {
        var config = new TCPRouteConfig();
        var definition = CreateDefinition(StandardGroup, "v1alpha2", "TCPRoute", "experimental");

        config.Matches(definition).ShouldBeTrue();
        config.Configure(definition);
        config.Kind.ApiVersion.ShouldBe("v1alpha2");
        config.Navigation.Groups.Select(static group => group.Id).ShouldBe(["gateway"]);
    }

    private static V1CustomResourceDefinition CreateDefinition(string group, string version, string kind, string channel)
        => new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"{kind.ToLowerInvariant()}s.{group}",
                Annotations = new Dictionary<string, string> { ["gateway.networking.k8s.io/channel"] = channel },
            },
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = group,
                Names = new V1CustomResourceDefinitionNames { Kind = kind, Plural = $"{kind.ToLowerInvariant()}s" },
                Scope = "Namespaced",
                Versions =
                [
                    new V1CustomResourceDefinitionVersion
                    {
                        Name = version,
                        Served = true,
                        Storage = true,
                    }
                ],
            }
        };
}
