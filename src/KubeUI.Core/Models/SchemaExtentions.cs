using KubeUI.Core;
using KubeUI.Schema;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace KubeUI.SchemaExtentions
{
    public partial class ConfigMap
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [Ignore]
        public Dictionary<string, byte[]> BinaryData;

        [ReadOnly(true)]
        public string Kind;
    }

    public partial class Container
    {
        public Collection<string> Args;

        public Collection<string> Command;

        [DisplayInTree(DisplayName = "Name")]
        public Collection<EnvVar> Env;

        [DisplayInTree(DisplayName = "")]
        public Collection<EnvFromSource> EnvFrom;

        [SelectList(Options = new string[] { "IfNotPresent", "Always", "Never" })]
        public string ImagePullPolicy;

        [UILevel(UILevel = UILevel.Advanced)]
        public Schema.Lifecycle Lifecycle;

        [UILevel(UILevel = UILevel.Advanced)]
        public Probe LivenessProbe;

        public Collection<Schema.ContainerPort> Ports;

        [UILevel(UILevel = UILevel.Advanced)]
        public Schema.Probe ReadinessProbe;

        [UILevel(UILevel = UILevel.Advanced)]
        public ResourceRequirements Resources;

        [UILevel(UILevel = UILevel.Advanced)]
        public SecurityContext SecurityContext;

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? Stdin;

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? StdinOnce;

        [UILevel(UILevel = UILevel.Advanced)]
        public string TerminationMessagePath;

        [UILevel(UILevel = UILevel.Advanced)]
        public string TerminationMessagePolicy;

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? Tty;

        [UILevel(UILevel = UILevel.Advanced)]
        public Collection<VolumeDevice> VolumeDevices;

        public Collection<VolumeMount> VolumeMounts;

        [UILevel(UILevel = UILevel.Advanced)]
        public string WorkingDir;
    }

    public partial class ContainerPort
    {
        /// <summary>Number of port to expose on the pod's IP address. This must be a valid port number, 0 &lt; x &lt; 65536.</summary>
        public int ContainerPort1 { get; set; }

        /// <summary>What host IP to bind the external port to.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string HostIP { get; set; }

        /// <summary>Number of port to expose on the host. If specified, this must be a valid port number, 0 &lt; x &lt; 65536. If HostNetwork is specified, this must match ContainerPort. Most containers do not need this.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public int? HostPort { get; set; }

        /// <summary>If specified, this must be an IANA_SVC_NAME and unique within the pod. Each named port in a pod must have a unique name. Name for the port that can be referred to by services.</summary>
        public string Name { get; set; }

        /// <summary>Protocol for port. Must be UDP, TCP, or SCTP. Defaults to "TCP".</summary>
        [SelectList(Options = new string[] { "TCP", "UDP", "SCTP" })]
        [UILevel(UILevel = UILevel.Advanced)]
        public string Protocol { get; set; }
    }

    public partial class Deployment
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [ReadOnly(true)]
        public string Kind;
    }

    public partial class DeploymentSpec
    {
        [Ignore]
        public bool? Paused;
    }

    [Ignore]
    public partial class DeploymentStatus
    {
    }

    [UILevel(UILevel = UILevel.Advanced)]
    public partial class DeploymentStrategy
    {
        [SelectList(Options = new string[] { "RollingUpdate", "Recreate" })]
        public string Type;
    }

    public partial class EnvVar
    {
        [UILevel(UILevel = UILevel.Advanced)]
        public EnvVarSource ValueFrom;
    }

    public partial class HTTPIngressRuleValue2
    {
        [DisplayInTree(DisplayName = "Path")]
        public Collection<HTTPIngressPath2> Paths;
    }

    public partial class Ingress2
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [ReadOnly(true)]
        public string Kind;

        [Ignore]
        public IngressStatus2 Status;
    }

    public partial class IngressSpec2
    {
        [DisplayInTree(DisplayName = "Host")]
        public Collection<IngressRule2> Rules;

        [DisplayInTree(DisplayName = "SecretName")]
        public Collection<IngressTLS2> Tls;
    }

    [Ignore]
    public partial class Initializers
    {
    }

    public partial class LabelSelector
    {
        [DisplayInTree(DisplayName = "Key")]
        public Collection<LabelSelectorRequirement> MatchExpressions;
    }

    public partial class LabelSelectorRequirement
    {
        [SelectList(Options = new string[] { "In", "NotIn", "Exists", "DoesNotExist" })]
        public string Operator;
    }

    public partial class ObjectMeta
    {
        [UILevel(UILevel = UILevel.Expert)]
        public string ClusterName;

        [Ignore]
        public System.DateTimeOffset? CreationTimestamp;

        [Ignore]
        public long? DeletionGracePeriodSeconds;

        [Ignore]
        public System.DateTimeOffset? DeletionTimestamp;

        [Ignore]
        public Collection<string> Finalizers;

        [Ignore]
        public string GenerateName;

        [Ignore]
        public long? Generation;

        [Ignore]
        public Collection<ManagedFieldsEntry> ManagedFields;

        [Ignore]
        public Collection<Schema.OwnerReference> OwnerReferences;

        [Ignore]
        public string ResourceVersion;

        [Ignore]
        public string SelfLink;

        [Ignore]
        public string Uid;
    }

    public partial class PersistentVolumeClaim
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [ReadOnly(true)]
        public string Kind;

        [Ignore]
        public PersistentVolumeClaimStatus Status;
    }

    public partial class PersistentVolumeClaimSpec
    {
        /// <summary>AccessModes contains the desired access modes the volume should have. More info: https://kubernetes.io/docs/concepts/storage/persistent-volumes#access-modes-1</summary>
        [SelectList(Options = new string[] { "ReadWriteOnce", "ReadOnlyMany", "ReadWriteMany" })]
        public Collection<string> AccessModes;

        [UILevel(UILevel = UILevel.Advanced)]
        public TypedLocalObjectReference DataSource;

        [UILevel(UILevel = UILevel.Advanced)]
        public string VolumeMode;
    }

    [UILevel(UILevel = UILevel.Advanced)]
    public partial class PodDNSConfig
    {
    }

    [UILevel(UILevel = UILevel.Advanced)]
    public partial class PodSecurityContext
    {
    }

    public partial class PodSpec
    {
        [UILevel(UILevel = UILevel.Advanced)]
        public Affinity Affinity;

        [DisplayInTree(DisplayName = "Image")]
        public Collection<Container> Containers;

        [SelectList(Options = new string[] { "ClusterFirst", "ClusterFirstWithHostNet", "Default", "None" })]
        public string DnsPolicy;

        [DisplayInTree(DisplayName = "Ip")]
        public Collection<HostAlias> HostAliases;

        [Ignore]
        public Collection<Container> InitContainers;

        [SelectList(Options = new string[] { "Always", "Always", "Never" })]
        public string RestartPolicy;

        [Ignore]
        public string ServiceAccount;

        [DisplayInTree(DisplayName = "Name")]
        public Collection<Schema.Volume> Volumes;
    }

    public partial class NodeAffinity
    {
        /// <summary>The scheduler will prefer to schedule pods to nodes that satisfy the affinity expressions specified by this field, but it may choose a node that violates one or more of the expressions. The node that is most preferred is the one with the greatest sum of weights, i.e. for each node that meets all of the scheduling requirements (resource request, requiredDuringScheduling affinity expressions, etc.), compute a sum by iterating through the elements of this field and adding "weight" to the sum if the node matches the corresponding matchExpressions; the node(s) with the highest sum are the most preferred.</summary>
        [DisplayInTree(DisplayName = "Weight")]
        public System.Collections.ObjectModel.Collection<PreferredSchedulingTerm> PreferredDuringSchedulingIgnoredDuringExecution { get; set; }

        /// <summary>If the affinity requirements specified by this field are not met at scheduling time, the pod will not be scheduled onto the node. If the affinity requirements specified by this field cease to be met at some point during pod execution (e.g. due to an update), the system may or may not try to eventually evict the pod from its node.</summary>
        public NodeSelector RequiredDuringSchedulingIgnoredDuringExecution { get; set; }


    }
    public partial class NodeSelector
    {
        /// <summary>Required. A list of node selector terms. The terms are ORed.</summary>
        [DisplayInTree()]
        public System.Collections.ObjectModel.Collection<NodeSelectorTerm> NodeSelectorTerms { get; set; } = new System.Collections.ObjectModel.Collection<NodeSelectorTerm>();


    }

    public partial class NodeSelectorRequirement
    {
        /// <summary>The label key that the selector applies to.</summary>
        public string Key { get; set; }

        /// <summary>Represents a key's relationship to a set of values. Valid operators are In, NotIn, Exists, DoesNotExist. Gt, and Lt.</summary>
        [SelectList(Options = new string[] { "In", "NotIn", "Exists", "DoesNotExist", "Gt", "Lt" })]
        public string Operator { get; set; }

        /// <summary>An array of string values. If the operator is In or NotIn, the values array must be non-empty. If the operator is Exists or DoesNotExist, the values array must be empty. If the operator is Gt or Lt, the values array must have a single element, which will be interpreted as an integer. This array is replaced during a strategic merge patch.</summary>
        public System.Collections.ObjectModel.Collection<string> Values { get; set; }
    }

    [UILevel(UILevel = UILevel.Advanced)]
    public partial class RollingUpdateDeployment
    {
    }

    public partial class Secret
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [Ignore]
        public Dictionary<string, byte[]> Data;

        [ReadOnly(true)]
        public string Kind;

        /// <summary>Used to facilitate programmatic handling of secret data.</summary>
        public string Type;
    }

    //public partial class HTTPIngressPath2
    //{
    //    [DisplayInTree(DisplayName = "ServiceName")]
    //    public IngressBackend2 Backend;
    //}
    public partial class Service
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [ReadOnly(true)]
        public string Kind;

        [Ignore]
        public ServiceStatus Status;
    }

    public partial class ServiceSpec
    {
        /// <summary>clusterIP is the IP address of the service and is usually assigned randomly by the master. If an address is specified manually and is not in use by others, it will be allocated to the service; otherwise, creation of the service will fail. This field can not be changed through updates. Valid values are "None", empty string (""), or a valid IP address. "None" can be specified for headless services when proxying is not required. Only applies to types ClusterIP, NodePort, and LoadBalancer. Ignored if type is ExternalName. More info: https://kubernetes.io/docs/concepts/services-networking/service/#virtual-ips-and-service-proxies</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string ClusterIP;

        /// <summary>externalIPs is a list of IP addresses for which nodes in the cluster will also accept traffic for this service.  These IPs are not managed by Kubernetes.  The user is responsible for ensuring that traffic arrives at a node with this IP.  A common example is external load-balancers that are not part of the Kubernetes system.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public Collection<string> ExternalIPs;

        /// <summary>externalName is the external reference that kubedns or equivalent will return as a CNAME record for this service. No proxying will be involved. Must be a valid RFC-1123 hostname (https://tools.ietf.org/html/rfc1123) and requires Type to be ExternalName.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string ExternalName;

        /// <summary>externalTrafficPolicy denotes if this Service desires to route external traffic to node-local or cluster-wide endpoints. "Local" preserves the client source IP and avoids a second hop for LoadBalancer and Nodeport type services, but risks potentially imbalanced traffic spreading. "Cluster" obscures the client source IP and may cause a second hop to another node, but should have good overall load-spreading.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string ExternalTrafficPolicy;

        /// <summary>healthCheckNodePort specifies the healthcheck nodePort for the service. If not specified, HealthCheckNodePort is created by the service api backend with the allocated nodePort. Will use user-specified nodePort value if specified by the client. Only effects when Type is set to LoadBalancer and ExternalTrafficPolicy is set to Local.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public int? HealthCheckNodePort;

        /// <summary>Only applies to Service Type: LoadBalancer LoadBalancer will get created with the IP specified in this field. This feature depends on whether the underlying cloud-provider supports specifying the loadBalancerIP when a load balancer is created. This field will be ignored if the cloud-provider does not support the feature.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string LoadBalancerIP;

        /// <summary>If specified and supported by the platform, this will restrict traffic through the cloud-provider load-balancer will be restricted to the specified client IPs. This field will be ignored if the cloud-provider does not support the feature." More info: https://kubernetes.io/docs/tasks/access-application-cluster/configure-cloud-provider-firewall/</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public Collection<string> LoadBalancerSourceRanges;

        /// <summary>The list of ports that are exposed by this service. More info: https://kubernetes.io/docs/concepts/services-networking/service/#virtual-ips-and-service-proxies</summary>
        public Collection<ServicePort> Ports;

        /// <summary>publishNotReadyAddresses, when set to true, indicates that DNS implementations must publish the notReadyAddresses of subsets for the Endpoints associated with the Service. The default value is false. The primary use case for setting this field is to use a StatefulSet's Headless Service to propagate SRV records for its Pods without respect to their readiness for purpose of peer discovery.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public bool? PublishNotReadyAddresses;

        /// <summary>Supports "ClientIP" and "None". Used to maintain session affinity. Enable client IP based session affinity. Must be ClientIP or None. Defaults to None. More info: https://kubernetes.io/docs/concepts/services-networking/service/#virtual-ips-and-service-proxies</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        [SelectList(Options = new string[] { "None", "ClusterIP" })]
        public string SessionAffinity;

        /// <summary>type determines how the Service is exposed. Defaults to ClusterIP. Valid options are ExternalName, ClusterIP, NodePort, and LoadBalancer. "ExternalName" maps to the specified externalName. "ClusterIP" allocates a cluster-internal IP address for load-balancing to endpoints. Endpoints are determined by the selector or if that is not specified, by manual construction of an Endpoints object. If clusterIP is "None", no virtual IP is allocated and the endpoints are published as a set of endpoints rather than a stable IP. "NodePort" builds on ClusterIP and allocates a port on every node which routes to the clusterIP. "LoadBalancer" builds on NodePort and creates an external load-balancer (if supported in the current cloud) which routes to the clusterIP. More info: https://kubernetes.io/docs/concepts/services-networking/service/#publishing-services-service-types</summary>
        [SelectList(Options = new string[] { "ClusterIP", "NodePort", "LoadBalancer", "ExternalName" })]
        public string Type;
    }

    /// <summary>ServicePort contains information on service's port.</summary>
    public partial class ServicePort
    {
        /// <summary>The name of this port within the service. This must be a DNS_LABEL. All ports within a ServiceSpec must have unique names. This maps to the 'Name' field in EndpointPort objects. Optional if only one ServicePort is defined on this service.</summary>
        public string Name { get; set; }

        /// <summary>The port on each node on which this service is exposed when type=NodePort or LoadBalancer. Usually assigned by the system. If specified, it will be allocated to the service if unused or else creation of the service will fail. Default is to auto-allocate a port if the ServiceType of this Service requires one. More info: https://kubernetes.io/docs/concepts/services-networking/service/#type-nodeport</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public int? NodePort { get; set; }

        /// <summary>The port that will be exposed by this service.</summary>
        public int Port { get; set; }

        /// <summary>The IP protocol for this port. Supports "TCP", "UDP", and "SCTP". Default is TCP.</summary>
        [SelectList(Options = new string[] { "TCP", "UDP", "SCTP" })]
        [UILevel(UILevel = UILevel.Advanced)]
        public string Protocol { get; set; }

        /// <summary>Number or name of the port to access on the pods targeted by the service. Number must be in the range 1 to 65535. Name must be an IANA_SVC_NAME. If this is a string, it will be looked up as a named port in the target Pod's container ports. If this is not specified, the value of the 'port' field is used (an identity map). This field is ignored for services with clusterIP=None, and should be omitted or set equal to the 'port' field. More info: https://kubernetes.io/docs/concepts/services-networking/service/#defining-a-service</summary>
        public string TargetPort { get; set; }


    }

    public partial class StatefulSet
    {
        [ReadOnly(true)]
        public string ApiVersion;

        [ReadOnly(true)]
        public string Kind;

        [Ignore]
        public StatefulSetStatus Status;
    }

    public partial class StatefulSetSpec
    {
        [SelectList(Options = new string[] { "OrderedReady", "Parallel" })]
        public string PodManagementPolicy;

        /// <summary>volumeClaimTemplates is a list of claims that pods are allowed to reference. The StatefulSet controller is responsible for mapping network identities to claims in a way that maintains the identity of a pod. Every claim in this list must have at least one matching (by name) volumeMount in one container in the template. A claim in this list takes precedence over any volumes in the template, with the same name.</summary>
        [DisplayInTree(DisplayName = "Kind")]
        public Collection<PersistentVolumeClaim> VolumeClaimTemplates;
    }

    public partial class StatefulSetUpdateStrategy
    {
        [SelectList(Options = new string[] { "RollingUpdate", "OnDelete" })]
        public string Type;
    }

    public partial class Volume
    {
        [UILevel(UILevel = UILevel.Advanced)]
        public AWSElasticBlockStoreVolumeSource AwsElasticBlockStore;

        [UILevel(UILevel = UILevel.Advanced)]
        public AzureDiskVolumeSource AzureDisk;

        [UILevel(UILevel = UILevel.Advanced)]
        public AzureFileVolumeSource AzureFile;

        [UILevel(UILevel = UILevel.Advanced)]
        public CephFSVolumeSource Cephfs;

        [UILevel(UILevel = UILevel.Advanced)]
        public CinderVolumeSource Cinder;

        [UILevel(UILevel = UILevel.Advanced)]
        public ConfigMapVolumeSource ConfigMap;

        [UILevel(UILevel = UILevel.Advanced)]
        public CSIVolumeSource Csi;

        [UILevel(UILevel = UILevel.Advanced)]
        public DownwardAPIVolumeSource DownwardAPI;

        [UILevel(UILevel = UILevel.Advanced)]
        public EmptyDirVolumeSource EmptyDir;

        [UILevel(UILevel = UILevel.Advanced)]
        public FCVolumeSource Fc;

        [UILevel(UILevel = UILevel.Advanced)]
        public FlexVolumeSource FlexVolume;

        [UILevel(UILevel = UILevel.Advanced)]
        public FlockerVolumeSource Flocker;

        [UILevel(UILevel = UILevel.Advanced)]
        public GCEPersistentDiskVolumeSource GcePersistentDisk;

        [UILevel(UILevel = UILevel.Advanced)]
        public GitRepoVolumeSource GitRepo;

        [UILevel(UILevel = UILevel.Advanced)]
        public GlusterfsVolumeSource Glusterfs;

        [UILevel(UILevel = UILevel.Advanced)]
        public HostPathVolumeSource HostPath;

        [UILevel(UILevel = UILevel.Advanced)]
        public ISCSIVolumeSource Iscsi;

        [Required]
        public string Name;

        [UILevel(UILevel = UILevel.Advanced)]
        public NFSVolumeSource Nfs;

        public PersistentVolumeClaimVolumeSource PersistentVolumeClaim;

        [UILevel(UILevel = UILevel.Advanced)]
        public PhotonPersistentDiskVolumeSource PhotonPersistentDisk;

        [UILevel(UILevel = UILevel.Advanced)]
        public PortworxVolumeSource PortworxVolume;

        [UILevel(UILevel = UILevel.Advanced)]
        public ProjectedVolumeSource Projected;

        [UILevel(UILevel = UILevel.Advanced)]
        public QuobyteVolumeSource Quobyte;

        [UILevel(UILevel = UILevel.Advanced)]
        public RBDVolumeSource Rbd;

        [UILevel(UILevel = UILevel.Advanced)]
        public ScaleIOVolumeSource ScaleIO;

        [UILevel(UILevel = UILevel.Advanced)]
        public SecretVolumeSource Secret;

        [UILevel(UILevel = UILevel.Advanced)]
        public StorageOSVolumeSource Storageos;

        [UILevel(UILevel = UILevel.Advanced)]
        public VsphereVirtualDiskVolumeSource VsphereVolume;
    }

    public partial class CronJob
    {
        [ReadOnly(true)]
        public string ApiVersion { get; set; }

        [ReadOnly(true)]
        public string Kind { get; set; }

        [Ignore]
        public CronJobStatus Status { get; set; }
    }

    public partial class CronJobSpec
    {
        [SelectList(Options = new string[] { "Allow", "Forbid", "Replace" })]
        public string ConcurrencyPolicy { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public int? FailedJobsHistoryLimit { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public long? StartingDeadlineSeconds { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public int? SuccessfulJobsHistoryLimit { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? Suspend { get; set; }
    }

    public partial class JobSpec
    {
        public long? ActiveDeadlineSeconds { get; set; }

        public int? BackoffLimit { get; set; }

        public int? Completions { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? ManualSelector { get; set; }

        public int? Parallelism { get; set; }

        [UILevel(UILevel = UILevel.Expert)]
        public int? TtlSecondsAfterFinished { get; set; }
    }

    public partial class DaemonSetUpdateStrategy
    {
        /// <summary>Type of daemon set update. Can be "RollingUpdate" or "OnDelete". Default is RollingUpdate.</summary>
        [SelectList(Options = new string[] { "RollingUpdate", "OnDelete" })]
        public string Type { get; set; }
    }

    public partial class DaemonSet
    {
        [ReadOnly(true)]
        public string ApiVersion { get; set; }

        [ReadOnly(true)]
        public string Kind { get; set; }

        [Ignore]
        public DaemonSetStatus Status { get; set; }
    }

    public partial class VolumeMount
    {
        /// <summary>Path within the container at which the volume should be mounted.  Must not contain ':'.</summary>
        public string MountPath { get; set; }

        /// <summary>mountPropagation determines how mounts are propagated from the host to container and the other way around. When not set, MountPropagationNone is used. This field is beta in 1.10.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string MountPropagation { get; set; }

        /// <summary>This must match the Name of a Volume.</summary>
        public string Name { get; set; }

        /// <summary>Mounted read-only if true, read-write otherwise (false or unspecified). Defaults to false.</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public bool? ReadOnly { get; set; }

        /// <summary>Path within the volume from which the container's volume should be mounted. Defaults to "" (volume's root).</summary>
        [UILevel(UILevel = UILevel.Advanced)]
        public string SubPath { get; set; }

        /// <summary>Expanded path within the volume from which the container's volume should be mounted. Behaves similarly to SubPath but environment variable references $(VAR_NAME) are expanded using the container's environment. Defaults to "" (volume's root). SubPathExpr and SubPath are mutually exclusive. This field is alpha in 1.14.</summary>
        [UILevel(UILevel = UILevel.Expert)]
        public string SubPathExpr { get; set; }
    }

    public partial class Toleration
    {
        /// <summary>Effect indicates the taint effect to match. Empty means match all taint effects. When specified, allowed values are NoSchedule, PreferNoSchedule and NoExecute.</summary>
        [SelectList(Options = new string[] { "NoSchedule", "PreferNoSchedule", "NoExecute" })]
        public string Effect { get; set; }

        /// <summary>Key is the taint key that the toleration applies to. Empty means match all taint keys. If the key is empty, operator must be Exists; this combination means to match all values and all keys.</summary>
        public string Key { get; set; }

        /// <summary>Operator represents a key's relationship to the value. Valid operators are Exists and Equal. Defaults to Equal. Exists is equivalent to wildcard for value, so that a pod can tolerate all taints of a particular category.</summary>
        [SelectList(Options = new string[] { "Equal", "Exists" })]
        public string Operator { get; set; }

        /// <summary>TolerationSeconds represents the period of time the toleration (which must be of effect NoExecute, otherwise this field is ignored) tolerates the taint. By default, it is not set, which means tolerate the taint forever (do not evict). Zero and negative values will be treated as 0 (evict immediately) by the system.</summary>
        public long? TolerationSeconds { get; set; }

        /// <summary>Value is the taint value the toleration matches to. If the operator is Exists, the value should be empty, otherwise just a regular string.</summary>
        public string Value { get; set; }


    }
}
#pragma warning restore 1591