using KubernetesCRDModelGen;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace KubeUI.Core.Client;

public class ClusterManager : IDisposable
{
    private ILogger<ClusterManager> Logger;

    [JsonInclude]
    private List<ICluster> _clusters = new List<ICluster>();

    [JsonInclude]
    private ICluster? activeCluster;

    private ILoggerFactory loggerFactory;

    private ICRDGenerator cRDGenerator;

    public event Action<ClusterManagerEvents> OnChange;

    public ClusterManager(ILoggerFactory loggerFactory, ICRDGenerator cRDGenerator)
    {
        this.loggerFactory = loggerFactory;
        Logger = loggerFactory.CreateLogger<ClusterManager>();
        this.cRDGenerator = cRDGenerator;

        Init();

        LoadClusters();

        AddGitOpsCluster("GitOps");

        LoadFromConfigFromPath(KubernetesClientConfiguration.KubeConfigDefaultLocation);
    }

    public void LoadFromConfigFromPath(string path)
    {
        if (File.Exists(path))
        {
            var config = KubernetesClientConfiguration.LoadKubeConfig(path);

            foreach (var item in config.Contexts)
            {
                AddCluster(new Cluster(loggerFactory, cRDGenerator) { Name = item.Name, KubeConfigPath = config.FileName });
            }
        }
    }

    public void LoadFromConfig(string kubeConfig)
    {
        var config = KubernetesClientConfiguration.LoadKubeConfig(new MemoryStream(Encoding.UTF8.GetBytes(kubeConfig)));

        foreach (var item in config.Contexts)
        {
            AddCluster(new Cluster(loggerFactory, cRDGenerator) { Name = item.Name, KubeConfigPath = config.FileName });
        }
    }

    public void LoadFromConfig(Stream stream)
    {
        var config = KubernetesClientConfiguration.LoadKubeConfig(stream);

        foreach (var item in config.Contexts)
        {
            AddCluster(new Cluster(loggerFactory, cRDGenerator) { Name = item.Name, KubeConfigPath = config.FileName });
        }
    }

    private void NotifyStateChanged(ClusterManagerEvents events) => OnChange?.Invoke(events);

    public void AddCluster(ICluster cluster)
    {
        _clusters.Add(cluster);
        NotifyStateChanged(ClusterManagerEvents.ClusterAdded);
    }

    public void RemoveCluster(ICluster cluster)
    {
        _clusters.Remove(cluster);
        NotifyStateChanged(ClusterManagerEvents.ClusterRemoved);
    }

    public void SetActiveCluster(ICluster cluster)
    {
        activeCluster = cluster;
        NotifyStateChanged(ClusterManagerEvents.ActiveClusterChanged);
    }

    public IEnumerable<ICluster> GetClusters()
    {
        return _clusters.OrderBy(x => x.Name);
    }

    public ICluster? GetCluster(string name)
    {
        return _clusters.FirstOrDefault(c => c.Name == name);
    }

    public ICluster? GetActiveCluster()
    {
        return activeCluster;
    }

    public void LoadClusters()
    {
    }

    public void SaveClusters()
    {
    }

    public void AddGitOpsCluster(string? name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "GitOps " + _clusters.Count;
        }

        AddCluster(new GitOpsCluster(loggerFactory.CreateLogger<GitOpsCluster>(), cRDGenerator) { Name = name });
    }

    public void Dispose()
    {
        SaveClusters();
    }

    private void Init()
    {
        CRDGenerator.FixSerializer();

        var assebly = typeof(CRDGenerator).Assembly;

        var kubeAssebly = typeof(V1Deployment).Assembly;
        var kubeAsseblyXmlDoc = new XmlDocument();
        kubeAsseblyXmlDoc.Load(assebly.GetManifestResourceStream("runtime.KubernetesClient.Models.xml"));

        AssemblyLoader.AddToCache(kubeAssebly, kubeAsseblyXmlDoc);

        var coreAssebly = typeof(Cluster).Assembly;

        initType(typeof(KubernetesCRDModelGen.Models.fluxcd.io.HelmRelease));

        void initType(Type type)
        {
            var kubeAsseblyXmlDoc = new XmlDocument();
            kubeAsseblyXmlDoc.Load(coreAssebly.GetManifestResourceStream($"model.docs.{Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.Name)}.xml"));
            AssemblyLoader.AddToCache(type.Assembly, null);
        }
    }
}

public enum ClusterManagerEvents
{
    ClusterAdded,
    ClusterRemoved,
    ActiveClusterChanged,
}
