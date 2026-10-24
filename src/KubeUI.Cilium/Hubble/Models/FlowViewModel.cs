using CommunityToolkit.Mvvm.ComponentModel;
using Pb = KubeUI.Cilium.Hubble.Models;

namespace KubeUI.Cilium.Hubble;

public enum FlowVerdict
{
    Unknown = 0,
    Forwarded = 1,
    Dropped = 2,
    Error = 3,
    Audit = 4,
    Redirected = 5,
    Traced = 6,
    Translated = 7
}

public enum FlowDirection
{
    Unknown = 0,
    Ingress = 1,
    Egress = 2
}

public sealed partial class FlowViewModel : ObservableObject
{
    private readonly string _fallbackCacheIdentity = Guid.NewGuid().ToString("N");

    public FlowViewModel() { }

    public FlowViewModel(Pb.Flow flow)
    {
        UpdateFrom(flow);
    }

    [ObservableProperty] public partial DateTime Timestamp { get; set; }
    [ObservableProperty] public partial string Uuid { get; set; } = string.Empty;
    [ObservableProperty] public partial FlowVerdict Verdict { get; set; }
    [ObservableProperty] public partial FlowDirection Direction { get; set; }
    [ObservableProperty] public partial Pb.FlowType FlowType { get; set; }
    [ObservableProperty] public partial string NodeName { get; set; } = string.Empty;

    // Source
    [ObservableProperty] public partial string SourceNamespace { get; set; } = string.Empty;
    [ObservableProperty] public partial string SourcePod { get; set; } = string.Empty;
    [ObservableProperty] public partial uint SourceId { get; set; }
    [ObservableProperty] public partial uint SourceIdentity { get; set; }
    [ObservableProperty] public partial string? SourceWorkload { get; set; }
    [ObservableProperty] public partial string? SourceWorkloadKind { get; set; }
    [ObservableProperty] public partial string SourceIp { get; set; } = string.Empty;

    // Destination
    [ObservableProperty] public partial string DestinationNamespace { get; set; } = string.Empty;
    [ObservableProperty] public partial string DestinationPod { get; set; } = string.Empty;
    [ObservableProperty] public partial uint DestinationId { get; set; }
    [ObservableProperty] public partial uint DestinationIdentity { get; set; }
    [ObservableProperty] public partial string? DestinationWorkload { get; set; }
    [ObservableProperty] public partial string? DestinationWorkloadKind { get; set; }
    [ObservableProperty] public partial string DestinationIp { get; set; } = string.Empty;

    // Network
    [ObservableProperty] public partial string Protocol { get; set; } = string.Empty;
    [ObservableProperty] public partial int DestinationPort { get; set; }
    [ObservableProperty] public partial int SourcePort { get; set; }
    [ObservableProperty] public partial TimeSpan? Latency { get; set; }
    [ObservableProperty] public partial bool IsReply { get; set; }

    // Policy
    [ObservableProperty] public partial string? MatchedPolicyName { get; set; }
    [ObservableProperty] public partial string? PolicyNamespace { get; set; }
    [ObservableProperty] public partial Pb.DropReason? DropReason { get; set; }
    [ObservableProperty] public partial string? DropReasonDescription { get; set; }
    [ObservableProperty] public partial string? ExtDropReasonDescription { get; set; }

    // DNS / L7
    [ObservableProperty] public partial string? DnsQuery { get; set; }
    [ObservableProperty] public partial string? HttpMethod { get; set; }
    [ObservableProperty] public partial string? HttpPath { get; set; }
    [ObservableProperty] public partial int? HttpStatusCode { get; set; }
    [ObservableProperty] public partial Pb.L7FlowType L7Type { get; set; }

    // Computed
    public string LatencyHumanReadable => Latency is { } latency
        ? $"{latency.TotalMilliseconds:F1} ms"
        : "\u2014";

    public FlowEndpoint Source => new()
    {
        Namespace = SourceNamespace,
        Pod = SourcePod,
        Workload = SourceWorkload,
        Identity = SourceIdentity,
        Ip = SourceIp
    };

    public FlowEndpoint Destination => new()
    {
        Namespace = DestinationNamespace,
        Pod = DestinationPod,
        Workload = DestinationWorkload,
        Identity = DestinationIdentity,
        Ip = DestinationIp
    };

    public FlowCacheKey CacheKey => new(string.IsNullOrWhiteSpace(Uuid) ? _fallbackCacheIdentity : Uuid);

