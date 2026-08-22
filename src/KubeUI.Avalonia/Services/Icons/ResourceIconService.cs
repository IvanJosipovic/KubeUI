using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using k8s.Models;
using KubernetesClient.Informer.Client;

using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Services.Icons;

public sealed partial class ResourceIconService : IResourceIconService
{
    private const string BlankIconPath = "/Assets/kube/blank.svg";
    private static readonly Uri AppUri = new("avares://KubeUI.Avalonia");
    private static readonly Dictionary<GroupApiVersionKind, string> IconPaths = new()
    {
        [GroupApiVersionKind.From<V1Node>()] = "/Assets/kube/infrastructure_components/unlabeled/node.svg",
        [GroupApiVersionKind.From<Corev1Event>()] = "/Assets/kube/infrastructure_components/unlabeled/etcd.svg",
        [GroupApiVersionKind.From<V1ConfigMap>()] = "/Assets/kube/resources/unlabeled/cm.svg",
        [GroupApiVersionKind.From<V1ClusterRoleBinding>()] = "/Assets/kube/resources/unlabeled/crb.svg",
        [GroupApiVersionKind.From<V1CustomResourceDefinition>()] = "/Assets/kube/resources/unlabeled/crd.svg",
        [GroupApiVersionKind.From<V1ClusterRole>()] = "/Assets/kube/resources/unlabeled/c-role.svg",
        [GroupApiVersionKind.From<V1CronJob>()] = "/Assets/kube/resources/unlabeled/cronjob.svg",
        [GroupApiVersionKind.From<V1Deployment>()] = "/Assets/kube/resources/unlabeled/deploy.svg",
        [GroupApiVersionKind.From<V1DaemonSet>()] = "/Assets/kube/resources/unlabeled/ds.svg",
        [GroupApiVersionKind.From<V1EndpointSlice>()] = "/Assets/kube/resources/unlabeled/ep.svg",
        [new GroupApiVersionKind(string.Empty, "v1", "APIGroup", string.Empty)] = "/Assets/kube/resources/unlabeled/group.svg",
        [GroupApiVersionKind.From<V1HorizontalPodAutoscaler>()] = "/Assets/kube/resources/unlabeled/hpa.svg",
        [GroupApiVersionKind.From<V2HorizontalPodAutoscaler>()] = "/Assets/kube/resources/unlabeled/hpa.svg",
        [GroupApiVersionKind.From<V1Ingress>()] = "/Assets/kube/resources/unlabeled/ing.svg",
        [GroupApiVersionKind.From<V1Job>()] = "/Assets/kube/resources/unlabeled/job.svg",
        [GroupApiVersionKind.From<V1LimitRange>()] = "/Assets/kube/resources/unlabeled/limits.svg",
        [GroupApiVersionKind.From<V1NetworkPolicy>()] = "/Assets/kube/resources/unlabeled/netpol.svg",
        [GroupApiVersionKind.From<V1Namespace>()] = "/Assets/kube/resources/unlabeled/ns.svg",
        [GroupApiVersionKind.From<V1Pod>()] = "/Assets/kube/resources/unlabeled/pod.svg",
        [GroupApiVersionKind.From<V1PersistentVolume>()] = "/Assets/kube/resources/unlabeled/pv.svg",
        [GroupApiVersionKind.From<V1PersistentVolumeClaim>()] = "/Assets/kube/resources/unlabeled/pvc.svg",
        [GroupApiVersionKind.From<V1ResourceQuota>()] = "/Assets/kube/resources/unlabeled/quota.svg",
        [GroupApiVersionKind.From<V1RoleBinding>()] = "/Assets/kube/resources/unlabeled/rb.svg",
        [GroupApiVersionKind.From<V1Role>()] = "/Assets/kube/resources/unlabeled/role.svg",
        [GroupApiVersionKind.From<V1ReplicaSet>()] = "/Assets/kube/resources/unlabeled/rs.svg",
        [GroupApiVersionKind.From<V1ServiceAccount>()] = "/Assets/kube/resources/unlabeled/sa.svg",
        [GroupApiVersionKind.From<V1StorageClass>()] = "/Assets/kube/resources/unlabeled/sc.svg",
        [GroupApiVersionKind.From<V1Secret>()] = "/Assets/kube/resources/unlabeled/secret.svg",
        [GroupApiVersionKind.From<V1StatefulSet>()] = "/Assets/kube/resources/unlabeled/sts.svg",
        [GroupApiVersionKind.From<V1Service>()] = "/Assets/kube/resources/unlabeled/svc.svg",
        [new GroupApiVersionKind(string.Empty, "v1", "UserSubject", string.Empty)] = "/Assets/kube/resources/unlabeled/user.svg",
    };

    private readonly ConcurrentDictionary<GroupApiVersionKind, SvgSource> _sources = new();

    public IImage GetIcon(GroupApiVersionKind resourceKind)
    {
        return new SvgImage { Source = _sources.GetOrAdd(resourceKind, CreateSource) };
    }

    private static SvgSource CreateSource(GroupApiVersionKind resourceKind)
    {
        var path = IconPaths.TryGetValue(resourceKind, out var exactPath)
            ? exactPath
            : IconPaths.FirstOrDefault(pair =>
                pair.Key.Group == resourceKind.Group
                && pair.Key.ApiVersion == resourceKind.ApiVersion
                && pair.Key.Kind == resourceKind.Kind).Value;

        if (path is not null)
        {
            return new SvgSource(AppUri) { Path = path };
        }

        using var stream = AssetLoader.Open(new Uri(BlankIconPath, UriKind.Relative), AppUri)
            ?? throw new InvalidOperationException($"Unable to load resource icon '{BlankIconPath}'.");
        using StreamReader reader = new(stream);
        var blankSvg = reader.ReadToEnd();
        var initials = GetInitials(resourceKind.Kind);
        var fontSize = initials.Length switch
        {
            1 => Typography.IconFontSizeOneCharacter,
            2 => Typography.IconFontSizeTwoCharacters,
            _ => Typography.IconFontSizeThreeOrMoreCharacters,
        };
        var generatedSvg = blankSvg.Replace("</svg>", $"<text x=\"9\" y=\"9.5\" text-anchor=\"middle\" dominant-baseline=\"middle\" font-family=\"{Typography.CodeFontFamilyName}\" font-size=\"{fontSize}\" font-weight=\"bold\" fill=\"#ffffff\">{initials}</text></svg>", StringComparison.OrdinalIgnoreCase);
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

    [GeneratedRegex("[A-Z]+(?=[A-Z][a-z]|$)|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex PascalCaseWordRegex();
}
