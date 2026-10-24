using System.Collections.Generic;
using KubeUI.Cilium.Hubble.Models;

namespace KubeUI.Cilium.Hubble;

/// <summary>Builds a time-scoped service graph from Hubble flow events.</summary>
public sealed class ServiceMapGraphBuilder
{
    private static readonly TimeSpan s_maxRetention = TimeSpan.FromHours(1);
    private readonly object _gate = new();
    private readonly List<FlowViewModel> _recentFlows = [];
    private readonly int _maxRetainedFlows;

    /// <summary>Creates a graph builder with a bounded event history.</summary>
    /// <param name="maxRetainedFlows">Maximum number of flow events retained in memory.</param>
    public ServiceMapGraphBuilder(int maxRetainedFlows = 100_000)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRetainedFlows);
        _maxRetainedFlows = maxRetainedFlows;
    }

    /// <summary>Adds one flow event to the rolling graph window.</summary>
    public void AddFlow(FlowViewModel flow)
    {
        ArgumentNullException.ThrowIfNull(flow);

        lock (_gate)
        {
            Prune(_recentFlows, DateTime.UtcNow - s_maxRetention);
            _recentFlows.Add(flow);
            var excessCount = _recentFlows.Count - _maxRetainedFlows;
            if (excessCount > 0)
            {
                _recentFlows.RemoveRange(0, excessCount);
            }
        }
    }

    /// <summary>Creates a graph for the requested time and namespace scope.</summary>
    public ServiceMapGraph Build(
        TimeSpan timeWindow,
        bool aggregateByNamespace = false,
        bool showOnlyErrors = false,
        IReadOnlyCollection<string>? namespaces = null)
    {
        var flows = GetFlows(timeWindow, showOnlyErrors, namespaces);
        var graph = new ServiceMapGraph();

        if (aggregateByNamespace)
        {
            BuildAggregatedGraph(graph, flows);
        }
        else
        {
            BuildPodLevelGraph(graph, flows);
        }

        return graph;
    }

    /// <summary>Gets the recent flows matching the requested map scope.</summary>
    public FlowViewModel[] GetFlows(
        TimeSpan timeWindow,
        bool showOnlyErrors = false,
        IReadOnlyCollection<string>? namespaces = null)
    {
        if (timeWindow <= TimeSpan.Zero)
        {
            return [];
        }

        var cutoff = DateTime.UtcNow - timeWindow;
        lock (_gate)
        {
            Prune(_recentFlows, DateTime.UtcNow - s_maxRetention);
            return _recentFlows.FindAll(flow =>
                    flow.Timestamp >= cutoff
                    && (!showOnlyErrors || IsDenied(flow.Verdict))
                    && (namespaces is not { Count: > 0 }
                        || namespaces.Contains(flow.SourceNamespace, StringComparer.Ordinal)
                        || namespaces.Contains(flow.DestinationNamespace, StringComparer.Ordinal)))
                .ToArray();
        }
    }

    private static void BuildPodLevelGraph(ServiceMapGraph graph, IReadOnlyList<FlowViewModel> flows)
    {
        var nodes = new Dictionary<string, ServiceMapNode>(StringComparer.Ordinal);
        var edges = new Dictionary<(string Source, string Target), ServiceMapEdge>();

        foreach (var flow in flows)
        {
            var sourceKey = GetNodeKey(flow.SourceNamespace, flow.SourcePod, flow.SourceIp, flow.SourceIdentity);
            var targetKey = GetNodeKey(flow.DestinationNamespace, flow.DestinationPod, flow.DestinationIp, flow.DestinationIdentity);
            var sourceNode = GetOrCreateNode(
                nodes,
                sourceKey,
                GetDisplayName(flow.SourcePod, flow.SourceIp, flow.SourceIdentity),
                flow.SourceNamespace,
                flow.SourceWorkloadKind ?? flow.SourceWorkload);
            var targetNode = GetOrCreateNode(
                nodes,
                targetKey,
                GetDisplayName(flow.DestinationPod, flow.DestinationIp, flow.DestinationIdentity),
                flow.DestinationNamespace,
                flow.DestinationWorkloadKind ?? flow.DestinationWorkload);

            AddNodeFlow(sourceNode, flow, isSource: true);
            AddNodeFlow(targetNode, flow, isSource: false);
            AddEdge(edges, sourceKey, targetKey, flow.Verdict);
        }

        AddSortedNodesAndEdges(graph, nodes, edges);
    }

    private static void BuildAggregatedGraph(ServiceMapGraph graph, IReadOnlyList<FlowViewModel> flows)
    {
        var nodes = new Dictionary<string, ServiceMapNode>(StringComparer.Ordinal);
        var edges = new Dictionary<(string Source, string Target), ServiceMapEdge>();

        foreach (var flow in flows)
        {
            var sourceKey = GetNamespaceKey(flow.SourceNamespace);
            var targetKey = GetNamespaceKey(flow.DestinationNamespace);
            var sourceNode = GetOrCreateNamespaceNode(nodes, sourceKey, flow.SourceNamespace);
            var targetNode = GetOrCreateNamespaceNode(nodes, targetKey, flow.DestinationNamespace);

            AddNodeFlow(sourceNode, flow, isSource: true);
            AddNodeFlow(targetNode, flow, isSource: false);
            AddEdge(edges, sourceKey, targetKey, flow.Verdict);
        }

        AddSortedNodesAndEdges(graph, nodes, edges);
    }

    private static ServiceMapNode GetOrCreateNode(
        Dictionary<string, ServiceMapNode> nodes,
        string key,
        string name,
        string ns,
        string? workload)
    {
        if (!nodes.TryGetValue(key, out var node))
        {
            node = new ServiceMapNode
            {
                Id = key,
                Name = name,
                Namespace = ns,
                WorkloadType = workload
            };
            nodes.Add(key, node);
        }

        return node;
    }

    private static ServiceMapNode GetOrCreateNamespaceNode(
        Dictionary<string, ServiceMapNode> nodes,
        string key,
        string ns)
    {
        if (!nodes.TryGetValue(key, out var node))
        {
            var isExternal = string.IsNullOrEmpty(ns);
            node = new ServiceMapNode
            {
                Id = key,
                Name = isExternal ? "World" : ns,
                Namespace = ns,
                WorkloadType = "namespace"
            };
            nodes.Add(key, node);
        }

        return node;
    }

    private static void AddNodeFlow(ServiceMapNode node, FlowViewModel flow, bool isSource)
    {
        if (isSource)
        {
            node.EgressFlowCount++;
        }
        else
        {
            node.IngressFlowCount++;
        }

        if (IsDenied(flow.Verdict))
        {
            node.DeniedCount++;
        }
        else if (flow.Verdict is FlowVerdict.Forwarded or FlowVerdict.Redirected)
        {
            node.AllowedCount++;
        }
    }

    private static void AddEdge(
        Dictionary<(string Source, string Target), ServiceMapEdge> edges,
        string sourceKey,
        string targetKey,
        FlowVerdict verdict)
    {
        var edgeKey = (Source: sourceKey, Target: targetKey);
        if (!edges.TryGetValue(edgeKey, out var edge))
        {
            edge = new ServiceMapEdge { SourceId = sourceKey, TargetId = targetKey };
            edges.Add(edgeKey, edge);
        }

        edge.FlowCount++;
        if (IsDenied(verdict))
        {
            edge.DeniedCount++;
        }

        edge.DominantVerdict = edge.DeniedCount * 2 > edge.FlowCount
            ? FlowVerdict.Dropped
            : FlowVerdict.Forwarded;
    }

    private static void AddSortedNodesAndEdges(
        ServiceMapGraph graph,
        Dictionary<string, ServiceMapNode> nodes,
        Dictionary<(string Source, string Target), ServiceMapEdge> edges)
    {
        graph.Nodes.AddRange(nodes.Values
            .OrderBy(static node => node.Namespace, StringComparer.Ordinal)
            .ThenBy(static node => node.Name, StringComparer.Ordinal));
        graph.Edges.AddRange(edges.Values
            .OrderBy(static edge => edge.SourceId, StringComparer.Ordinal)
            .ThenBy(static edge => edge.TargetId, StringComparer.Ordinal));
    }

    private static void Prune(List<FlowViewModel> flows, DateTime cutoff)
        => flows.RemoveAll(flow => flow.Timestamp < cutoff);

    private static bool IsDenied(FlowVerdict verdict)
        => verdict is FlowVerdict.Dropped or FlowVerdict.Error;

    private static string GetNodeKey(string ns, string pod, string ip, uint identity)
    {
        if (!string.IsNullOrEmpty(pod))
        {
            return $"{ns}/{pod}";
        }

        if (!string.IsNullOrEmpty(ip))
        {
            return $"ip/{ip}";
        }

        return identity == 0 ? "world" : $"identity/{identity}";
    }

    private static string GetNamespaceKey(string ns)
        => string.IsNullOrEmpty(ns) ? "world" : $"namespace/{ns}";

    private static string GetDisplayName(string pod, string ip, uint identity)
    {
        if (!string.IsNullOrEmpty(pod))
        {
            return pod;
        }

        if (!string.IsNullOrEmpty(ip))
        {
            return ip;
        }

        return identity == 0 ? "World" : $"Identity {identity}";
    }
}
