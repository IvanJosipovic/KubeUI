using System.Text.Json;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes.Resources.GatewayApi;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.GatewayApi;

public sealed class GatewayApiResourceConfigTests
{
    [AvaloniaFact]
    public void gateway_api_crd_gets_gateway_api_category_and_channel()
    {
        var config = CreateConfig("gateway.networking.k8s.io", "HTTPRoute", "v1");

        config.GatewayApiChannel.ShouldBe(GatewayApiChannel.Standard);
        config.Category.ShouldBe(ResourceCategories.GatewayApi);
    }

    [AvaloniaFact]
    public void experimental_gateway_api_crd_is_marked_experimental()
    {
        var config = CreateConfig("gateway.networking.x-k8s.io", "XListenerSet", "v1alpha1");

        config.GatewayApiChannel.ShouldBe(GatewayApiChannel.Experimental);
        config.Category.ShouldBe(ResourceCategories.GatewayApi);
    }

    [AvaloniaFact]
    public void other_crd_has_no_gateway_api_channel_or_category()
    {
        var config = CreateConfig("example.com", "Widget", "v1");

        config.GatewayApiChannel.ShouldBeNull();
        config.Category.ShouldBeNull();
        config.CreateNewResource().Properties.ShouldNotContainKey("spec");
    }

    [AvaloniaFact]
    public void new_gateway_api_resource_contains_template_spec()
    {
        var config = CreateConfig("gateway.networking.k8s.io", "Gateway", "v1");

        var resource = config.CreateNewResource();

        resource.ApiVersion.ShouldBe("gateway.networking.k8s.io/v1");
        resource.Kind.ShouldBe("Gateway");
        resource.Metadata.NamespaceProperty.ShouldBe("default");
        resource.Properties["spec"].GetProperty("gatewayClassName").GetString().ShouldBe("example");
        resource.Properties["spec"].GetProperty("listeners").ValueKind.ShouldBe(JsonValueKind.Array);
    }

    private static CRDResourceConfig CreateConfig(string group, string kind, string version)
    {
        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
        config.Initialize(cluster);
        config.Configure(new V1CustomResourceDefinition
        {
            Metadata = new V1ObjectMeta { Name = $"{kind.ToLowerInvariant()}s.{group}" },
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = group,
                Scope = "Namespaced",
                Names = new V1CustomResourceDefinitionNames { Kind = kind, Plural = kind.ToLowerInvariant() + "s" },
                Versions = [new V1CustomResourceDefinitionVersion { Name = version, Served = true, Storage = true }],
            },
        });
        return config;
    }
}
