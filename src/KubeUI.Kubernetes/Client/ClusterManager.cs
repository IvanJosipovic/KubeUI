using System.Collections.ObjectModel;
using System.Diagnostics;
using k8s;
using k8s.KubeConfigModels;
using Swordfish.NET.Collections;

namespace KubeUI.Client;

public sealed class ClusterComparer : IComparer<IClusterRuntime>
{
    public int Compare(IClusterRuntime? x, IClusterRuntime? y)
    {
        return string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
    }
}

public sealed partial class ClusterManager : ObservableObject, IClusterRuntimeCatalog, IDisposable
{
    private readonly ILogger<ClusterManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsService _settingsService;
    private readonly IDictionary<string, FileSystemWatcher> _fileWatchers = new Dictionary<string, FileSystemWatcher>(StringComparer.Ordinal);
    private readonly object _syncRoot = new();

    public ObservableCollection<IClusterRuntime> Clusters { get; } = new ObservableSortedCollection<IClusterRuntime>(new ClusterComparer());

    IEnumerable<IClusterRuntime> IClusterRuntimeCatalog.Clusters => Clusters;

    public ClusterManager(ILogger<ClusterManager> logger, IServiceProvider serviceProvider, ISettingsService settingsService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settingsService = settingsService;

        KubernetesClientConfiguration.ExecStdError += KubernetesClientConfiguration_ExecStdError;

        LoadClusters();
    }

    private void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            _logger.LogError("Cluster ExecStdError: no data");
            return;
        }

        _logger.LogError("Cluster ExecStdError: {data}", e.Data);
    }

    private void LoadClusters()
    {
        if (!_settingsService.Settings.KubeConfigs.Contains(KubernetesClientConfiguration.KubeConfigDefaultLocation))
        {
            try
            {
                LoadFromConfigFromPath(KubernetesClientConfiguration.KubeConfigDefaultLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from Default KubeConfig");
            }
        }

        foreach (var config in _settingsService.Settings.KubeConfigs)
        {
            try
            {
                LoadFromConfigFromPath(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from KubeConfig: {Config}", config);
            }
        }
    }

    public void LoadFromConfigFromPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!File.Exists(path))
        {
            return;
        }

        lock (_syncRoot)
        {
            if (!_settingsService.Settings.KubeConfigs.Contains(path))
            {
                _settingsService.Settings.KubeConfigs.Add(path);
                _settingsService.SaveSettings();
            }
        }

        WatchKubeConfig(path);

        var count = 0;

        do
        {
            try
            {
                var config = KubernetesClientConfiguration.LoadKubeConfig(path);

                foreach (var item in config.Contexts)
                {
                    var cluster = _serviceProvider.GetRequiredService<IClusterRuntime>();

                    cluster.Name = item.Name;
                    cluster.KubeConfigPath = config.FileName;

                    if (!ClusterExists(cluster.Name, cluster.KubeConfigPath))
                    {
                        Clusters.Add(cluster);
                    }
                }

                return;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Unable to open Kube Config {Path}", path);
                Task.Delay(1000).GetAwaiter().GetResult();
                count++;
            }
        } while (count <= 5);
    }

    public void LoadFromConfig(K8SConfiguration kubeConfig)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);

        foreach (var item in kubeConfig.Contexts)
        {
            var cluster = _serviceProvider.GetRequiredService<IClusterRuntime>();

            cluster.Name = item.Name;
            cluster.KubeConfig = kubeConfig;
            cluster.KubeConfigPath = kubeConfig.FileName;

            if (!ClusterExists(cluster.Name, cluster.KubeConfigPath))
            {
                Clusters.Add(cluster);
            }
        }
    }

    public IClusterRuntime? GetCluster(string name)
    {
        return Clusters.FirstOrDefault(c => c.Name == name);
    }

    public IClusterRuntime? GetDefault()
    {
        try
        {
            if (File.Exists(KubernetesClientConfiguration.KubeConfigDefaultLocation))
            {
                var kubeConfig = KubernetesClientConfiguration.LoadKubeConfig();
                return GetCluster(kubeConfig.CurrentContext);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to resolve default cluster");
            return null;
        }
    }

    public void RemoveCluster(IClusterRuntime cluster)
    {
        if (string.IsNullOrEmpty(cluster.KubeConfigPath) || !File.Exists(cluster.KubeConfigPath))
        {
            return;
        }

        try
        {
            var config = KubernetesClientConfiguration.LoadKubeConfig(cluster.KubeConfigPath);
            var context = config.Contexts.First(x => x.ContextDetails.Cluster == cluster.Name);

            ((List<k8s.KubeConfigModels.Cluster>)config.Clusters).RemoveAll(c => c.Name == context.ContextDetails.Cluster);
            ((List<k8s.KubeConfigModels.User>)config.Users).RemoveAll(c => c.Name == context.ContextDetails.User);
            ((List<k8s.KubeConfigModels.Context>)config.Contexts).Remove(context);

            var yaml = KubernetesYaml.Serialize(config);
            File.WriteAllText(cluster.KubeConfigPath, yaml);

            Clusters.Remove(cluster);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Removing Cluster from configuration");
        }
    }

    private void WatchKubeConfig(string path)
    {
        if (_fileWatchers.ContainsKey(path))
        {
            return;
        }

        var dir = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(filename))
        {
            return;
        }

        var watcher = new FileSystemWatcher
        {
            Path = dir,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = filename,
            EnableRaisingEvents = true,
        };

        watcher.Changed += Watcher_Changed;
        _fileWatchers.Add(path, watcher);
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        try
        {
            LoadFromConfigFromPath(e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Watching Kube Config {Path}", e.FullPath);
        }
    }

    private bool ClusterExists(string name, string kubeConfigPath)
    {
        return Clusters.Any(x => x.Name == name && x.KubeConfigPath == kubeConfigPath);
    }

    public void Dispose()
    {
        KubernetesClientConfiguration.ExecStdError -= KubernetesClientConfiguration_ExecStdError;

        foreach (var watcher in _fileWatchers.Values)
        {
            watcher.Dispose();
        }
    }
}
