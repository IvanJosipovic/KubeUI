using KubeUI.Kubernetes.Serialization;
using k8s;
using k8s.KubeConfigModels;
using Swordfish.NET.Collections;

namespace KubeUI.Kubernetes;

public sealed class ClusterComparer : IComparer<IClusterRuntime>
{
    public int Compare(IClusterRuntime? x, IClusterRuntime? y)
    {
        return string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
    }
}

public sealed partial class ClusterManager : ObservableObject, IClusterRuntimeCatalog, IDisposable
{
    private readonly IKubernetesYamlSerializer _yamlSerializer;
    private readonly ILogger<ClusterManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IClusterSettingsStore _settings;
    private readonly IThreadDispatcher _dispatcher;
    private readonly IKubeConfigPathProvider _kubeConfigPathProvider;
    private readonly IKubeConfigDiscovery _discovery;
    private readonly HashSet<string> _discoveredPaths = new(StringComparer.Ordinal);
    private FileSystemWatcher? _discoveryWatcher;
    private readonly IDictionary<string, FileSystemWatcher> _fileWatchers = new Dictionary<string, FileSystemWatcher>(StringComparer.Ordinal);
    private readonly Lock _syncRoot = new();

    public ObservableCollection<IClusterRuntime> Clusters { get; } = new ObservableSortedCollection<IClusterRuntime>(new ClusterComparer());

    IEnumerable<IClusterRuntime> IClusterRuntimeCatalog.Clusters => Clusters;

    public ClusterManager(
        ILogger<ClusterManager> logger,
        IServiceProvider serviceProvider,
        IClusterSettingsStore settings,
        IThreadDispatcher dispatcher,
        IKubeConfigPathProvider kubeConfigPathProvider,
        IKubeConfigDiscovery discovery,
        IKubernetesYamlSerializer yamlSerializer)
    {
        _yamlSerializer = yamlSerializer;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings;
        _dispatcher = dispatcher;
        _kubeConfigPathProvider = kubeConfigPathProvider;
        _discovery = discovery;

    }

