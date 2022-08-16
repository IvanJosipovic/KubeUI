using FluentAssertions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace KubeCRDGenerator.Tests;

public class UnitTest1
{
    private static ServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<ICRDGenerator, CRDGenerator>();
        services.AddHttpClient();

        return services.BuildServiceProvider();
    }

    private static ICRDGenerator GetCRDGenerator()
    {
        return GetServiceProvider().GetRequiredService<ICRDGenerator>();
    }

    private static async Task<Type?> GetType(string filename, string kind)
    {
        var crd = await KubernetesYaml.LoadAllFromFileAsync(filename);

        var assembly = await GetCRDGenerator().GenerateAssembly((V1CustomResourceDefinition)crd[0], "KubeCRDGenerator.Tests.Models");

        var types = assembly.Item1.DefinedTypes.Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(KubernetesEntityAttribute) && y.NamedArguments.Any(z => z.MemberName == "Kind" && z.TypedValue.Value.Equals(kind))));

        return types.First();
    }

    [Fact]
    public async Task TestNamespace()
    {
        var type = await GetType("CRDs/1.yaml", "Alert");
        type.Namespace.Should().Be("KubeCRDGenerator.Tests.Models");
    }

    [Fact]
    public async Task TestKubernetesEntity()
    {
        var type = await GetType("CRDs/1.yaml", "Alert");

        type.GetProperty("ApiVersion").PropertyType.Should().NotBeNull();
        type.GetProperty("Kind").PropertyType.Should().NotBeNull();
        type.GetProperty("Metadata").PropertyType.Should().NotBeNull();

        var attribute = type.CustomAttributes.First(x => x.AttributeType == typeof(KubernetesEntityAttribute));

        var kind = attribute.NamedArguments.First(x => x.MemberName == "Kind");
        kind.TypedValue.Value.Should().Be("Alert");

        var group = attribute.NamedArguments.First(x => x.MemberName == "Group");
        group.TypedValue.Value.Should().Be("my.group.com");

        var version = attribute.NamedArguments.First(x => x.MemberName == "ApiVersion");
        version.TypedValue.Value.Should().Be("v1beta1");

        var plural = attribute.NamedArguments.First(x => x.MemberName == "PluralName");
        plural.TypedValue.Value.Should().Be("alerts");
    }

    [Fact]
    public async Task TestString()
    {
        var type = await GetType("CRDs/1.yaml", "Alert");

        var specType = type.GetProperty("Spec").PropertyType;

        specType.GetProperty("EnumString", typeof(string)).Should().NotBeNull();
    }

    [Fact]
    public async Task TestBool()
    {
        var type = await GetType("CRDs/1.yaml", "Alert");

        var specType = type.GetProperty("Spec").PropertyType;

        specType.GetProperty("Suspend", typeof(bool?)).Should().NotBeNull();
    }

    [Fact]
    public async Task TestInt()
    {
        var type = await GetType("CRDs/1.yaml", "Alert");

        var specType = type.GetProperty("Spec").PropertyType;

        specType.GetProperty("IntProp", typeof(int?)).Should().NotBeNull();
    }

    [Fact]
    public async Task TestInt64()
    {
        var type = await GetType("CRDs/1.yaml", "Alert");

        var specType = type.GetProperty("Spec").PropertyType;

        specType.GetProperty("Int64Prop", typeof(long?)).Should().NotBeNull();
    }

    [Fact]
    public async Task UnkownFields()
    {
        var type = await GetType("CRDs/sealedsecret.yaml", "SealedSecret");

        var specType = type.GetProperty("Spec").PropertyType;

        var prop = specType.GetProperty("ExtensionData", typeof(Dictionary<string, object>));

        prop.Should().NotBeNull();

        prop.CustomAttributes.Any(y => y.AttributeType == typeof(JsonExtensionDataAttribute)).Should().BeTrue();

        type.GetProperty("ApiVersion").PropertyType.Should().NotBeNull();
        type.GetProperty("Kind").PropertyType.Should().NotBeNull();
        type.GetProperty("Metadata").PropertyType.Should().NotBeNull();
    }

    [Fact]
    public async Task GitRepository()
    {
        var type = await GetType("CRDs/gitrepository.yaml", "GitRepository");

        var specType = type.GetProperty("Spec").PropertyType;

        specType.GetProperty("GitImplementation", typeof(string)).Should().NotBeNull();
    }

    [Fact]
    public async Task FlexibleServer()
    {
        var type = await GetType("CRDs/flexibleserver.yaml", "FlexibleServer");

        var specType = type.GetProperty("Spec").PropertyType;

        var prop = specType.GetProperty("AdministratorLogin", typeof(string)).Should().NotBeNull();
    }

    [Fact]
    public async Task HelmRelease()
    {
        var type = await GetType("CRDs/helmrelease.yaml", "HelmRelease");

        var specType = type.GetProperty("Spec").PropertyType;

        specType.GetProperty("Values").PropertyType.Should().Be<JsonNode?>();
    }

    [Fact]
    public async Task ClusterPolicyReport()
    {
        var type = await GetType("CRDs/clusterpolicyreport.yaml", "ClusterPolicyReport");

        type.Should().NotBeNull();
    }
}
