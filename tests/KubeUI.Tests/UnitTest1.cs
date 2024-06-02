using FluentAssertions;
using k8s.Models;
using KubeUI.Client;

namespace TestProject1;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        GroupApiVersionKind.From<V1Pod>().Kind.Should().Be("Pod");
    }
}
