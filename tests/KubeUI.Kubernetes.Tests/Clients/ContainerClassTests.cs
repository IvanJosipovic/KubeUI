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

    [Fact]
    public void Source_cache_removes_resource_when_delete_notification_has_no_uid()
    {
        ContainerClass<V1Secret> container = new();
        V1Secret existing = Secret("uid-existing");
        V1Secret deleted = Secret(null);

        container.Items.AddOrUpdate(existing);

        Exception? exception = Record.Exception(() => container.Remove(deleted));

        exception.ShouldBeNull();
        container.Items.Items.ShouldBeEmpty();
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

    private static V1Secret Secret(string? uid)
    {
        return new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "kube-system",
                Name = "same-name",
                Uid = uid,
            },
        };
    }
}
