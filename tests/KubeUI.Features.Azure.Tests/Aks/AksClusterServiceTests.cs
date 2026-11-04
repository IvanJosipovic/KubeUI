using Azure.Identity;
using k8s.KubeConfigModels;
using KubeUI.Features.Azure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Features.Azure.Tests.Aks;

public sealed class AksClusterServiceTests
{
    [Fact]
    public void AddKubeUIAzureFeature_registers_aks_service()
    {
        ServiceCollection services = [];

        services.AddKubeUIAzureFeature();

        services.ShouldContain(descriptor =>
            descriptor.ServiceType == typeof(IAksClusterService) &&
            descriptor.ImplementationType == typeof(AksClusterService));
    }

    [Fact]
    public async Task TranslateCredentialUnavailableAsync_preserves_message_and_cause()
    {
        CredentialUnavailableException cause = new("Azure CLI is not available.");

        var exception = await Should.ThrowAsync<AzureAuthenticationUnavailableException>(
            () => AksClusterService.TranslateCredentialUnavailableAsync<int>(
                () => Task.FromException<int>(cause)));

        exception.Message.ShouldBe(cause.Message);
        exception.InnerException.ShouldBeSameAs(cause);
    }

    [Fact]
    public async Task GetClustersAsync_returns_faulted_task_for_invalid_subscription_id()
    {
        AksClusterService service = new(null!);

        var operation = service.GetClustersAsync(" ");

        await Should.ThrowAsync<ArgumentException>(() => operation);
    }

    [Fact]
    public void NormalizeKubeConfigNames_uses_cluster_name_for_context_and_cluster()
    {
        K8SConfiguration kubeConfig = new()
        {
            CurrentContext = "clusterUser_rg_test-aks",
            Contexts =
            [
                new Context
                {
                    Name = "clusterUser_rg_test-aks",
                    ContextDetails = new ContextDetails
                    {
                        Cluster = "cluster-test-aks",
                        User = "clusterUser_rg_test-aks"
                    }
                }
            ],
            Clusters =
            [
                new k8s.KubeConfigModels.Cluster
                {
                    Name = "cluster-test-aks",
                    ClusterEndpoint = new ClusterEndpoint
                    {
                        Server = "https://test-aks.example.com"
                    }
                }
            ],
            Users =
            [
                new User
                {
                    Name = "clusterUser_rg_test-aks",
                    UserCredentials = new UserCredentials()
                }
            ]
        };

        AksClusterService.NormalizeKubeConfigNames(kubeConfig, "test-aks");

        kubeConfig.CurrentContext.ShouldBe("test-aks");
        kubeConfig.Contexts.ShouldHaveSingleItem().Name.ShouldBe("test-aks");
        kubeConfig.Contexts.ShouldHaveSingleItem().ContextDetails.Cluster.ShouldBe("test-aks");
        kubeConfig.Clusters.ShouldHaveSingleItem().Name.ShouldBe("test-aks");
    }
}