    public void UpdateFrom(Pb.Flow flow)
    {
        if (flow.Time != null)
            Timestamp = flow.Time.ToDateTime();
        Uuid = flow.Uuid ?? string.Empty;
        Verdict = (FlowVerdict)flow.Verdict;
        Direction = flow.TrafficDirection switch
        {
            Pb.TrafficDirection.Ingress => FlowDirection.Ingress,
            Pb.TrafficDirection.Egress => FlowDirection.Egress,
            _ => FlowDirection.Unknown
        };
        FlowType = flow.Type;
        NodeName = flow.NodeName ?? string.Empty;

        if (flow.Source != null)
        {
            SourceNamespace = flow.Source.Namespace ?? string.Empty;
            SourcePod = flow.Source.PodName ?? string.Empty;
            SourceId = flow.Source.ID;
            SourceIdentity = flow.Source.Identity;
            SourceWorkload = flow.Source.Workloads.Count > 0 ? flow.Source.Workloads[0].Name : null;
            SourceWorkloadKind = flow.Source.Workloads.Count > 0 ? flow.Source.Workloads[0].Kind : null;
        }

        if (flow.Destination != null)
        {
            DestinationNamespace = flow.Destination.Namespace ?? string.Empty;
            DestinationPod = flow.Destination.PodName ?? string.Empty;
            DestinationId = flow.Destination.ID;
            DestinationIdentity = flow.Destination.Identity;
            DestinationWorkload = flow.Destination.Workloads.Count > 0 ? flow.Destination.Workloads[0].Name : null;
            DestinationWorkloadKind = flow.Destination.Workloads.Count > 0 ? flow.Destination.Workloads[0].Kind : null;
        }

        if (flow.IP != null)
        {
            SourceIp = flow.IP.Source ?? string.Empty;
            DestinationIp = flow.IP.Destination ?? string.Empty;
        }

        if (flow.L4 != null)
        {
            if (flow.L4.TCP != null)
            {
                Protocol = "TCP";
                SourcePort = (int)flow.L4.TCP.SourcePort;
                DestinationPort = (int)flow.L4.TCP.DestinationPort;
            }
            else if (flow.L4.UDP != null)
            {
                Protocol = "UDP";
                SourcePort = (int)flow.L4.UDP.SourcePort;
                DestinationPort = (int)flow.L4.UDP.DestinationPort;
            }
            else if (flow.L4.ICMPv4 != null)
            {
                Protocol = "ICMPv4";
            }
            else if (flow.L4.ICMPv6 != null)
            {
                Protocol = "ICMPv6";
            }
            else if (flow.L4.SCTP != null)
            {
                Protocol = "SCTP";
                SourcePort = (int)flow.L4.SCTP.SourcePort;
                DestinationPort = (int)flow.L4.SCTP.DestinationPort;
            }
        }

        Latency = flow.L7 is { LatencyNs: > 0 } l7
            ? TimeSpan.FromTicks((long)Math.Min(l7.LatencyNs / 100, (ulong)TimeSpan.MaxValue.Ticks))
            : null;

        IsReply = flow.IsReply ?? false;

        if (flow.EgressDeniedBy.Count > 0)
        {
            MatchedPolicyName = flow.EgressDeniedBy[0].Name;
            PolicyNamespace = flow.EgressDeniedBy[0].Namespace;
        }
        else if (flow.IngressDeniedBy.Count > 0)
        {
            MatchedPolicyName = flow.IngressDeniedBy[0].Name;
            PolicyNamespace = flow.IngressDeniedBy[0].Namespace;
        }
        else if (flow.EgressAllowedBy.Count > 0)
        {
            MatchedPolicyName = flow.EgressAllowedBy[0].Name;
            PolicyNamespace = flow.EgressAllowedBy[0].Namespace;
        }
        else if (flow.IngressAllowedBy.Count > 0)
        {
            MatchedPolicyName = flow.IngressAllowedBy[0].Name;
            PolicyNamespace = flow.IngressAllowedBy[0].Namespace;
        }

        DropReason = flow.DropReasonDesc != Pb.DropReason.Unknown ? flow.DropReasonDesc : null;
        DropReasonDescription = flow.DropReasonDesc.ToString();
        ExtDropReasonDescription = flow.ExtDropReasonDesc;

        if (flow.L7 != null)
        {
            L7Type = flow.L7.Type;
            if (flow.L7.Dns != null)
            {
                DnsQuery = flow.L7.Dns.Query;
            }
            if (flow.L7.Http != null)
            {
                HttpMethod = flow.L7.Http.Method;
                HttpPath = flow.L7.Http.Url;
                HttpStatusCode = flow.L7.Http.Code > 0 ? (int?)flow.L7.Http.Code : null;
            }
        }
    }

}

public readonly record struct FlowCacheKey(string EventId)
{
    public static FlowCacheKey From(FlowViewModel flow)
        => flow.CacheKey;
}

public sealed class FlowEndpoint
{
    public string Namespace { get; init; } = string.Empty;
    public string Pod { get; init; } = string.Empty;
    public string? Workload { get; init; }
    public uint? Identity { get; init; }
    public string Ip { get; init; } = string.Empty;
}

public sealed class ServiceMapGraph
{
    public List<ServiceMapNode> Nodes { get; } = [];
    public List<ServiceMapEdge> Edges { get; } = [];
    public int NodeCount => Nodes.Count;
    public int EdgeCount => Edges.Count;
}

public sealed class ServiceMapNode
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string? WorkloadType { get; init; }
    public int IngressFlowCount { get; set; }
    public int EgressFlowCount { get; set; }
    public int DeniedCount { get; set; }
    public int AllowedCount { get; set; }
}

public sealed class ServiceMapEdge
{
    public string SourceId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public int FlowCount { get; set; }
    public int DeniedCount { get; set; }
    public FlowVerdict DominantVerdict { get; set; }
}
