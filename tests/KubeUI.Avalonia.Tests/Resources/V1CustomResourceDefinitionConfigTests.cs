using Avalonia;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public class V1CustomResourceDefinitionConfigTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void list_crd_command_does_not_throw_when_type_is_unavailable()
    {
        var cluster = new TestCluster().CreateWorkspace();
        var services = Application.Current.GetRequiredService<IServiceProvider>();
        var config = ActivatorUtilities.CreateInstance<V1CustomResourceDefinitionConfig>(services);
        config.Initialize(cluster);

        var crd = new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Names = new V1CustomResourceDefinitionNames
                {
                    Kind = "Example",
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

        Should.NotThrow(() =>
        {
            config.ListCRDCommand.CanExecute(crd).ShouldBeFalse();
        });
    }

    [AvaloniaFact]
    public void generate_uses_humanized_plural_kind_for_display_name()
    {
        var cluster = new TestCluster().CreateWorkspace();
        var services = Application.Current.GetRequiredService<IServiceProvider>();
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
}

[KubernetesEntity(Group = "example.com", ApiVersion = "v1", Kind = "IngressClass")]
internal sealed class TestCustomResource : k8s.IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "example.com/v1";
    public string Kind { get; set; } = "IngressClass";
    public V1ObjectMeta Metadata { get; set; } = new();
}
