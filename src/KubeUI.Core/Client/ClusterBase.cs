using KubernetesCRDModelGen;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Compression;
using System.Reflection;
using YamlDotNet.Core;

namespace KubeUI.Core.Client;

public abstract class ClusterBase : INotifyPropertyChanged
{
    private ILogger<ClusterBase> Logger { get; set; }

    public string Name { get; set; }

    public bool IsConnected { get; set; }

    private ICRDGenerator CRDGenerator { get; set; }

    protected ClusterBase(ILogger<ClusterBase> logger, ICRDGenerator cRDGenerator)
    {
        Logger = logger;
        CRDGenerator = cRDGenerator;

        SeedMethodInfo = GetType().GetMethods().First(x => x.Name == nameof(Seed) && x.IsGenericMethod && x.GetParameters().Length == 0);
    }

    public ConcurrentDictionary<string, ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>> Objects { get; set; } = new();

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>> OnChange;

    public event PropertyChangedEventHandler? PropertyChanged;

    private List<string> SelectedNamespaces = new();

    protected void NotifyStateChanged(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item) => OnChange?.Invoke(eventType, type, item);

    protected void AddInternalObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion + "/" + @object.Kind;

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
        }

        Objects[key][$"{@object.Namespace()}|{@object.Name()}"] = @object;

        NotifyStateChanged(WatchEventType.Added, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    protected void UpdateInternalObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion + "/" + @object.Kind;

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
        }

        Objects[key][$"{@object.Namespace()}|{@object.Name()}"] = @object;

        NotifyStateChanged(WatchEventType.Modified, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    protected void DeleteInternalObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion + "/" + @object.Kind;

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
        }

        Objects[key].TryRemove($"{@object.Namespace()}|{@object.Name()}", out _);

        NotifyStateChanged(WatchEventType.Deleted, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    public IEnumerable<T> GetObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/');

        if (string.IsNullOrEmpty(key))
        {
            return Enumerable.Empty<T>();
        }

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
            Seed<T>(version, kind, group);
        }

        if (GetSelectedNamespaces().Any())
        {
            return Objects[key].Values.Where(x => string.IsNullOrEmpty(x.Namespace()) || GetSelectedNamespaces().Contains(x.Namespace())).Cast<T>();
        }

        return Objects[key].Values.Cast<T>();
    }

    public IEnumerable<T> GetObjects<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        return GetObjects<T>(attribute.ApiVersion, attribute.Kind, attribute.Group);
    }

    public long CountObjects(string version, string kind, string group = "")
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/');

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
            Seed(version, kind, group);
        }

        if (GetSelectedNamespaces().Any())
        {
            return Objects[key].Values.Count(x => string.IsNullOrEmpty(x.Namespace()) || GetSelectedNamespaces().Contains(x.Namespace()));
        }

        return Objects[key].Count;
    }

    public long CountObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/');

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
            Seed<T>(version, kind, group);
        }

        if (GetSelectedNamespaces().Any())
        {
            return Objects[key].Values.Count(x => string.IsNullOrEmpty(x.Namespace()) || GetSelectedNamespaces().Contains(x.Namespace()));
        }

        return Objects[key].Count;
    }

    public long CountObjects<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        return CountObjects<T>(attribute.ApiVersion, attribute.Kind, attribute.Group);
    }

    public abstract void Seed<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    public abstract Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    public void Seed<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        Seed<T>(attribute.ApiVersion, attribute.Kind, attribute.Group);
    }

    protected MethodInfo SeedMethodInfo;

    public void Seed(string version, string kind, string group = "")
    {
        var type = ModelCache.GetResourceType(group, version, kind);

        if (type == null)
        {
            return;
        }

        Seed(type);
    }

    public void Seed(Assembly assembly)
    {
        foreach (var type in ModelCache.GetTypes(assembly))
        {
            Seed(type.Value);
        }
    }

    public void Seed(Type type)
    {
        var fooRef = SeedMethodInfo.MakeGenericMethod(type);
        fooRef.Invoke(this, null);
    }

    public T? GetObject<T>(string version, string kind, string @namespace, string name, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/');

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new();
            Seed<T>(version, kind, group);
        }

        if (Objects[key].TryGetValue($"{@namespace}|{name}", out var value))
        {
            return value as T;
        }

        return null;
    }

    public T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        return GetObject<T>(attribute.ApiVersion, attribute.Kind, @namespace, name, attribute.Group);
    }

    public async Task<Assembly?> GenerateCRDAssembly(V1CustomResourceDefinition crd)
    {
        if (crd.Spec.Group.EndsWith("fluxcd.io"))
        {
            return null;
        }

        var assembly = await CRDGenerator.GenerateAssembly(crd, "KubernetesCRDModelGen.Models." + crd.Spec.Group);

        if (assembly.Item1 != null && assembly.Item2 != null)
        {
            ModelCache.AddToCache(assembly.Item1, assembly.Item2);
        }

        return assembly.Item1;
    }

    public IEnumerable<string> GetSelectedNamespaces() => SelectedNamespaces;

    public void SetSelectedNamespaces(IEnumerable<string> namespaces)
    {
        if (namespaces == null)
        {
            SelectedNamespaces.Clear();
        }
        else
        {
            SelectedNamespaces = namespaces.ToList();
        }
    }

    public async Task ImportYaml(Stream stream)
    {
        var mi = GetType().GetMethods().First(x => x.Name == nameof(AddOrUpdate) && x.IsGenericMethod && x.GetParameters().Length == 1);

        var reader = new StreamReader(stream);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<YamlDotNet.Core.Events.StreamStart>();

        while (parser.Accept<YamlDotNet.Core.Events.DocumentStart>(out _))
        {
            var doc = Serialization.KubernetesYaml.Deserializer.Deserialize(parser);
            var yaml = Serialization.KubernetesYaml.Serialize(doc);

            var obj = Serialization.KubernetesYaml.Deserialize<KubernetesObject>(yaml);
            try
            {
                var type = ModelCache.GetResourceType(obj.ApiGroup(), obj.ApiGroupVersion(), obj.Kind);

                var model = Serialization.KubernetesYaml.Deserializer.Deserialize(yaml, type);

                if (model != null)
                {
                    var fooRef = mi.MakeGenericMethod(type);
                    fooRef.Invoke(this, new[] { model });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error Deserializing {kind}", obj.ApiVersion + "/" + obj.Kind);
            }
        }
    }

    public async Task ImportFolder(string path)
    {
        if (Directory.Exists(path))
        {
            var files = new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(fi => fi.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) || fi.Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    await ImportYaml(file.OpenRead());
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error parsing Yaml {filename}", file.FullName);
                }
            }
        }
    }

    public async Task ImportZip(Stream stream)
    {
        using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        await ImportYaml(entry.Open());
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error parsing Yaml {filename}", entry.FullName);
                    }
                }
            }
        }
    }
}
