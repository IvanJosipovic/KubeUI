using System.Collections.Generic;
using KubeUI.Cilium.Hubble.Models;

namespace KubeUI.Cilium.Hubble;

/// <summary>Builds a time-scoped service graph from Hubble flow events.</summary>
public sealed class ServiceMapGraphBuilder
{
    private static readonly TimeSpan s_maxRetention = TimeSpan.FromHours(1);
    private readonly Lock _gate = new();
    private readonly FlowViewModel[] _recentFlows;
    private readonly int _maxRetainedFlows;
    private int _nextFlowIndex;
    private int _flowCount;

    /// <summary>Creates a graph builder with a bounded event history.</summary>
    /// <param name="maxRetainedFlows">Maximum number of flow events retained in memory.</param>
    public ServiceMapGraphBuilder(int maxRetainedFlows = 100_000)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRetainedFlows);
        _maxRetainedFlows = maxRetainedFlows;
        _recentFlows = new FlowViewModel[maxRetainedFlows];
    }

    /// <summary>Adds one flow event to the rolling graph window.</summary>
    public void AddFlow(FlowViewModel flow)
    {
        ArgumentNullException.ThrowIfNull(flow);

        lock (_gate)
        {
            _recentFlows[_nextFlowIndex] = flow;
            _nextFlowIndex = (_nextFlowIndex + 1) % _maxRetainedFlows;
            _flowCount = Math.Min(_flowCount + 1, _maxRetainedFlows);
        }
    }

    /// <summary>Creates a graph for the requested time and namespace scope.</summary>
    public ServiceMapGraph Build(
        TimeSpan timeWindow,
        bool aggregateByNamespace = false,
        bool showOnlyErrors = false,
        IReadOnlyCollection<string>? namespaces = null,
        CancellationToken cancellationToken = default)
    {
        var flows = GetFlows(timeWindow, showOnlyErrors, namespaces, cancellationToken);
        var graph = new ServiceMapGraph();

        if (aggregateByNamespace)
        {
            BuildAggregatedGraph(graph, flows, cancellationToken);
        }
        else
        {
            BuildServiceGraph(graph, flows, cancellationToken);
        }

        return graph;
    }

    /// <summary>Gets the recent flows matching the requested map scope.</summary>
    public FlowViewModel[] GetFlows(
        TimeSpan timeWindow,
        bool showOnlyErrors = false,
        IReadOnlyCollection<string>? namespaces = null,
        CancellationToken cancellationToken = default)
    {
        if (timeWindow <= TimeSpan.Zero || namespaces is not { Count: > 0 })
        {
            return [];
        }

        var now = DateTime.UtcNow;
        var cutoff = now - timeWindow;
        var retentionCutoff = now - s_maxRetention;
        var namespaceSet = new HashSet<string>(namespaces, StringComparer.Ordinal);
        FlowViewModel[] snapshot;
        lock (_gate)
        {
            snapshot = new FlowViewModel[_flowCount];
            var oldestIndex = (_nextFlowIndex - _flowCount + _maxRetainedFlows) % _maxRetainedFlows;
            var firstSegmentLength = Math.Min(_flowCount, _maxRetainedFlows - oldestIndex);
            Array.Copy(_recentFlows, oldestIndex, snapshot, 0, firstSegmentLength);
            if (firstSegmentLength < _flowCount)
            {
                Array.Copy(_recentFlows, 0, snapshot, firstSegmentLength, _flowCount - firstSegmentLength);
            }
        }

        var matchingFlows = new List<FlowViewModel>();
        for (var index = 0; index < snapshot.Length; index++)
        {
            if ((index & 0xFF) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var flow = snapshot[index];
            if (flow.Timestamp < cutoff
                || flow.Timestamp < retentionCutoff
                || flow.IsReply
                || (showOnlyErrors && !IsDenied(flow.Verdict))
                || (!namespaceSet.Contains(flow.SourceNamespace)
                    && !namespaceSet.Contains(flow.DestinationNamespace)))
            {
                continue;
            }

            matchingFlows.Add(flow);
        }

        return matchingFlows.ToArray();
    }

    /// <summary>Gets recent flows connected to a graph node within the selected namespace scope.</summary>
    public FlowViewModel[] GetFlowsForNode(
        ServiceMapNode node,
        TimeSpan timeWindow,
        bool aggregateByNamespace,
        bool showOnlyErrors,
        IReadOnlyCollection<string> namespaces,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(node);
        var flows = GetFlows(timeWindow, showOnlyErrors, namespaces, cancellationToken);
        var matchingFlows = new List<FlowViewModel>();
        foreach (var flow in flows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (aggregateByNamespace
                ? node.Id == GetNamespaceKey(flow.SourceNamespace) || node.Id == GetNamespaceKey(flow.DestinationNamespace)
                : node.Id == GetEndpoint(flow, isSource: true).Key || node.Id == GetEndpoint(flow, isSource: false).Key)
            {
                matchingFlows.Add(flow);
            }
        }

        return matchingFlows.ToArray();
    }

    private static void BuildServiceGraph(ServiceMapGraph graph, IReadOnlyList<FlowViewModel> flows, CancellationToken cancellationToken)
    {
        var nodes = new Dictionary<string, ServiceMapNode>(StringComparer.Ordinal);
        var edges = new Dictionary<(string Source, string Target), ServiceMapEdge>();

        foreach (var flow in flows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = GetEndpoint(flow, isSource: true);
            var target = GetEndpoint(flow, isSource: false);
            var sourceNode = GetOrCreateNode(nodes, source);
            var targetNode = GetOrCreateNode(nodes, target);

            AddNodeFlow(sourceNode, flow, isSource: true);
            AddNodeFlow(targetNode, flow, isSource: false);
            AddNodeAccessPoint(targetNode, flow);
            AddEdge(edges, source.Key, target.Key, flow);
        }

        AddSortedNodesAndEdges(graph, nodes, edges);
    }

    private static void BuildAggregatedGraph(ServiceMapGraph graph, IReadOnlyList<FlowViewModel> flows, CancellationToken cancellationToken)
    {
        var nodes = new Dictionary<string, ServiceMapNode>(StringComparer.Ordinal);
        var edges = new Dictionary<(string Source, string Target), ServiceMapEdge>();

        foreach (var flow in flows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceKey = GetNamespaceKey(flow.SourceNamespace);
            var targetKey = GetNamespaceKey(flow.DestinationNamespace);
            var sourceNode = GetOrCreateNamespaceNode(nodes, sourceKey, flow.SourceNamespace);
            var targetNode = GetOrCreateNamespaceNode(nodes, targetKey, flow.DestinationNamespace);

            AddNodeFlow(sourceNode, flow, isSource: true);
            AddNodeFlow(targetNode, flow, isSource: false);
            AddNodeAccessPoint(targetNode, flow);
            AddEdge(edges, sourceKey, targetKey, flow);
        }

        AddSortedNodesAndEdges(graph, nodes, edges);
    }

    private static ServiceMapNode GetOrCreateNode(
        Dictionary<string, ServiceMapNode> nodes,
        EndpointNode endpoint)
    {
        if (!nodes.TryGetValue(endpoint.Key, out var node))
        {
            node = new ServiceMapNode
            {
                Id = endpoint.Key,
                Name = endpoint.Name,
                Namespace = endpoint.Namespace,
                WorkloadType = endpoint.Type
            };
            nodes.Add(endpoint.Key, node);
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
        FlowViewModel flow)
    {
        var edgeKey = (Source: sourceKey, Target: targetKey);
        if (!edges.TryGetValue(edgeKey, out var edge))
        {
            edge = new ServiceMapEdge { SourceId = sourceKey, TargetId = targetKey };
            edges.Add(edgeKey, edge);
        }

        edge.FlowCount++;
        if (IsDenied(flow.Verdict))
        {
            edge.DeniedCount++;
        }

        var accessPointKey = (flow.Protocol, flow.DestinationPort);
        if (!edge.AccessPointIndex.TryGetValue(accessPointKey, out var accessPoint))
        {
            accessPoint = new ServiceMapAccessPoint
            {
                Protocol = flow.Protocol,
                Port = flow.DestinationPort
            };
            edge.AccessPointIndex.Add(accessPointKey, accessPoint);
            edge.AccessPoints.Add(accessPoint);
        }

        accessPoint.FlowCount++;
        if (IsDenied(flow.Verdict))
        {
            accessPoint.DeniedCount++;
        }

        if (!string.IsNullOrWhiteSpace(flow.ApplicationProtocol)
            && accessPoint.ApplicationProtocolIndex.Add(flow.ApplicationProtocol))
        {
            accessPoint.ApplicationProtocols.Add(flow.ApplicationProtocol);
        }

        edge.DominantVerdict = edge.DeniedCount * 2 > edge.FlowCount
            ? FlowVerdict.Dropped
            : FlowVerdict.Forwarded;
    }

    private static void AddNodeAccessPoint(
        ServiceMapNode node,
        FlowViewModel flow)
    {
        var accessPointKey = (flow.Protocol, flow.DestinationPort);
        if (!node.AccessPointIndex.TryGetValue(accessPointKey, out var accessPoint))
        {
            accessPoint = new ServiceMapAccessPoint
            {
                Protocol = flow.Protocol,
                Port = flow.DestinationPort
            };
            node.AccessPointIndex.Add(accessPointKey, accessPoint);
            node.AccessPoints.Add(accessPoint);
        }

        accessPoint.FlowCount++;
        if (IsDenied(flow.Verdict))
        {
            accessPoint.DeniedCount++;
        }

        if (!string.IsNullOrWhiteSpace(flow.ApplicationProtocol)
            && accessPoint.ApplicationProtocolIndex.Add(flow.ApplicationProtocol))
        {
            accessPoint.ApplicationProtocols.Add(flow.ApplicationProtocol);
        }
    }

    private static EndpointNode GetEndpoint(FlowViewModel flow, bool isSource)
    {
        var ns = isSource ? flow.SourceNamespace : flow.DestinationNamespace;
        var serviceName = isSource ? flow.SourceServiceName : flow.DestinationServiceName;
        var serviceNamespace = isSource ? flow.SourceServiceNamespace : flow.DestinationServiceNamespace;
        var workload = isSource ? flow.SourceWorkload : flow.DestinationWorkload;
        var workloadKind = isSource ? flow.SourceWorkloadKind : flow.DestinationWorkloadKind;
        var pod = isSource ? flow.SourcePod : flow.DestinationPod;
        var ip = isSource ? flow.SourceIp : flow.DestinationIp;
        var identity = isSource ? flow.SourceIdentity : flow.DestinationIdentity;

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            ns = string.IsNullOrWhiteSpace(serviceNamespace) ? ns : serviceNamespace;
            return new EndpointNode($"service/{ns}/{serviceName}", serviceName, ns, "service");
        }

        if (!string.IsNullOrWhiteSpace(workload))
        {
            var kind = string.IsNullOrWhiteSpace(workloadKind) ? "workload" : workloadKind;
            return new EndpointNode($"workload/{ns}/{kind}/{workload}", workload, ns, kind);
        }

        var isWorld = isSource ? flow.SourceIsWorld : flow.DestinationIsWorld;
        if (isWorld)
        {
            var names = isSource ? flow.SourceNames : flow.DestinationNames;
            var side = isSource ? "sender" : "receiver";
            if (names.Length > 0 && !string.IsNullOrWhiteSpace(names[0]))
            {
                var dnsName = names[0];
                return new EndpointNode($"{dnsName}-{side}", dnsName, ns, "dns");
            }

            return new EndpointNode($"world-{side}", "World", ns, "world");
        }

        if (!string.IsNullOrWhiteSpace(pod))
        {
            return new EndpointNode($"{ns}/{pod}", pod, ns, "Pod");
        }

        if (!string.IsNullOrWhiteSpace(ip))
        {
            return new EndpointNode($"ip/{ip}", ip, ns, null);
        }

        var displayName = identity == 0 ? "World" : $"Identity {identity}";
        var key = identity == 0 ? "world" : $"identity/{identity}";
        return new EndpointNode(key, displayName, ns, null);
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
        foreach (var edge in graph.Edges)
        {
            edge.AccessPoints.Sort(static (left, right) =>
            {
                var protocolOrder = StringComparer.Ordinal.Compare(left.Protocol, right.Protocol);
                if (protocolOrder != 0)
                {
                    return protocolOrder;
                }

                var portOrder = left.Port.CompareTo(right.Port);
                return portOrder != 0
                    ? portOrder
                    : StringComparer.Ordinal.Compare(left.ApplicationProtocols.FirstOrDefault(), right.ApplicationProtocols.FirstOrDefault());
            });
        }
        foreach (var node in graph.Nodes)
        {
            node.AccessPoints.Sort(static (left, right) =>
            {
                var protocolOrder = StringComparer.Ordinal.Compare(left.Protocol, right.Protocol);
                if (protocolOrder != 0)
                {
                    return protocolOrder;
                }

                var portOrder = left.Port.CompareTo(right.Port);
                return portOrder != 0
                    ? portOrder
                    : StringComparer.Ordinal.Compare(left.ApplicationProtocols.FirstOrDefault(), right.ApplicationProtocols.FirstOrDefault());
            });
        }
    }

    private static bool IsDenied(FlowVerdict verdict)
        => verdict is FlowVerdict.Dropped or FlowVerdict.Error;

    private static string GetNamespaceKey(string ns)
        => string.IsNullOrEmpty(ns) ? "world" : $"namespace/{ns}";

    private readonly record struct EndpointNode(string Key, string Name, string Namespace, string? Type);
}
