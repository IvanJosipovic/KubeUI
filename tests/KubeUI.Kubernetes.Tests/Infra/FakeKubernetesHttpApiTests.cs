using k8s;
using k8s.Models;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class FakeKubernetesHttpApiTests
{
    [Fact]
    public async Task RealClientUsesFakeHttpTransportForCrud()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Namespace>();

        using var client = new k8s.Kubernetes(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            api);

        var created = await client.CreateNamespaceAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" },
        }, cancellationToken: TestContext.Current.CancellationToken);

        var listed = await client.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken);

        created.Name().ShouldBe("test");
        listed.Items.ShouldContain(item => item.Name() == "test");
        api.RequestUris.ShouldContain(uri => uri != null && uri.AbsolutePath == "/api/v1/namespaces");
    }

    [Fact]
    public async Task GenericClientUsesFakeHttpTransportForCrud()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Namespace>();
        using var client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://fake-kubernetes" }, api);
        using var generic = client.GetGenericClient<V1Namespace>();

#pragma warning disable xUnit1051 // GenericClient.CreateAsync does not expose the token parameter by name in this client version.
        var created = await generic.CreateAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" },
        });
#pragma warning restore xUnit1051

        created.Name().ShouldBe("test");
    }
}
