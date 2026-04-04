using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public class MapsterConfigurationTests
{
    [Fact]
    public void SameTypeAdapter_HandlesNestedResourceQuantityWithoutThrowing()
    {
        new ServiceCollection().AddKubeUIKubernetesServices();

        V1Pod source = new V1Pod
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "pod-a",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "c1",
                        Env =
                        [
                            new V1EnvVar
                            {
                                Name = "CPU_LIMIT",
                                ValueFrom = new V1EnvVarSource
                                {
                                    ResourceFieldRef = new V1ResourceFieldSelector
                                    {
                                        Resource = "limits.cpu",
                                        Divisor = new ResourceQuantity("1m"),
                                    },
                                },
                            },
                        ],
                    },
                ],
            },
        };

        V1Pod destination = new V1Pod();

        Should.NotThrow(() => source.Adapt(destination));
    }

    [Fact]
    public void Adapter_PreservesSharedReferencesAcrossDifferentTypes()
    {
        new ServiceCollection().AddKubeUIKubernetesServices();

        ParentSource source = new ParentSource
        {
            First = new ChildSource
            {
                Name = "shared",
            },
        };
        source.Second = source.First;

        ParentDestination destination = source.Adapt<ParentDestination>();

        destination.First.ShouldNotBeNull();
        destination.Second.ShouldNotBeNull();
        destination.First.ShouldBeSameAs(destination.Second);
    }

    private sealed class ParentSource
    {
        public ChildSource? First { get; set; }

        public ChildSource? Second { get; set; }
    }

    private sealed class ChildSource
    {
        public string? Name { get; set; }
    }

    private sealed class ParentDestination
    {
        public ChildDestination? First { get; set; }

        public ChildDestination? Second { get; set; }
    }

    private sealed class ChildDestination
    {
        public string? Name { get; set; }
    }
}
