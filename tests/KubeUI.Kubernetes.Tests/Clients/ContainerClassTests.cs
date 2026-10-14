using DynamicData;
using DynamicData.Kernel;
using k8s;
using k8s.Models;
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

    [Fact]
    public void QueryTopFilters_and_keeps_only_the_first_items_in_stable_order()
    {
        var container = new ContainerClass<V1Pod>();
        container.Items.AddOrUpdate(NamedPod("zeta", "default"));
        container.Items.AddOrUpdate(NamedPod("alpha", "default"));
        container.Items.AddOrUpdate(NamedPod("beta", "other"));
        var comparer = Comparer<IKubernetesObject<V1ObjectMeta>>.Create((left, right) =>
        {
            var namespaceComparison = StringComparer.Ordinal.Compare(
                left.Metadata?.NamespaceProperty, right.Metadata?.NamespaceProperty);
            return namespaceComparison != 0
                ? namespaceComparison
                : StringComparer.Ordinal.Compare(left.Metadata?.Name, right.Metadata?.Name);
        });

        var result = container.QueryTop(
            resource => string.Equals(resource.Metadata?.NamespaceProperty, "default", StringComparison.Ordinal),
            comparer,
            1);

        result.Select(resource => resource.Metadata?.Name).ShouldBe(["alpha"]);

        var scanOrder = new Dictionary<IKubernetesObject<V1ObjectMeta>, int>(ReferenceEqualityComparer.Instance);
        IKubernetesObject<V1ObjectMeta>? lastScanned = null;
        var latestFirst = Comparer<IKubernetesObject<V1ObjectMeta>>.Create(
            (left, right) => scanOrder[right].CompareTo(scanOrder[left]));
        var replacementResult = container.QueryTop(resource =>
        {
            scanOrder.Add(resource, scanOrder.Count);
            lastScanned = resource;
            return true;
        }, latestFirst, 1);

        replacementResult.Single().ShouldBe(lastScanned);
    }

    [Fact]
    public void QueryTop_validates_arguments_and_honors_cancellation()
    {
        var container = new ContainerClass<V1Pod>();
        var comparer = Comparer<IKubernetesObject<V1ObjectMeta>>.Create((left, right) =>
            StringComparer.Ordinal.Compare(left.Metadata?.Name, right.Metadata?.Name));

        Should.Throw<ArgumentNullException>(() => container.QueryTop(null!, comparer, 1));
        Should.Throw<ArgumentNullException>(() => container.QueryTop(_ => true, null!, 1));
        Should.Throw<ArgumentOutOfRangeException>(() => container.QueryTop(_ => true, comparer, 0));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Should.Throw<OperationCanceledException>(() => container.QueryTop(
            _ => true, comparer, 1, cancellation.Token));
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

    private static V1Pod NamedPod(string name, string @namespace) => new()
    {
        Metadata = new V1ObjectMeta
        {
            NamespaceProperty = @namespace,
            Name = name,
        },
    };
}
