using System.Collections.Generic;
using System.Linq;
using KubeUI.Cilium.Hubble.Models;

namespace KubeUI.Cilium.Hubble;

public sealed class ServiceMapGraphBuilder
{
    private readonly Dictionary<string, ServiceMapNode> _nodes = new();
    private readonly Dictionary<(string Source, string Target), ServiceMapEdge> _edges = new();
    private readonly List<FlowViewModel> _recentFlows = [];
    private readonly TimeSpan _maxAge;

    public ServiceMapGraphBuilder(TimeSpan? maxAge = null)
    {
        _maxAge = maxAge ?? TimeSpan.FromMinutes(5);
    }

    public void AddFlow(FlowViewModel flow, TimeSpan timeWindow)
    {
        var cutoff = DateTime.UtcNow - timeWindow;

        _recentFlows.RemoveAll(f => f.Timestamp < cutoff);
        _recentFlows.Add(flow);

        var sourceKey = GetNodeKey(flow.SourceNamespace, flow.SourcePod);
        var targetKey = GetNodeKey(flow.DestinationNamespace, flow.DestinationPod);

        var sourceNode = GetOrCreateNode(sourceKey, flow.SourceNamespace, flow.SourcePod, flow.SourceWorkload);
        var targetNode = GetOrCreateNode(targetKey, flow.DestinationNamespace, flow.DestinationPod, flow.DestinationWorkload);

        sourceNode.EgressBytes += flow.Bytes;
        targetNode.IngressBytes += flow.Bytes;

        if (flow.Verdict == FlowVerdict.Dropped || flow.Verdict == FlowVerdict.Error)
        {
            sourceNode.DeniedCount++;
            targetNode.DeniedCount++;
        }
        else if (flow.Verdict == FlowVerdict.Forwarded || flow.Verdict == FlowVerdict.Redirected)
        {
            sourceNode.AllowedCount++;
            targetNode.AllowedCount++;
        }

        var edgeKey = (sourceKey, targetKey);
        if (!_edges.TryGetValue(edgeKey, out var edge))
        {
            edge = new ServiceMapEdge
            {
                SourceId = sourceKey,
                TargetId = targetKey
            };
            _edges[edgeKey] = edge;
        }

        edge.Bytes += flow.Bytes;
        edge.FlowCount++;

        if (flow.Verdict == FlowVerdict.Dropped || flow.Verdict == FlowVerdict.Error)
        {
            edge.DeniedCount++;
        }

        edge.DominantVerdict = edge.DeniedCount > edge.FlowCount / 2
            ? FlowVerdict.Dropped
            : FlowVerdict.Forwarded;
    }

    public ServiceMapGraph Build(bool aggregateByNamespace = false)
    {
        var graph = new ServiceMapGraph();
        var cutoff = DateTime.UtcNow - _maxAge;

        var relevantFlows = _recentFlows.Where(f => f.Timestamp >= cutoff).ToList();

        if (aggregateByNamespace)
        {
            BuildAggregatedGraph(graph, relevantFlows);
        }
        else
        {
            BuildPodLevelGraph(graph, relevantFlows);
        }

        return graph;
    }

