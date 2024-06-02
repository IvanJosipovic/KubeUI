using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Scrutor;

namespace KubeUI.Client;

[ServiceDescriptor<ClusterManager>(ServiceLifetime.Singleton)]
public sealed partial class ClusterManager : ObservableObject
{
    public ObservableCollection<Cluster> Clusters { get; set; } = [];

    private readonly IServiceProvider _serviceProvider;

    public ClusterManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        LoadClusters();
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
