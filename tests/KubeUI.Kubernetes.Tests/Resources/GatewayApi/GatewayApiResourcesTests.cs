using System.Text.Json;
using k8s.Models;
using KubeUI.Kubernetes.Resources.GatewayApi;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Resources.GatewayApi;

public class GatewayApiResourcesTests
{
    [Theory]
    [InlineData("gateway.networking.k8s.io", true)]
    [InlineData("gateway.networking.x-k8s.io", true)]
    [InlineData("networking.k8s.io", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsGatewayApiGroup_matches_only_gateway_groups(string? group, bool expected)
    {
        GatewayApiResources.IsGatewayApiGroup(group).ShouldBe(expected);
    }

    [Theory]
    [InlineData("gateway.networking.k8s.io", "HTTPRoute", new[] { "v1", "v1beta1" }, GatewayApiChannel.Standard)]
    [InlineData("gateway.networking.k8s.io", "ReferenceGrant", new[] { "v1beta1" }, GatewayApiChannel.Standard)]
    [InlineData("gateway.networking.k8s.io", "TCPRoute", new[] { "v1alpha2" }, GatewayApiChannel.Experimental)]
    [InlineData("gateway.networking.x-k8s.io", "XListenerSet", new[] { "v1alpha1" }, GatewayApiChannel.Experimental)]
    public void GetChannel_classifies_by_group_and_served_versions(string group, string kind, string[] versions, GatewayApiChannel expected)
    {
        var crd = CreateCrd(group, kind, versions);
        crd.Metadata.Annotations = new Dictionary<string, string> { ["gateway.networking.k8s.io/channel"] = "experimental" };

        GatewayApiResources.GetChannel(crd).ShouldBe(expected);
    }

    [Fact]
    public void GetChannel_returns_null_for_other_groups()
    {
        GatewayApiResources.GetChannel(CreateCrd("example.com", "Widget", ["v1"])).ShouldBeNull();
    }

    [Fact]
    public void GetChannel_ignores_versions_that_are_not_served()
    {
        var crd = CreateCrd("gateway.networking.k8s.io", "TLSRoute", ["v1", "v1alpha2"]);
        crd.Spec.Versions[0].Served = false;

        GatewayApiResources.GetChannel(crd).ShouldBe(GatewayApiChannel.Experimental);
    }

    [Theory]
    [InlineData("GatewayClass", "controllerName")]
    [InlineData("Gateway", "listeners")]
    [InlineData("HTTPRoute", "rules")]
    [InlineData("GRPCRoute", "rules")]
    [InlineData("TLSRoute", "hostnames")]
    [InlineData("TCPRoute", "parentRefs")]
    [InlineData("UDPRoute", "parentRefs")]
    [InlineData("ReferenceGrant", "from")]
    [InlineData("BackendTLSPolicy", "targetRefs")]
    [InlineData("ListenerSet", "parentRef")]
    [InlineData("XListenerSet", "listeners")]
    public void TryGetSpecTemplate_returns_object_spec_for_known_kinds(string kind, string expectedProperty)
    {
        GatewayApiResources.TryGetSpecTemplate(kind, out var spec).ShouldBeTrue();

        spec.ValueKind.ShouldBe(JsonValueKind.Object);
        spec.TryGetProperty(expectedProperty, out _).ShouldBeTrue();
    }

    [Fact]
    public void TryGetSpecTemplate_returns_false_for_unknown_kind()
    {
        GatewayApiResources.TryGetSpecTemplate("Widget", out _).ShouldBeFalse();
    }

    private static V1CustomResourceDefinition CreateCrd(string group, string kind, string[] versions) => new()
    {
        Metadata = new V1ObjectMeta { Name = $"{kind.ToLowerInvariant()}s.{group}" },
        Spec = new V1CustomResourceDefinitionSpec
        {
            Group = group,
            Scope = "Namespaced",
            Names = new V1CustomResourceDefinitionNames { Kind = kind, Plural = kind.ToLowerInvariant() + "s" },
            Versions = [.. versions.Select((name, index) => new V1CustomResourceDefinitionVersion { Name = name, Served = true, Storage = index == 0 })],
        },
    };
}
