using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using k8s.Models;

namespace KubeUI.Avalonia.Services.Icons;

public sealed partial class ResourceIconService : IResourceIconService
{
    private const string BlankIconPath = "/Assets/kube/blank.svg";
    private static readonly Uri AppUri = new("avares://KubeUI.Avalonia");
    private static readonly Dictionary<Type, string> IconPaths = new()
    {
        [typeof(V1Node)] = "/Assets/kube/infrastructure_components/unlabeled/node.svg",
        [typeof(Corev1Event)] = "/Assets/kube/infrastructure_components/unlabeled/etcd.svg",
        [typeof(V1ConfigMap)] = "/Assets/kube/resources/unlabeled/cm.svg",
        [typeof(V1ClusterRoleBinding)] = "/Assets/kube/resources/unlabeled/crb.svg",
        [typeof(V1CustomResourceDefinition)] = "/Assets/kube/resources/unlabeled/crd.svg",
        [typeof(V1ClusterRole)] = "/Assets/kube/resources/unlabeled/c-role.svg",
        [typeof(V1CronJob)] = "/Assets/kube/resources/unlabeled/cronjob.svg",
        [typeof(V1Deployment)] = "/Assets/kube/resources/unlabeled/deploy.svg",
        [typeof(V1DaemonSet)] = "/Assets/kube/resources/unlabeled/ds.svg",
        [typeof(V1EndpointSlice)] = "/Assets/kube/resources/unlabeled/ep.svg",
        [typeof(V1APIGroup)] = "/Assets/kube/resources/unlabeled/group.svg",
        [typeof(V1HorizontalPodAutoscaler)] = "/Assets/kube/resources/unlabeled/hpa.svg",
        [typeof(V2HorizontalPodAutoscaler)] = "/Assets/kube/resources/unlabeled/hpa.svg",
        [typeof(V1Ingress)] = "/Assets/kube/resources/unlabeled/ing.svg",
        [typeof(V1Job)] = "/Assets/kube/resources/unlabeled/job.svg",
        [typeof(V1LimitRange)] = "/Assets/kube/resources/unlabeled/limits.svg",
        [typeof(V1NetworkPolicy)] = "/Assets/kube/resources/unlabeled/netpol.svg",
        [typeof(V1Namespace)] = "/Assets/kube/resources/unlabeled/ns.svg",
        [typeof(V1Pod)] = "/Assets/kube/resources/unlabeled/pod.svg",
        [typeof(V1PersistentVolume)] = "/Assets/kube/resources/unlabeled/pv.svg",
        [typeof(V1PersistentVolumeClaim)] = "/Assets/kube/resources/unlabeled/pvc.svg",
        [typeof(V1ResourceQuota)] = "/Assets/kube/resources/unlabeled/quota.svg",
        [typeof(V1RoleBinding)] = "/Assets/kube/resources/unlabeled/rb.svg",
        [typeof(V1Role)] = "/Assets/kube/resources/unlabeled/role.svg",
        [typeof(V1ReplicaSet)] = "/Assets/kube/resources/unlabeled/rs.svg",
        [typeof(V1ServiceAccount)] = "/Assets/kube/resources/unlabeled/sa.svg",
        [typeof(V1StorageClass)] = "/Assets/kube/resources/unlabeled/sc.svg",
        [typeof(V1Secret)] = "/Assets/kube/resources/unlabeled/secret.svg",
        [typeof(V1StatefulSet)] = "/Assets/kube/resources/unlabeled/sts.svg",
        [typeof(V1Service)] = "/Assets/kube/resources/unlabeled/svc.svg",
        [typeof(V1UserSubject)] = "/Assets/kube/resources/unlabeled/user.svg",
    };

    private readonly ConcurrentDictionary<Type, SvgSource> _sources = new();

    public IImage GetIcon(Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        return new SvgImage { Source = _sources.GetOrAdd(resourceType, CreateSource) };
    }

    private static SvgSource CreateSource(Type resourceType)
    {
        if (IconPaths.TryGetValue(resourceType, out var path))
        {
            return new SvgSource(AppUri) { Path = path };
        }

        using var stream = AssetLoader.Open(new Uri(BlankIconPath, UriKind.Relative), AppUri)
            ?? throw new InvalidOperationException($"Unable to load resource icon '{BlankIconPath}'.");
        using StreamReader reader = new(stream);
        var blankSvg = reader.ReadToEnd();
        var initials = GetInitials(GetResourceKind(resourceType));
        var fontSize = initials.Length switch
        {
            1 => "14",
            2 => "10",
            _ => "8",
        };
        var generatedSvg = blankSvg.Replace("</svg>", $"<text x=\"9\" y=\"9.5\" text-anchor=\"middle\" dominant-baseline=\"middle\" font-family=\"Cascadia Mono\" font-size=\"{fontSize}\" font-weight=\"bold\" fill=\"#ffffff\">{initials}</text></svg>", StringComparison.OrdinalIgnoreCase);
        return SvgSource.LoadFromSvg(generatedSvg) ?? throw new InvalidOperationException("Unable to generate a resource icon.");
    }

    private static string GetInitials(string typeName)
    {
        var words = PascalCaseWordRegex().Matches(typeName);
        if (words.Count > 1)
        {
            return string.Concat(words.Cast<Match>().Take(3).Select(match => match.Value[0])).ToUpperInvariant();
        }

        return typeName.Length == 0 ? string.Empty : typeName[..1].ToUpperInvariant();
    }

    private static string GetResourceKind(Type resourceType)
    {
        var kindField = resourceType.GetField("KubeKind", BindingFlags.Public | BindingFlags.Static);
        return kindField?.GetValue(null) as string ?? resourceType.Name;
    }

    [GeneratedRegex("[A-Z]+(?=[A-Z][a-z]|$)|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex PascalCaseWordRegex();
}
