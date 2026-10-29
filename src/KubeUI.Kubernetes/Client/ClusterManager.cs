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
    private int _discoveryGeneration;
    private Task? _discoveryTask;
    private CancellationTokenSource _discoveryCancellation = new();
    private bool _disposed;
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

        int generation;
        CancellationToken discoveryToken;
        lock (_syncRoot)
        {
            if (!_settings.KubeConfigAutoDiscoveryEnabled || _disposed)
            {
                return;
            }
            generation = _discoveryGeneration;
            discoveryToken = _discoveryCancellation.Token;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, discoveryToken);
        try
        {
            await DiscoverKubeConfigsAsync(generation, linkedCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
        }
    }

    private async Task DiscoverKubeConfigsAsync(int generation, CancellationToken cancellationToken)
    {
        var directory = _kubeConfigPathProvider.DiscoveryDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        StartDiscoveryWatcher(directory, generation);

        var files = await Task.Run(() => _discovery.DiscoverFiles(directory), cancellationToken)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var file in files)
        {
            if (!TryTrackDiscoveredPath(file, generation))
            {
                continue;
            }

            try
            {
                await LoadFromConfigFromPathAsync(file, persist: false, cancellationToken, generation).ConfigureAwait(false);
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

    private async Task LoadFromConfigFromPathAsync(string path, bool persist, CancellationToken cancellationToken, int? discoveryGeneration = null)
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
                AddClustersFromConfig(config, discoveryGeneration);
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

    private void LoadFromPath(string path, bool persist, int? discoveryGeneration = null)
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
                AddClustersFromConfig(config, discoveryGeneration);
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

    private bool TryTrackDiscoveredPath(string path, int? generation = null)
    {
        lock (_syncRoot)
        {
            if (!_settings.KubeConfigAutoDiscoveryEnabled || _disposed
                || (generation.HasValue && generation.Value != _discoveryGeneration)
                || IsPersistedPath(path))
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
        int generation;
        CancellationToken discoveryToken;
        string[] discovered;
        lock (_syncRoot)
        {
            if (_disposed || _settings.KubeConfigAutoDiscoveryEnabled == enabled)
            {
                return;
            }

            _settings.KubeConfigAutoDiscoveryEnabled = enabled;
            generation = ++_discoveryGeneration;
            _discoveryCancellation.Cancel();
            _discoveryCancellation.Dispose();
            _discoveryCancellation = new CancellationTokenSource();
            discoveryToken = _discoveryCancellation.Token;
            discovered = enabled ? [] : [.. _discoveredPaths];
            if (!enabled)
            {
                _discoveredPaths.Clear();
            }
        }

        OnPropertyChanged(nameof(KubeConfigAutoDiscoveryEnabled));
        if (enabled)
        {
            lock (_syncRoot)
            {
                if (generation == _discoveryGeneration && !_disposed)
                {
                    _discoveryTask = RunDiscoveryAsync(generation, discoveryToken);
                }
            }
            return;
        }

        StopDiscoveryWatcher();
        foreach (var path in discovered)
        {
            RemoveClustersForPath(path);
        }
    }

    internal Task? DiscoveryTask
    {
        get
        {
            lock (_syncRoot)
            {
                return _discoveryTask;
            }
        }
    }

    private async Task RunDiscoveryAsync(int generation, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(() => DiscoverKubeConfigsAsync(generation, cancellationToken), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering kubeconfigs");
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
            && !Path.GetFileName(path).StartsWith('.')
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

    private void AddClustersFromConfig(K8SConfiguration kubeConfig, int? discoveryGeneration = null)
    {
        foreach (var item in kubeConfig.Contexts)
        {
            var cluster = _serviceProvider.GetRequiredService<IClusterRuntime>();

            cluster.Name = item.Name;
            cluster.KubeConfig = kubeConfig;
            cluster.KubeConfigPath = kubeConfig.FileName;
            AddCluster(cluster, discoveryGeneration);
        }
    }

    public void AddCluster(IClusterRuntime cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        AddCluster(cluster, null);
    }

    private void AddCluster(IClusterRuntime cluster, int? discoveryGeneration)
    {
        _dispatcher.Invoke(() =>
        {
            if (discoveryGeneration.HasValue)
            {
                lock (_syncRoot)
                {
                    if (_disposed || !_settings.KubeConfigAutoDiscoveryEnabled
                        || discoveryGeneration.Value != _discoveryGeneration
                        || IsPersistedPath(cluster.KubeConfigPath)
                        || !_discoveredPaths.Contains(cluster.KubeConfigPath)
                        || !File.Exists(cluster.KubeConfigPath)
                        || Clusters.Any(existing => existing.Name == cluster.Name))
                    {
                        return;
                    }
                    Clusters.Add(cluster);
                    return;
                }
            }

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

    private void StartDiscoveryWatcher(string directory, int generation)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        lock (_syncRoot)
        {
            if (_discoveryWatcher is not null || _disposed || !_settings.KubeConfigAutoDiscoveryEnabled
                || generation != _discoveryGeneration)
            {
                return;
            }

            try
            {
                _discoveryWatcher = new FileSystemWatcher();
                _discoveryWatcher.Path = directory;
                _discoveryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                _discoveryWatcher.IncludeSubdirectories = false;
                _discoveryWatcher.Changed += DiscoveryWatcher_Changed;
                _discoveryWatcher.Created += DiscoveryWatcher_Changed;
                _discoveryWatcher.Deleted += DiscoveryWatcher_Deleted;
                _discoveryWatcher.Renamed += DiscoveryWatcher_Changed;
                _discoveryWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _discoveryWatcher?.Dispose();
                _discoveryWatcher = null;
                _logger.LogWarning(ex, "Unable to watch kubeconfig directory {Directory}", directory);
            }
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
        watcher.Deleted -= DiscoveryWatcher_Deleted;
        watcher.Renamed -= DiscoveryWatcher_Changed;
        watcher.Dispose();
    }

    private void DiscoveryWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        int generation;
        lock (_syncRoot)
        {
            if (_disposed || !_settings.KubeConfigAutoDiscoveryEnabled || IsPersistedPath(e.FullPath))
            {
                return;
            }
            generation = _discoveryGeneration;
        }

        _dispatcher.Invoke(() =>
        {
            lock (_syncRoot)
            {
                if (_disposed || !_settings.KubeConfigAutoDiscoveryEnabled
                    || generation != _discoveryGeneration || IsPersistedPath(e.FullPath)
                    || File.Exists(e.FullPath) || !_discoveredPaths.Remove(e.FullPath))
                {
                    return;
                }
            }

            foreach (var cluster in Clusters.Where(x => string.Equals(x.KubeConfigPath, e.FullPath, StringComparison.Ordinal)).ToArray())
            {
                Clusters.Remove(cluster);
            }
        });
    }

    private void DiscoveryWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        try
        {
            int generation;
            lock (_syncRoot)
            {
                if (_disposed || !_settings.KubeConfigAutoDiscoveryEnabled)
                {
                    return;
                }
                generation = _discoveryGeneration;
            }

            if (Path.GetFileName(e.FullPath).StartsWith('.') || !_discovery.IsKubeConfig(e.FullPath)
                || !TryTrackDiscoveredPath(e.FullPath, generation))
            {
                return;
            }

            LoadFromPath(e.FullPath, persist: false, generation);
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
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _discoveryGeneration++;
            _discoveryCancellation.Cancel();
            _discoveryCancellation.Dispose();
        }
        StopDiscoveryWatcher();

        foreach (var watcher in _fileWatchers.Values)
        {
            watcher.Dispose();
        }
    }
}
