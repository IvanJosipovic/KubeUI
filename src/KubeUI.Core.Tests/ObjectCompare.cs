using k8s.Models;
using KubeUI.Core.Client;

namespace KubeUI.Core.Tests
{
    public class ObjectCompareTests
    {
        [Fact]
        public void Test1()
        {
            var left = new V1Deployment()
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "test"
                },
                Spec = new V1DeploymentSpec()
                {
                    Template = new V1PodTemplateSpec()
                    {
                        Spec = new V1PodSpec()
                        {
                            Containers = new List<V1Container>()
                            {
                                new V1Container()
                                {
                                    Name = "test1",
                                    Image = "test1",
                                    Args = new List<string>(),
                                    Ports= new List<V1ContainerPort>()
                                    {
                                        new V1ContainerPort()
                                        {
                                            Name = "Bob",
                                            ContainerPort = 1234,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var right = new V1Deployment()
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "test"
                },
                Spec = new V1DeploymentSpec()
                {
                    Template = new V1PodTemplateSpec()
                    {
                        Spec = new V1PodSpec()
                        {
                            Containers = new List<V1Container>()
                            {
                                new V1Container()
                                {
                                    Name = "test2",
                                    Image = "test2",
                                    Ports= new List<V1ContainerPort>()
                                    {
                                        new V1ContainerPort()
                                        {
                                            Name = "Bob",
                                            ContainerPort = 123,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = ObjectCompare.CompareObjects(left, right);

            var kind = result.Kind;
        }
    }
}