using Helm.NET;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Helm;

public sealed class HelmClientFactoryTests
{
    [Fact]
    public void CreateOptions_maps_connection_to_helm_options()
    {
        var connection = new HelmConnection("/home/user/.kube/config", "kind-test", "apps");

        var options = HelmClientFactory.CreateOptions(connection);

        options.Kubeconfig.ShouldBe("/home/user/.kube/config");
        options.KubeContext.ShouldBe("kind-test");
        options.Namespace.ShouldBe("apps");
    }

    [Fact]
    public void CreateOptions_uses_helm_defaults_for_empty_values()
    {
        var connection = new HelmConnection("  ", " ", null);

        var options = HelmClientFactory.CreateOptions(connection);

        options.Kubeconfig.ShouldBeNull();
        options.KubeContext.ShouldBeNull();
        options.Namespace.ShouldBe("default");
    }

    [Fact]
    public void Create_returns_client_for_connection()
    {
        var sut = new HelmClientFactory();

        var client = sut.Create(new HelmConnection("/home/user/.kube/config", "kind-test", "apps"));

        client.ShouldNotBeNull();
        client.ShouldBeOfType<HelmClient>();
    }

    [Fact]
    public void Create_throws_for_null_connection()
    {
        var sut = new HelmClientFactory();

        Should.Throw<ArgumentNullException>(() => sut.Create(null!));
    }
}
