using System.Diagnostics;
using System.Text;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using KubeUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace KubeUI.Client;

[ServiceDescriptor<ClusterManager>(ServiceLifetime.Singleton)]
public sealed partial class ClusterManager : ObservableObject
{
    public ObservableCollection<Cluster> Clusters { get; set; } = [];

    private readonly ILogger<ClusterManager> _logger;

    private readonly IServiceProvider _serviceProvider;

    private readonly IFactory _factory;

    public ClusterManager(ILogger<ClusterManager> logger, IServiceProvider serviceProvider, IFactory factory)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _factory = factory;

        KubernetesClientConfiguration.ExecStdError += KubernetesClientConfiguration_ExecStdError;

        LoadClusters();
    }

    private async void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
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

    [RelayCommand]
    private void LoadClusters()
    {
        LoadFromConfigFromPath(KubernetesClientConfiguration.KubeConfigDefaultLocation);
    }

    [RelayCommand]
    private void LoadFromConfigFromPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (File.Exists(path))
        {
            var config = KubernetesClientConfiguration.LoadKubeConfig(path);

            foreach (var item in config.Contexts)
            {
                var cluster = _serviceProvider.GetRequiredService<Cluster>();

                cluster.Name = item.Name;

                cluster.KubeConfigPath = config.FileName;

                Clusters.Add(cluster);
            }
        }
    }

    public void LoadFromConfig(string kubeConfig)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);

        var config = KubernetesClientConfiguration.LoadKubeConfig(new MemoryStream(Encoding.UTF8.GetBytes(kubeConfig)));

        foreach (var item in config.Contexts)
        {
            var cluster = _serviceProvider.GetRequiredService<Cluster>();

            cluster.Name = item.Name;

            cluster.KubeConfigPath = config.FileName;

            cluster.KubeConfig = config;

            Clusters.Add(cluster);
        }
    }

    public Cluster? GetCluster(string name)
    {
        return Clusters.FirstOrDefault(c => c.Name == name);
    }
}