    internal async Task LoadClustersAsync(CancellationToken cancellationToken)
    {
        var kubeConfigPaths = _settings.KubeConfigPaths ?? [];
        if (!kubeConfigPaths.Contains(_kubeConfigPathProvider.DefaultPath))
        {
            try
            {
                await LoadFromConfigFromPathAsync(_kubeConfigPathProvider.DefaultPath, persist: true, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from Default KubeConfig");
            }
        }

        foreach (var config in kubeConfigPaths.ToArray())
        {
            try
            {
                await LoadFromConfigFromPathAsync(config, persist: true, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from KubeConfig: {Config}", config);
            }
        }

        if (_settings.KubeConfigAutoDiscoveryEnabled)
        {
            await DiscoverKubeConfigsAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DiscoverKubeConfigsAsync(CancellationToken cancellationToken)
    {
        var directory = _kubeConfigPathProvider.DiscoveryDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        StartDiscoveryWatcher(directory);

        var files = await Task.Run(() => _discovery.DiscoverFiles(directory), CancellationToken.None)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var file in files)
        {
            if (!TryTrackDiscoveredPath(file))
            {
                continue;
            }

            try
            {
                await LoadFromConfigFromPathAsync(file, persist: false, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Clusters from discovered KubeConfig: {Config}", file);
            }
        }
    }

    private async Task LoadFromConfigFromPathAsync(string path, bool persist, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!File.Exists(path))
        {
            return;
        }

        if (persist)
        {
            TrackPersistedPath(path);
        }

        var count = 0;

        do
        {
            try
            {
                var config = await Task.Run(() => KubernetesClientConfiguration.LoadKubeConfig(path), CancellationToken.None)
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
                AddClustersFromConfig(config);
                return;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Unable to open Kube Config {Path}", path);
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                count++;
            }
        } while (count <= 5);
    }

    public void LoadFromConfigFromPath(string path)
    {
        LoadFromPath(path, persist: true);
    }

    private void LoadFromPath(string path, bool persist)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!File.Exists(path))
        {
            return;
        }

        if (persist)
        {
            TrackPersistedPath(path);
        }

        var count = 0;

        do
        {
            try
            {
                var config = KubernetesClientConfiguration.LoadKubeConfig(path);
                AddClustersFromConfig(config);
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

    private void TrackPersistedPath(string path)
    {
        lock (_syncRoot)
        {
            _settings.AddKubeConfigPath(path);
            _discoveredPaths.Remove(path);
        }

        WatchKubeConfig(path);
    }

    private bool TryTrackDiscoveredPath(string path)
    {
        lock (_syncRoot)
        {
            if (IsPersistedPath(path))
            {
                return false;
            }

            _discoveredPaths.Add(path);
            return true;
        }
    }

    private bool IsPersistedPath(string path)
    {
        return string.Equals(path, _kubeConfigPathProvider.DefaultPath, StringComparison.Ordinal)
            || (_settings.KubeConfigPaths?.Contains(path) ?? false);
    }

    /// <inheritdoc />
    public bool KubeConfigAutoDiscoveryEnabled => _settings.KubeConfigAutoDiscoveryEnabled;

    /// <inheritdoc />
    public void SetKubeConfigAutoDiscovery(bool enabled)
    {
        if (_settings.KubeConfigAutoDiscoveryEnabled == enabled)
        {
            return;
        }

        _settings.KubeConfigAutoDiscoveryEnabled = enabled;
        OnPropertyChanged(nameof(KubeConfigAutoDiscoveryEnabled));

        if (enabled)
        {
            DiscoverKubeConfigs();
            return;
        }

        StopDiscoveryWatcher();

        string[] discovered;
        lock (_syncRoot)
        {
            discovered = [.. _discoveredPaths];
            _discoveredPaths.Clear();
        }

        foreach (var path in discovered)
        {
            RemoveClustersForPath(path);
        }
    }

    private void DiscoverKubeConfigs()
    {
        var directory = _kubeConfigPathProvider.DiscoveryDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        StartDiscoveryWatcher(directory);

        foreach (var file in _discovery.DiscoverFiles(directory))
        {
            if (TryTrackDiscoveredPath(file))
            {
                LoadFromPath(file, persist: false);
            }
        }
    }

    /// <inheritdoc />
    public void RemoveKubeConfig(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (string.Equals(path, _kubeConfigPathProvider.DefaultPath, StringComparison.Ordinal))
        {
            _logger.LogWarning("The default kubeconfig {Path} cannot be removed", path);
            return;
        }

        lock (_syncRoot)
        {
            _settings.RemoveKubeConfigPath(path);
        }

        UnwatchKubeConfig(path);

        if (IsDiscoverable(path) && TryTrackDiscoveredPath(path))
        {
            return;
        }

        RemoveClustersForPath(path);
    }

    private bool IsDiscoverable(string path)
    {
        var directory = _kubeConfigPathProvider.DiscoveryDirectory;
        return _settings.KubeConfigAutoDiscoveryEnabled
            && !string.IsNullOrWhiteSpace(directory)
            && string.Equals(Path.GetDirectoryName(path), directory, StringComparison.Ordinal)
            && _discovery.IsKubeConfig(path);
    }

    private void RemoveClustersForPath(string path)
    {
        _dispatcher.Invoke(() =>
        {
            foreach (var cluster in Clusters.Where(x => string.Equals(x.KubeConfigPath, path, StringComparison.Ordinal)).ToArray())
            {
                Clusters.Remove(cluster);
            }
        });
    }

    public void LoadFromConfig(K8SConfiguration kubeConfig)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);

        AddClustersFromConfig(kubeConfig);
    }

    private void AddClustersFromConfig(K8SConfiguration kubeConfig)
    {
        foreach (var item in kubeConfig.Contexts)
        {
            var cluster = _serviceProvider.GetRequiredService<IClusterRuntime>();

            cluster.Name = item.Name;
            cluster.KubeConfig = kubeConfig;
            cluster.KubeConfigPath = kubeConfig.FileName;
            AddCluster(cluster);
        }
    }

    public void AddCluster(IClusterRuntime cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);

        _dispatcher.Invoke(() =>
        {
            if (!ClusterExists(cluster.Name, cluster.KubeConfigPath))
            {
                Clusters.Add(cluster);
            }
        });
    }

    public void ImportIntoKubeConfig(K8SConfiguration kubeConfig)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);

        var path = _kubeConfigPathProvider.DefaultPath;
        var existing = File.Exists(path)
            ? _yamlSerializer.Deserialize<K8SConfiguration>(File.ReadAllText(path))
            : new K8SConfiguration();

        K8SConfiguration merged = new()
        {
            ApiVersion = string.IsNullOrWhiteSpace(kubeConfig.ApiVersion) ? existing.ApiVersion ?? "v1" : kubeConfig.ApiVersion,
            Kind = string.IsNullOrWhiteSpace(kubeConfig.Kind) ? existing.Kind ?? "Config" : kubeConfig.Kind,
            CurrentContext = string.IsNullOrWhiteSpace(kubeConfig.CurrentContext)
                ? existing.CurrentContext
                : kubeConfig.CurrentContext,
            Preferences = existing.Preferences ?? kubeConfig.Preferences,
            Clusters = MergeNamedItems(existing.Clusters, kubeConfig.Clusters, cluster => cluster.Name),
            Users = MergeNamedItems(existing.Users, kubeConfig.Users, user => user.Name),
            Contexts = MergeNamedItems(existing.Contexts, kubeConfig.Contexts, context => context.Name),
            Extensions = MergeNamedItems(existing.Extensions, kubeConfig.Extensions, extension => extension.Name),
            FileName = path
        };

        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var yaml = _yamlSerializer.Serialize(merged);
        File.WriteAllText(path, yaml);

