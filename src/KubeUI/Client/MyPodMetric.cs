using k8s;
using k8s.Models;

namespace KubeUI.Client
{
    [KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePluralName)]
    public partial class MyPodMetrics : PodMetrics, IKubernetesObject<V1ObjectMeta>
    {
        public const string KubeApiVersion = "v1beta1";
        public const string KubeKind = "PodMetrics";
        public const string KubeGroup = "metrics.k8s.io";
        public const string KubePluralName = "pods";

        public string ApiVersion { get; set; }
        public string Kind { get; set; }
    }
}
