using k8s.Models;
using DynamicData;
using DynamicData.Kernel;
using KubeUI.Kubernetes.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clients;

public sealed class ContainerClassTests
{
    [Fact]
    public void Source_cache_uses_uid_for_resources_with_the_same_name()
    {
        ContainerClass<V1Pod> container = new();
        V1Pod first = Pod("uid-first");
        V1Pod second = Pod("uid-second");

        container.Items.Edit(updater => updater.AddOrUpdate(first));
        container.Items.Edit(updater => updater.AddOrUpdate(second));

        container.Items.Items.Count.ShouldBe(2);
        container.Items.Lookup("uid-first").ValueOrDefault().ShouldBe(first);
        container.Items.Lookup("uid-second").ValueOrDefault().ShouldBe(second);
    }

    private static V1Pod Pod(string uid)
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "default",
                Name = "same-name",
                Uid = uid,
            },
        };
    }
}
