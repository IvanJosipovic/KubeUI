﻿using k8s.KubeConfigModels;
using KubernetesCRDModelGen;
using KubeUI.Core.Client.Informer;
using System.Collections.Concurrent;
using System.Text.Json;

namespace KubeUI.Core.Client;

public class Cluster : ClusterBase, ICluster
{
    private readonly ILoggerFactory loggerFactory;

    public string KubeConfigPath { get; set; }

    public K8SConfiguration KubeConfig { get; set; }

    public IKubernetes? Client;

    private PodMetricsCache PodMetricsCache { get; set; }

    private KubernetesClientConfiguration GetClientConfiguration()
    {
        if (!string.IsNullOrEmpty(KubeConfigPath))
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
        }
        else
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigObject(KubeConfig, Name);
        }

        throw new Exception("Unable to find KubeConfig");
    }

    private readonly ConcurrentDictionary<string, IResourceInformer> informers = new();

    public Cluster(ILoggerFactory loggerFactory, ICRDGenerator cRDGenerator) : base(loggerFactory.CreateLogger<ClusterBase>(), cRDGenerator)
    {
        this.loggerFactory = loggerFactory;
    }

    private void Init()
    {
        if (Client == null)
        {
            Client = new Kubernetes(GetClientConfiguration());

            OnChange += Cluster_OnChange;
        }
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
                if (item is V1CustomResourceDefinition v1CustomResourceDefinition)
                {
                    Task.Run(() => GenerateCRDAssembly(v1CustomResourceDefinition)).ConfigureAwait(false);
                }
                break;
            case WatchEventType.Modified:
                break;
            case WatchEventType.Deleted:
                break;
            case WatchEventType.Error:
                break;
            case WatchEventType.Bookmark:
                break;
            default:
                break;
        }
    }

    public override void Seed<T>(string version, string kind, string group = "")
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/');

        if (!informers.ContainsKey(key))
        {
            Init();

            var informer = new ResourceInformer<T>(loggerFactory.CreateLogger<ResourceInformer<T>>(), Client);

            if (informers.TryAdd(key, informer))
            {
                informer.Register(new ResourceInformerCallback<T>((x, y) =>
                {
                    switch (x)
                    {
                        case WatchEventType.Added:
                            AddOrUpdateInternalObject(y);
                            break;
                        case WatchEventType.Modified:
                            AddOrUpdateInternalObject(y);
                            break;
                        case WatchEventType.Deleted:
                            DeleteInternalObject(y);
                            break;
                        case WatchEventType.Error:
                            break;
                        case WatchEventType.Bookmark:
                            break;
                        default:
                            break;
                    }
                    //_logger.LogInformation("{type} - {type} - {name}", typeof(V1Pod), x, y.Metadata.Name);
                }));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                informer.RunAsync(new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
    }

    public async Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

        try
        {
            if (string.IsNullOrEmpty(item.Namespace()))
            {
                await client.DeleteAsync<T>(item.Name());
            }
            else
            {
                await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name());
            }
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Failed to delete");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete");
            throw;
        }
    }

    public async Task<V1APIGroupList> GetAPIs()
    {
        Init();

        return await Client.Apis.GetAPIVersionsAsync();
    }

    public async Task<VersionInfo> GetVersion()
    {
        Init();

        return await Client.Version.GetCodeAsync();
    }

    public override async Task AddOrUpdate<T>(T item)
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

        try
        {
            if (string.IsNullOrEmpty(item.Namespace()))
            {
                if (item.Metadata.Uid != null)
                {
                    // update
                    await client.ReplaceAsync<T>(item, item.Name());
                }
                else
                {
                    // add
                    await client.CreateAsync<T>(item);
                }
            }
            else
            {
                if (item.Metadata.Uid != null)
                {
                    // update namespaced
                    await client.ReplaceNamespacedAsync<T>(item, item.Namespace(), item.Name());
                }
                else
                {
                    // add namespaced
                    await client.CreateNamespacedAsync<T>(item, item.Namespace());
                }
            }
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Failed to AddOrUpdate");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to AddOrUpdate");
            throw;
        }
    }

    public PodMetrics? GetPodMetrics(string @namespace, string name)
    {
        if (PodMetricsCache?.PodMetricsList != null)
        {
            return PodMetricsCache.PodMetricsList.Items.FirstOrDefault(x => x.Namespace() == @namespace && x.Name() == name);
        }

        return null;
    }

    public async Task Connect()
    {
        APIGroups = await GetAPIs();
        IsConnected = true;

        if (PodMetrics)
        {
            PodMetricsCache = new PodMetricsCache(this, loggerFactory.CreateLogger<PodMetricsCache>());
        }
    }
}
