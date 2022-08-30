using FluentAssertions;
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

        [Fact]
        public void CleanMetadata()
        {
            var left = new V1Deployment()
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "test",
                    Generation = 213,
                    CreationTimestamp = DateTime.UtcNow,
                    Finalizers = new List<string>() { "test" },
                    ManagedFields = new List<V1ManagedFieldsEntry>() { new V1ManagedFieldsEntry() { Operation = "test" } },
                    ResourceVersion = "1",
                    SelfLink = "asd",
                    Uid = "asd"
                },
            };

            var result = ObjectCompare.CleanObject(left);

            result.Metadata.Generation.Should().BeNull();
            result.Metadata.CreationTimestamp.Should().BeNull();
            result.Metadata.Finalizers.Should().BeNull();
            result.Metadata.ManagedFields.Should().BeNull();
            result.Metadata.ResourceVersion.Should().BeNull();
            result.Metadata.SelfLink.Should().BeNull();
            result.Metadata.Uid.Should().BeNull();
        }

        [Fact]
        public void CleanStatus()
        {
            var left = new V1Deployment()
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "test"
                },
                Status = new V1DeploymentStatus()
                {
                    AvailableReplicas = 1,
                }
            };

            var result = ObjectCompare.CleanObject(left);

            var prop = result.GetType().GetProperty("Status");

            prop.GetValue(result).Should().BeNull();
        }
    }
}