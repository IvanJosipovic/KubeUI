using Avalonia;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
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
}
