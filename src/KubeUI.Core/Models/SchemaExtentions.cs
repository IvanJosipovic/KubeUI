using KubeUI.Core;
using KubeUI.Schema;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
            [UILevel(UILevel = UILevel.Advanced)]
        public string HostIP { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public int? HostPort { get; set; }

        public string Name { get; set; }

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
        public Initializers Initializers { get; set; }

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
        [SelectList(Options = new string[] { "ReadWriteOnce", "ReadOnlyMany", "ReadWriteMany" })]
        public Collection<string> AccessModes;

        [UILevel(UILevel = UILevel.Advanced)]
        public TypedLocalObjectReference DataSource;

        [UILevel(UILevel = UILevel.Advanced)]
        public string VolumeMode;
    }

    public partial class PodAffinity
    {
        [DisplayInTree(DisplayName = "Weight")]
        public System.Collections.ObjectModel.Collection<WeightedPodAffinityTerm> PreferredDuringSchedulingIgnoredDuringExecution { get; set; }

        [DisplayInTree(DisplayName = "TopologyKey")]
        public System.Collections.ObjectModel.Collection<PodAffinityTerm> RequiredDuringSchedulingIgnoredDuringExecution { get; set; }
    }

    public partial class PodSpec
    {
        [UILevel(UILevel = UILevel.Advanced)]
        public Affinity Affinity { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? AutomountServiceAccountToken { get; set; }

        [DisplayInTree(DisplayName = "Image")]
        public System.Collections.ObjectModel.Collection<Container> Containers { get; set; } = new System.Collections.ObjectModel.Collection<Container>();

        [UILevel(UILevel = UILevel.Advanced)]
        public PodDNSConfig DnsConfig { get; set; }

        [SelectList(Options = new string[] { "ClusterFirst", "ClusterFirstWithHostNet", "Default", "None" })]
        [UILevel(UILevel = UILevel.Advanced)]
        public string DnsPolicy { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? EnableServiceLinks { get; set; }

        [DisplayInTree(DisplayName = "Ip")]
        [UILevel(UILevel = UILevel.Advanced)]
        public System.Collections.ObjectModel.Collection<HostAlias> HostAliases { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? HostIPC { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? HostNetwork { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? HostPID { get; set; }

        [Ignore]
        public System.Collections.ObjectModel.Collection<Container> InitContainers { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public int? Priority { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public string PriorityClassName { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public System.Collections.ObjectModel.Collection<PodReadinessGate> ReadinessGates { get; set; }

        [SelectList(Options = new string[] { "Always", "Always", "Never" })]
        public string RestartPolicy { get; set; }

        [UILevel(UILevel = UILevel.Expert)]
        public string RuntimeClassName { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public string SchedulerName { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public PodSecurityContext SecurityContext { get; set; }

        [Ignore]
        public string ServiceAccount { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public string ServiceAccountName { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? ShareProcessNamespace { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public string Subdomain { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public System.Collections.ObjectModel.Collection<Toleration> Tolerations { get; set; }

        [DisplayInTree(DisplayName = "Name")]
        public System.Collections.ObjectModel.Collection<Volume> Volumes { get; set; }
    }

    public partial class NodeAffinity
    {
        [DisplayInTree(DisplayName = "Weight")]
        public System.Collections.ObjectModel.Collection<PreferredSchedulingTerm> PreferredDuringSchedulingIgnoredDuringExecution { get; set; }
    }

    public partial class NodeSelector
    {
        [DisplayInTree()]
        public System.Collections.ObjectModel.Collection<NodeSelectorTerm> NodeSelectorTerms { get; set; } = new System.Collections.ObjectModel.Collection<NodeSelectorTerm>();
    }

    public partial class NodeSelectorRequirement
    {
        [SelectList(Options = new string[] { "In", "NotIn", "Exists", "DoesNotExist", "Gt", "Lt" })]
        public string Operator { get; set; }
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
        [UILevel(UILevel = UILevel.Advanced)]
        public string ClusterIP;

        [UILevel(UILevel = UILevel.Advanced)]
        public Collection<string> ExternalIPs;

        [UILevel(UILevel = UILevel.Advanced)]
        public string ExternalName;

        [UILevel(UILevel = UILevel.Advanced)]
        public string ExternalTrafficPolicy;

        [UILevel(UILevel = UILevel.Advanced)]
        public int? HealthCheckNodePort;

        [UILevel(UILevel = UILevel.Advanced)]
        public string LoadBalancerIP;

        [UILevel(UILevel = UILevel.Advanced)]
        public Collection<string> LoadBalancerSourceRanges;

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? PublishNotReadyAddresses;

        [UILevel(UILevel = UILevel.Advanced)]
        [SelectList(Options = new string[] { "None", "ClusterIP" })]
        public string SessionAffinity;

        [SelectList(Options = new string[] { "ClusterIP", "NodePort", "LoadBalancer", "ExternalName" })]
        public string Type;
    }

    public partial class ServicePort
    {
        [UILevel(UILevel = UILevel.Advanced)]
        public int? NodePort { get; set; }

        [SelectList(Options = new string[] { "TCP", "UDP", "SCTP" })]
        [UILevel(UILevel = UILevel.Advanced)]
        public string Protocol { get; set; }
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
        [UILevel(UILevel = UILevel.Advanced)]
        public bool? ManualSelector { get; set; }

        [UILevel(UILevel = UILevel.Expert)]
        public int? TtlSecondsAfterFinished { get; set; }
    }

    public partial class DaemonSetUpdateStrategy
    {
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
        public string MountPath { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public string MountPropagation { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public bool? ReadOnly { get; set; }

        [UILevel(UILevel = UILevel.Advanced)]
        public string SubPath { get; set; }

        [UILevel(UILevel = UILevel.Expert)]
        public string SubPathExpr { get; set; }
    }

    public partial class Toleration
    {
        [SelectList(Options = new string[] { "NoSchedule", "PreferNoSchedule", "NoExecute" })]
        public string Effect { get; set; }

        [SelectList(Options = new string[] { "Equal", "Exists" })]
        public string Operator { get; set; }
    }
}
#pragma warning restore 1591