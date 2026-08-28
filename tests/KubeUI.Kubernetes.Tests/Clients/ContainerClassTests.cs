using k8s.Models;
using DynamicData;
using DynamicData.Kernel;
using KubeUI.Kubernetes.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clients;

public sealed class ContainerClassTests
{
    [Fact]
    public void SourceCacheUsesNamespaceAndNameAsResourceIdentity()
    {
        var container = new ContainerClass<V1Pod>();
        var first = Pod();
        var second = Pod();
        var otherNamespace = Pod("other");

        container.Items.Edit(updater => updater.AddOrUpdate(first));
        container.Items.Edit(updater => updater.AddOrUpdate(second));
        container.Items.Edit(updater => updater.AddOrUpdate(otherNamespace));

        container.Items.Items.Count.ShouldBe(2);
        container.Items.Lookup(new ResourceCacheKey("default", "same-name")).ValueOrDefault().ShouldBe(second);
        container.Items.Lookup(new ResourceCacheKey("other", "same-name")).ValueOrDefault().ShouldBe(otherNamespace);
        container.Items.Items.ShouldContain(second);
        container.Items.Items.ShouldContain(otherNamespace);
    }

    [Fact]
    public void SourceCacheAcceptsResourceWithoutUid()
    {
        var container = new ContainerClass<V1Secret>();
        var resource = Secret();

        var exception = Record.Exception(() => container.Items.AddOrUpdate(resource));

        exception.ShouldBeNull();
        container.Items.Items.ShouldContain(resource);
    }

    [Fact]
    public void SourceCacheRemovesResourceWhenDeleteNotificationHasNoUid()
    {
        var container = new ContainerClass<V1Secret>();
        var existing = Secret();
        var deleted = Secret();

        container.Items.AddOrUpdate(existing);

        var exception = Record.Exception(() => container.Items.Remove(deleted));

        exception.ShouldBeNull();
        container.Items.Items.ShouldBeEmpty();
    }

    private static V1Pod Pod(string @namespace = "default")
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = @namespace,
                Name = "same-name",
            },
        };
    }

    private static V1Secret Secret()
    {
        return new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "kube-system",
                Name = "same-name",
            },
        };
    }
}
