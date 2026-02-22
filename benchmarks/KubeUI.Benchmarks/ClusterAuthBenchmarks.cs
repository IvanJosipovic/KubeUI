using System;
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
        _index = new Dictionary<string, V1SelfSubjectAccessReview>();

        for (int i = 0; i < 1000; i++)
        {
            var review = new V1SelfSubjectAccessReview()
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
                }
            };

            // set status via reflection
            var statusType = typeof(V1SelfSubjectAccessReview).Assembly.GetType("k8s.Models.V1SelfSubjectAccessReviewStatus");
            if (statusType != null)
            {
                var status = Activator.CreateInstance(statusType)!;
                var allowedProp = statusType.GetProperty("Allowed");
                allowedProp?.SetValue(status, (i % 3 == 0));
                typeof(V1SelfSubjectAccessReview).GetProperty("Status")?.SetValue(review, status);
            }

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
