using System.Xml;
using k8s.Models;
using KubeUI.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public class ModelCacheTests
{
    [Fact]
    public void AddToCache_PopulatesTypeCache()
    {
        var cache = new ModelCache();

        cache.AddToCache(typeof(V1Pod).Assembly, new XmlDocument());

        cache.GetResourceType(string.Empty, V1Pod.KubeApiVersion, V1Pod.KubeKind).ShouldBe(typeof(V1Pod));
    }

    [Fact]
    public void AddToCache_IgnoresDuplicateAssemblies()
    {
        var cache = new ModelCache();
        var xml1 = new XmlDocument();
        var xml2 = new XmlDocument();

        cache.AddToCache(typeof(V1Pod).Assembly, xml1);
        cache.AddToCache(typeof(V1Pod).Assembly, xml2);

        cache.Cache.Count.ShouldBe(1);
        cache.Cache[typeof(V1Pod).Assembly].ShouldBe(xml1);
    }
}
