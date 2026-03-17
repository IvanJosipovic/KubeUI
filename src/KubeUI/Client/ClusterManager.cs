using System.Diagnostics;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using k8s.KubeConfigModels;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;

namespace KubeUI.Client;

public class ClusterComparer : IComparer<ICluster>
{
    public int Compare(ICluster? x, ICluster? y)
    {
        return x?.Name.CompareTo(y?.Name) ?? 0;
    }
}

public sealed partial class ClusterManager : ObservableObject, IDisposable
{
    public ObservableSortedCollection<ICluster> Clusters { get; set; } = new(new ClusterComparer());

    private readonly ILogger<ClusterManager> _logger;

    private readonly IServiceProvider _serviceProvider;

    private readonly IFactory _factory;

    private readonly ISettingsService _settingsService;

    private readonly IDictionary<string, FileSystemWatcher> _fileWatchers = new Dictionary<string, FileSystemWatcher>();

    public ClusterManager(ILogger<ClusterManager> logger, IServiceProvider serviceProvider, IFactory factory, ISettingsService settingsService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _factory = factory;
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

        var doc = _factory.GetDockable<IDocumentDock>("Documents");

        var vm = new ClusterErrorViewModel()
        {
            Id = "cluster-error",
            Error = e.Data
        };

        _logger.LogError("Cluster ExecStdError: {data}", e.Data);

        Dispatcher.UIThread.Post(() =>
        {
            var existingDock = _factory.FindDockableById(vm.Id);

            if (existingDock != null)
            {
                _factory?.CloseDockable(existingDock);
            }

            _factory?.AddDockable(doc, vm);
            _factory?.SetActiveDockable(vm);
            _factory?.SetFocusedDockable(doc, vm);
        });
    }

    private void WatchKubeConfig(string path)
    {
        if (_fileWatchers.ContainsKey(path))
        {
            return;
        }

        var dir = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

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
            _logger.LogError(ex, "Error Watching Kube Config {0}", e.FullPath);
        }
    }

    private void LoadClusters()
    {
        if (!_settingsService.Settings.KubeConfigs.Contains(KubernetesClientConfiguration.KubeConfigDefaultLocation))
        {
            // Load Default Kube Config
            try
            {
                LoadFromConfigFromPath(KubernetesClientConfiguration.KubeConfigDefaultLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from Default KubeConfig");
            }
        }

        // Load Saved Kube Configs
        foreach (var config in _settingsService.Settings.KubeConfigs)
        {
            try
            {
                LoadFromConfigFromPath(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from KubeConfig: " + config);
            }
        }
    }

    public void LoadFromConfigFromPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (File.Exists(path))
        {
            if (!SettingsContainsKubeConfig(path))
            {
                AddKubeConfig(path);
            }

            try
            {
                WatchKubeConfig(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to watch Kube Config {0}", path);
            }

            var count = 0;

            do
            {
                try
                {
                    var config = KubernetesClientConfiguration.LoadKubeConfig(path);

                    foreach (var item in config.Contexts)
                    {
                        var cluster = _serviceProvider.GetRequiredService<ICluster>();

                        cluster.Name = item.Name;

                        cluster.KubeConfigPath = config.FileName;

                        if (!ClusterExists(cluster.Name, cluster.KubeConfigPath))
                        {
                            AddCluster(cluster);
                        }
                    }
                    //todo remove all clusters which are no longer in the config

                    return;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Unable to open Kube Config {0}", path);
                    Task.Delay(1000).GetAwaiter().GetResult();
                    count++;
                }
            } while (count <= 5);
        }
    }

    public void LoadFromConfig(K8SConfiguration kubeConfig)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);

        foreach (var item in kubeConfig.Contexts)
        {
            var cluster = _serviceProvider.GetRequiredService<ICluster>();

            cluster.Name = item.Name;

            cluster.KubeConfig = kubeConfig;

            cluster.KubeConfigPath = kubeConfig.FileName;

            AddCluster(cluster);
        }
    }

    private bool SettingsContainsKubeConfig(string path)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return _settingsService.Settings.KubeConfigs.Contains(path);
        }

        return Dispatcher.UIThread.InvokeAsync(() => _settingsService.Settings.KubeConfigs.Contains(path), DispatcherPriority.Background).GetAwaiter().GetResult();
    }

    private void AddKubeConfig(string path)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            _settingsService.Settings.KubeConfigs.Add(path);
            _settingsService.SaveSettings();
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _settingsService.Settings.KubeConfigs.Add(path);
            _settingsService.SaveSettings();
        }, DispatcherPriority.Background).GetAwaiter().GetResult();
    }

    private bool ClusterExists(string name, string kubeConfigPath)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return Clusters.Any(x => x.Name == name && x.KubeConfigPath == kubeConfigPath);
        }

        return Dispatcher.UIThread.InvokeAsync(() => Clusters.Any(x => x.Name == name && x.KubeConfigPath == kubeConfigPath), DispatcherPriority.Background).GetAwaiter().GetResult();
    }

    private void AddCluster(ICluster cluster)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Clusters.Add(cluster);
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() => Clusters.Add(cluster), DispatcherPriority.Background).GetAwaiter().GetResult();
    }

    public ICluster? GetCluster(string name)
    {
        return Clusters.FirstOrDefault(c => c.Name == name);
    }

    public void RemoveCluster(ICluster cluster)
    {
        if (!string.IsNullOrEmpty(cluster.KubeConfigPath))
        {
            try
            {
                if (File.Exists(cluster.KubeConfigPath))
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Removing Cluster from configuration");
            }
        }
    }

    public ICluster? GetDefault()
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
        catch (Exception)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _fileWatchers.ForEach(x => x.Value.Dispose());
    }
}