        LoadFromConfigFromPath(path);
    }

    public IClusterRuntime? GetCluster(string name)
    {
        return Clusters.FirstOrDefault(c => c.Name == name);
    }

    public IClusterRuntime? GetDefault()
    {
        try
        {
            if (File.Exists(_kubeConfigPathProvider.DefaultPath))
            {
                var kubeConfig = KubernetesClientConfiguration.LoadKubeConfig(_kubeConfigPathProvider.DefaultPath);
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
        ArgumentNullException.ThrowIfNull(cluster);

        if (string.IsNullOrEmpty(cluster.KubeConfigPath) || !File.Exists(cluster.KubeConfigPath))
        {
            _dispatcher.Invoke(() => Clusters.Remove(cluster));
            return;
        }

        try
        {
            var config = KubernetesClientConfiguration.LoadKubeConfig(cluster.KubeConfigPath);
            var context = config.Contexts.FirstOrDefault(x =>
                string.Equals(x.Name, cluster.Name, StringComparison.Ordinal) ||
                string.Equals(x.ContextDetails?.Cluster, cluster.Name, StringComparison.Ordinal));

            if (context is null)
            {
                _logger.LogWarning(
                    "Unable to remove cluster {Cluster} from kubeconfig {Path} because no matching context was found.",
                    cluster.Name,
                    cluster.KubeConfigPath);
                return;
            }

            if (!string.IsNullOrWhiteSpace(context.ContextDetails?.Cluster))
            {
                ((List<k8s.KubeConfigModels.Cluster>)config.Clusters).RemoveAll(c => c.Name == context.ContextDetails.Cluster);
            }

            if (!string.IsNullOrWhiteSpace(context.ContextDetails?.User))
            {
                ((List<User>)config.Users).RemoveAll(c => c.Name == context.ContextDetails.User);
            }

            ((List<Context>)config.Contexts).Remove(context);

            var yaml = _yamlSerializer.Serialize(config);
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
        var dir = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(filename))
        {
            return;
        }

        lock (_syncRoot)
        {
            if (_fileWatchers.ContainsKey(path))
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
    }

    private void UnwatchKubeConfig(string path)
    {
        FileSystemWatcher? watcher;
        lock (_syncRoot)
        {
            if (!_fileWatchers.TryGetValue(path, out watcher))
            {
                return;
            }

            _fileWatchers.Remove(path);
        }

        using (watcher)
        {
            watcher.Changed -= Watcher_Changed;
        }
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        try
        {
            OnKubeConfigFileChanged(e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Watching Kube Config {Path}", e.FullPath);
        }
    }

    internal void OnKubeConfigFileChanged(string path)
    {
        lock (_syncRoot)
        {
            // Watcher events can arrive after RemoveKubeConfig stopped watching the file; ignore them.
            if (!_fileWatchers.ContainsKey(path))
            {
                return;
            }
        }

        LoadFromConfigFromPath(path);
    }

    private void StartDiscoveryWatcher(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        lock (_syncRoot)
        {
            if (_discoveryWatcher is not null)
            {
                return;
            }

            _discoveryWatcher = new FileSystemWatcher
            {
                Path = directory,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            _discoveryWatcher.Changed += DiscoveryWatcher_Changed;
            _discoveryWatcher.Created += DiscoveryWatcher_Changed;
            _discoveryWatcher.Renamed += DiscoveryWatcher_Changed;
        }
    }

    private void StopDiscoveryWatcher()
    {
        FileSystemWatcher? watcher;
        lock (_syncRoot)
        {
            watcher = _discoveryWatcher;
            _discoveryWatcher = null;
        }

        if (watcher is null)
        {
            return;
        }

        watcher.Changed -= DiscoveryWatcher_Changed;
        watcher.Created -= DiscoveryWatcher_Changed;
        watcher.Renamed -= DiscoveryWatcher_Changed;
        watcher.Dispose();
    }

    private void DiscoveryWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (!_settings.KubeConfigAutoDiscoveryEnabled || !_discovery.IsKubeConfig(e.FullPath))
            {
                return;
            }

            lock (_syncRoot)
            {
                if (IsPersistedPath(e.FullPath))
                {
                    return;
                }

                _discoveredPaths.Add(e.FullPath);
            }

            LoadFromPath(e.FullPath, persist: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading discovered Kube Config {Path}", e.FullPath);
        }
    }

    private bool ClusterExists(string name, string kubeConfigPath)
    {
        return Clusters.Any(x => x.Name == name && x.KubeConfigPath == kubeConfigPath);
    }

    private static List<T> MergeNamedItems<T>(
        IEnumerable<T>? existing,
        IEnumerable<T>? imported,
        Func<T, string> getName)
    {
        Dictionary<string, T> merged = new(StringComparer.Ordinal);

        if (existing is not null)
        {
            foreach (var item in existing)
            {
                var name = getName(item);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    merged[name] = item;
                }
            }
        }

        if (imported is not null)
        {
            foreach (var item in imported)
            {
                var name = getName(item);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    merged[name] = item;
                }
            }
        }

        return [.. merged.Values];
    }

    public void Dispose()
    {
        StopDiscoveryWatcher();

        foreach (var watcher in _fileWatchers.Values)
        {
            watcher.Dispose();
        }
    }
}
