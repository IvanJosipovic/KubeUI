
namespace KubeUI.Core.Tests
{
    public class CompareClusterTests
    {
        private ServiceProvider GetServices()
        {
            var service = new ServiceCollection();
            service.AddLogging();
            service.AddSingleton<ICRDGenerator, CRDGenerator>();
            service.AddTransient<GitOpsCluster>();

            return service.BuildServiceProvider();
        }

        [Fact]
        public void Modified()
        {
            var services = GetServices();

            var left = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 1
                }
            };

            var right = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 2
                }
            };

            var leftCluster = services.GetRequiredService<GitOpsCluster>();

            leftCluster.AddInternalObject(left);

            var rightCluster = services.GetRequiredService<GitOpsCluster>();

            rightCluster.AddInternalObject(right);

            var result = Components.CompareCluster.Compare(leftCluster, rightCluster);

            result.Count.Should().Be(1);
            result[0].Key.Should().Be("ns|test");
            result[0].Name = "test";
            result[0].Namespace = "ns";
            result[0].Result.Should().Be(Components.CompareCluster.CompareObjectResultEnum.Modified);
        }

        [Fact]
        public void Added()
        {
            var services = GetServices();

            var right = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 2
                }
            };

            var leftCluster = services.GetRequiredService<GitOpsCluster>();

            var rightCluster = services.GetRequiredService<GitOpsCluster>();

            rightCluster.AddInternalObject(right);

            var result = Components.CompareCluster.Compare(leftCluster, rightCluster);

            result.Count.Should().Be(1);
            result[0].Key.Should().Be("ns|test");
            result[0].Name = "test";
            result[0].Namespace = "ns";
            result[0].Result.Should().Be(Components.CompareCluster.CompareObjectResultEnum.Added);
        }

        [Fact]
        public void Removed()
        {
            var services = GetServices();

            var right = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 2
                }
            };

            var leftCluster = services.GetRequiredService<GitOpsCluster>();

            leftCluster.AddInternalObject(right);

            var rightCluster = services.GetRequiredService<GitOpsCluster>();

            var result = Components.CompareCluster.Compare(leftCluster, rightCluster);

            result.Count.Should().Be(1);
            result[0].Key.Should().Be("ns|test");
            result[0].Name = "test";
            result[0].Namespace = "ns";
            result[0].Result.Should().Be(Components.CompareCluster.CompareObjectResultEnum.Removed);
        }

        [Fact]
        public void AddedRemoved()
        {
            var services = GetServices();

            var left = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 2
                }
            };

            var right = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test2",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 2
                }
            };

            var leftCluster = services.GetRequiredService<GitOpsCluster>();

            leftCluster.AddInternalObject(left);

            var rightCluster = services.GetRequiredService<GitOpsCluster>();

            rightCluster.AddInternalObject(right);

            var result = Components.CompareCluster.Compare(leftCluster, rightCluster);

            result.Count.Should().Be(2);
            result[0].Key.Should().Be("ns|test");
            result[0].Name = "test";
            result[0].Namespace = "ns";
            result[0].Result.Should().Be(Components.CompareCluster.CompareObjectResultEnum.Removed);
            result[1].Key.Should().Be("ns|test2");
            result[1].Name = "test2";
            result[1].Namespace = "ns";
            result[1].Result.Should().Be(Components.CompareCluster.CompareObjectResultEnum.Added);
        }

        [Fact]
        public void Equal()
        {
            var services = GetServices();
            var left = new V1Deployment()
            {
                ApiVersion = V1Deployment.KubeApiVersion,
                Kind = V1Deployment.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    NamespaceProperty = "ns"
                },
                Spec = new V1DeploymentSpec()
                {
                    MinReadySeconds = 1
                }
            };

            var leftCluster = services.GetRequiredService<GitOpsCluster>();

            leftCluster.AddInternalObject(left);

            var rightCluster = services.GetRequiredService<GitOpsCluster>();

            rightCluster.AddInternalObject(left);

            var result = Components.CompareCluster.Compare(leftCluster, rightCluster);

            result.Count.Should().Be(1);
            result[0].Key.Should().Be("ns|test");
            result[0].Name = "test";
            result[0].Namespace = "ns";
            result[0].Result.Should().Be(Components.CompareCluster.CompareObjectResultEnum.None);
        }
    }
}