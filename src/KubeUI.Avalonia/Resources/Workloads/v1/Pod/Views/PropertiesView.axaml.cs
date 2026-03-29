using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

public partial class PropertiesView : UserControl
{
    public PropertiesView()
    {
        InitializeComponent();
#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new()
                {
                    Name = "actions-runner-controller-7cb57759b5-ddhbq",
                    NamespaceProperty = "actions-runner-system",
                    CreationTimestamp = DateTime.Parse("2025-02-06T22:11:36Z"),
                    Labels = new System.Collections.Generic.Dictionary<string, string> { { "app", "actions-runner-controller" }, { "controller", "true" }, { "env", "prod" } },
                    Annotations = new System.Collections.Generic.Dictionary<string, string> { { "annotation1", "value1" }, { "annotation2", "value2" }, { "annotation3", "value3" }, { "annotation4", "value4" } },
                    OwnerReferences = [
                        new(){
                            Controller = true,
                            Name = "ReplicaSet actions-runner-controller-7cb57759b5"
                        }
                    ]
                },
                Spec = new()
                {
                    NodeName = "r720",
                    ServiceAccountName = "actions-runner-controller",
                    Tolerations = [ new() { Key = "example", Value = "true" } ],
                    ImagePullSecrets = [ new() { Name = "controller-manager" }, new() { Name = "actions-runner-controller-serving-cert" } ],
                    Volumes = [
                        new() { Name = "secret-vol", Secret = new() { SecretName = "controller-manager" } },
                        new() { Name = "empty-dir-vol", EmptyDir = new() { } },
                        new() { Name = "projected-vol", Projected = new() { Sources = [] } }
                    ],
                    InitContainers = [
                        new(){
                            Name = "init1",
                            Image = "summerwind/actions-runner-controller:v0.27.6",
                            ImagePullPolicy = "Always",
                            Ports = [ new() { Name = "webhook-server", ContainerPort = 9443, Protocol = "TCP" } ],
                            Env = [ new() { Name = "ENV1", Value = "VAL1" }, new() { Name = "ENV2", Value = "VAL2" } ],
                            VolumeMounts = [ new() { Name = "secret-vol", MountPath = "/mnt/secret" } ],
                            Command = [ "/manager" ],
                            Args = [ "--metrics-addr=127.0.0.1:8080", "--enable-leader-election" ],
                            Resources = new k8s.Models.V1ResourceRequirements {
                                Requests = new System.Collections.Generic.Dictionary<string, k8s.Models.ResourceQuantity> {
                                    { "cpu", new k8s.Models.ResourceQuantity("100m") },
                                    { "memory", new k8s.Models.ResourceQuantity("128Mi") }
                                },
                                Limits = new System.Collections.Generic.Dictionary<string, k8s.Models.ResourceQuantity> {
                                    { "cpu", new k8s.Models.ResourceQuantity("200m") },
                                    { "memory", new k8s.Models.ResourceQuantity("256Mi") }
                                }
                            }
                        }
                    ],
                    Containers = [
                        new(){
                            Name = "manager",
                            Image = "summerwind/actions-runner-controller:v0.27.6",
                            ImagePullPolicy = "Always",
                            Ports = [ new() { Name = "webhook-server", ContainerPort = 9443, Protocol = "TCP" } ],
                            Env = [ new() { Name = "ENV1", Value = "VAL1" }, new() { Name = "ENV2", Value = "VAL2" }, new() { Name = "ENV3", Value = "VAL3" }, new() { Name = "ENV4", Value = "VAL4" }, new() { Name = "ENV5", Value = "VAL5" } ],
                            VolumeMounts = [ new() { Name = "secret-vol", MountPath = "/mnt/secret" }, new() { Name = "empty-dir-vol", MountPath = "/mnt/empty" }, new() { Name = "projected-vol", MountPath = "/mnt/projected" }, new() { Name = "other-vol", MountPath = "/mnt/other" } ],
                            Command = [ "/manager" ],
                            Args = [ "--metrics-addr=127.0.0.1:8080", "--enable-leader-election", "--port=9443", "--sync-period=1m" ],
                            Resources = new k8s.Models.V1ResourceRequirements {
                                Requests = new System.Collections.Generic.Dictionary<string, k8s.Models.ResourceQuantity> {
                                    { "cpu", new k8s.Models.ResourceQuantity("100m") },
                                    { "memory", new k8s.Models.ResourceQuantity("128Mi") }
                                },
                                Limits = new System.Collections.Generic.Dictionary<string, k8s.Models.ResourceQuantity> {
                                    { "cpu", new k8s.Models.ResourceQuantity("200m") },
                                    { "memory", new k8s.Models.ResourceQuantity("256Mi") }
                                }
                            }
                        },
                        new(){
                            Name = "kube-rbac-proxy",
                            Image = "quay.io/brancz/kube-rbac-proxy:v0.13.1",
                            ImagePullPolicy = "Always",
                            Ports = [ new() { Name = "metrics-port", ContainerPort = 8443, Protocol = "TCP" } ],
                            Env = [],
                            VolumeMounts = [ new() { Name = "kube-api-access", MountPath = "/var/run/secrets/kubernetes.io/serviceaccount" } ],
                            Command = [ "--secure-listen-address=0.0.0.0:8443" ],
                            Args = [ "--upstream=http://127.0.0.1:8080/", "--logtostderr=true", "--v=10" ],
                            Resources = new k8s.Models.V1ResourceRequirements {
                                Requests = new System.Collections.Generic.Dictionary<string, k8s.Models.ResourceQuantity>(),
                                Limits = new System.Collections.Generic.Dictionary<string, k8s.Models.ResourceQuantity>()
                            }
                        }
                    ]
                },
                Status = new()
                {
                    Phase = "Running",
                    PodIP = "10.1.43.156",
                    PodIPs = [ new() { Ip = "10.1.43.156" } ],
                    QosClass = "BestEffort",
                    Conditions = [
                        new() { Type = "PodReady", Status = "True" },
                        new() { Type = "ContainersReady", Status = "True" },
                        new() { Type = "PodScheduled", Status = "True" }
                    ]
                }
            };
        }
#endif
    }
}