    private void BuildPodLevelGraph(ServiceMapGraph graph, List<FlowViewModel> flows)
    {
        var nodeMap = new Dictionary<string, ServiceMapNode>();
        var edgeMap = new Dictionary<(string, string), ServiceMapEdge>();

        foreach (var flow in flows)
        {
            var sourceKey = GetNodeKey(flow.SourceNamespace, flow.SourcePod);
            var targetKey = GetNodeKey(flow.DestinationNamespace, flow.DestinationPod);

            if (!nodeMap.TryGetValue(sourceKey, out var sourceNode))
            {
                sourceNode = new ServiceMapNode
                {
                    Id = sourceKey,
                    Name = flow.SourcePod,
                    Namespace = flow.SourceNamespace,
                    WorkloadType = flow.SourceWorkload
                };
                nodeMap[sourceKey] = sourceNode;
            }

            if (!nodeMap.TryGetValue(targetKey, out var targetNode))
            {
                targetNode = new ServiceMapNode
                {
                    Id = targetKey,
                    Name = flow.DestinationPod,
                    Namespace = flow.DestinationNamespace,
                    WorkloadType = flow.DestinationWorkload
                };
                nodeMap[targetKey] = targetNode;
            }

            sourceNode.EgressBytes += flow.Bytes;
            targetNode.IngressBytes += flow.Bytes;

            if (flow.Verdict == FlowVerdict.Dropped || flow.Verdict == FlowVerdict.Error)
            {
                sourceNode.DeniedCount++;
                targetNode.DeniedCount++;
            }
            else if (flow.Verdict == FlowVerdict.Forwarded || flow.Verdict == FlowVerdict.Redirected)
            {
                sourceNode.AllowedCount++;
                targetNode.AllowedCount++;
            }

            var edgeKey = (sourceKey, targetKey);
            if (!edgeMap.TryGetValue(edgeKey, out var edge))
            {
                edge = new ServiceMapEdge
                {
                    SourceId = sourceKey,
                    TargetId = targetKey
                };
                edgeMap[edgeKey] = edge;
            }

            edge.Bytes += flow.Bytes;
            edge.FlowCount++;

            if (flow.Verdict == FlowVerdict.Dropped || flow.Verdict == FlowVerdict.Error)
            {
                edge.DeniedCount++;
            }

            edge.DominantVerdict = edge.DeniedCount > edge.FlowCount / 2
                ? FlowVerdict.Dropped
                : FlowVerdict.Forwarded;
        }

        graph.Nodes.AddRange(nodeMap.Values);
        graph.Edges.AddRange(edgeMap.Values);
    }

    private void BuildAggregatedGraph(ServiceMapGraph graph, List<FlowViewModel> flows)
    {
        var namespaceMap = new Dictionary<string, ServiceMapNode>();
        var edgeMap = new Dictionary<(string, string), ServiceMapEdge>();

        foreach (var flow in flows)
        {
            var sourceNs = flow.SourceNamespace;
            var targetNs = flow.DestinationNamespace;

            if (!namespaceMap.TryGetValue(sourceNs, out var sourceNode))
            {
                sourceNode = new ServiceMapNode
                {
                    Id = sourceNs,
                    Name = sourceNs,
                    Namespace = sourceNs,
                    WorkloadType = "namespace"
                };
                namespaceMap[sourceNs] = sourceNode;
            }

            if (!namespaceMap.TryGetValue(targetNs, out var targetNode))
            {
                targetNode = new ServiceMapNode
                {
                    Id = targetNs,
                    Name = targetNs,
                    Namespace = targetNs,
                    WorkloadType = "namespace"
                };
                namespaceMap[targetNs] = targetNode;
            }

            sourceNode.EgressBytes += flow.Bytes;
            targetNode.IngressBytes += flow.Bytes;

            if (flow.Verdict == FlowVerdict.Dropped || flow.Verdict == FlowVerdict.Error)
            {
                sourceNode.DeniedCount++;
                targetNode.DeniedCount++;
            }
            else if (flow.Verdict == FlowVerdict.Forwarded || flow.Verdict == FlowVerdict.Redirected)
            {
                sourceNode.AllowedCount++;
                targetNode.AllowedCount++;
            }

            var edgeKey = (sourceNs, targetNs);
            if (!edgeMap.TryGetValue(edgeKey, out var edge))
            {
                edge = new ServiceMapEdge
                {
                    SourceId = sourceNs,
                    TargetId = targetNs
                };
                edgeMap[edgeKey] = edge;
            }

            edge.Bytes += flow.Bytes;
            edge.FlowCount++;

            if (flow.Verdict == FlowVerdict.Dropped || flow.Verdict == FlowVerdict.Error)
            {
                edge.DeniedCount++;
            }

            edge.DominantVerdict = edge.DeniedCount > edge.FlowCount / 2
                ? FlowVerdict.Dropped
                : FlowVerdict.Forwarded;
        }

        graph.Nodes.AddRange(namespaceMap.Values);
        graph.Edges.AddRange(edgeMap.Values);
    }

    private static string GetNodeKey(string ns, string pod)
        => $"{ns}/{pod}";

    private ServiceMapNode GetOrCreateNode(string key, string ns, string pod, string? workload)
    {
        if (!_nodes.TryGetValue(key, out var node))
        {
            node = new ServiceMapNode
            {
                Id = key,
                Name = pod,
                Namespace = ns,
                WorkloadType = workload
            };
            _nodes[key] = node;
        }
        return node;
    }
}