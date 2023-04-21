using KubernetesCRDModelGen;
using KubernetesCRDModelGen.Models.helm.toolkit.fluxcd.io;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;

namespace KubeUI.Core.Client;

public class ClusterManager : IDisposable
{
    private readonly ILogger<ClusterManager> Logger;

    [JsonInclude]
    private readonly List<ICluster> _clusters = new List<ICluster>();

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
    }

    public void LoadFromConfigFromPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (File.Exists(path))
        {
            var config = KubernetesClientConfiguration.LoadKubeConfig(path);

            foreach (var item in config.Contexts)
            {
                AddCluster(new Cluster(loggerFactory, cRDGenerator) { Name = item.Name, KubeConfigPath = config.FileName, KubeConfig = config });
            }
        }
    }

    public void LoadFromConfig(string kubeConfig)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);

        var config = KubernetesClientConfiguration.LoadKubeConfig(new MemoryStream(Encoding.UTF8.GetBytes(kubeConfig)));

        foreach (var item in config.Contexts)
        {
            AddCluster(new Cluster(loggerFactory, cRDGenerator) { Name = item.Name, KubeConfigPath = config.FileName, KubeConfig = config });
        }
    }

    public void LoadFromConfig(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var config = KubernetesClientConfiguration.LoadKubeConfig(stream);

        foreach (var item in config.Contexts)
        {
            AddCluster(new Cluster(loggerFactory, cRDGenerator) { Name = item.Name, KubeConfigPath = config.FileName, KubeConfig = config });
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
        return _clusters.Find(c => c.Name == name);
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

        var cluster = new GitOpsCluster(loggerFactory.CreateLogger<GitOpsCluster>(), cRDGenerator) { Name = name };

        AddCluster(cluster);
    }

    public void Dispose()
    {
        SaveClusters();
    }

    private void Init()
    {
        var kubeAssemblyXmlDoc = new XmlDocument();
        kubeAssemblyXmlDoc.Load(typeof(CRDGenerator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.Models.xml"));

        ModelCache.AddToCache(typeof(V1Deployment).Assembly, kubeAssemblyXmlDoc);

        var coreAssembly = typeof(Cluster).Assembly;

        initType(typeof(V2beta1HelmRelease));

        void initType(Type type)
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(coreAssembly.GetManifestResourceStream($"model.docs.{type.Assembly.GetName().Name}.xml"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error Loading docs for type {group}", type.FullName);
            }

            ModelCache.AddToCache(type.Assembly, xmlDoc);
        }

        LoadClusters();

        LoadFromConfigFromPath(KubernetesClientConfiguration.KubeConfigDefaultLocation);

        AddGitOpsCluster("GitOps");
    }
}

public enum ClusterManagerEvents
{
    ClusterAdded,
    ClusterRemoved,
    ActiveClusterChanged,
}
