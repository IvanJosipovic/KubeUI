using k8s.KubeConfigModels;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class AksClusterServiceTests
{
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
