using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using k8s.Models;
using KubeUI.Client;
using Microsoft.VSDiagnostics;

namespace KubeUI.Benchmarks;

[CPUUsageDiagnoser]
public class ClusterAuthBenchmarks
{
    private Dictionary<string, V1SelfSubjectAccessReview> _index = null!;

    [GlobalSetup]
    public void Setup()
    {
        _index = [];

        for (int i = 0; i < 1000; i++)
        {
            var review = new V1SelfSubjectAccessReview
            {
                Spec = new V1SelfSubjectAccessReviewSpec()
                {
                    ResourceAttributes = new V1ResourceAttributes
                    {
                        Group = "",
                        NamespaceProperty = i % 10 == 0 ? null : $"ns{i % 10}",
                        Resource = "pods",
                        Subresource = null,
                        Verb = "list",
                        Version = "v1"
                    }
                },
                Status = new V1SubjectAccessReviewStatus
                {
                    Allowed = (i % 3 == 0)
                }
            };

            var key = Cluster.BuildReviewKeyFromParts("", "pods", "v1", "list", (i % 10 == 0 ? null : $"ns{i % 10}"), null);
            _index[key] = review;
        }
    }

    [Benchmark]
    public bool FindReviewIndexed_Benchmark()
    {
        var r = Cluster.FindReviewIndexedByParts(_index, "", "pods", "v1", "list", "ns1", null);
        return r?.Status?.Allowed == true;
    }
}
