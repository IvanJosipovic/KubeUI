using KubernetesCRDModelGen;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KubeUI.Core.Client;

public abstract class ClusterBase : INotifyPropertyChanged
{
    private ILogger<ClusterBase> Logger { get; set; }

    public string Name { get; set; }

    public bool IsConnected { get; set; } = true;

    private ICRDGenerator CRDGenerator { get; set; }

    protected ClusterBase(ILogger<ClusterBase> logger, ICRDGenerator cRDGenerator)
    {
        Logger = logger;
        CRDGenerator = cRDGenerator;
    }

    internal ConcurrentDictionary<string, ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>> Objects { get; set; } = new();

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>> OnChange;

    public event PropertyChangedEventHandler? PropertyChanged;

    private List<string> SelectedNamespaces = new();

    private void NotifyStateChanged(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item) => OnChange?.Invoke(eventType, type, item);

    public void AddObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        Objects[key][$"{@object.Namespace()}|{@object.Name()}"] = @object;

        NotifyStateChanged(WatchEventType.Added, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    public void UpdateObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        Objects[key][$"{@object.Namespace()}|{@object.Name()}"] = @object;

        NotifyStateChanged(WatchEventType.Modified, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    public void DeleteObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        Objects[key].TryRemove($"{@object.Namespace()}|{@object.Name()}", out _);

        NotifyStateChanged(WatchEventType.Deleted, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    public void AddObjects(IEnumerable<IKubernetesObject<V1ObjectMeta>> objects)
    {
        foreach (var @object in objects)
        {
            AddObject(@object);
        }
    }

    public IEnumerable<T> GetObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/').ToLower();

        if (string.IsNullOrEmpty(key))
        {
            return Enumerable.Empty<T>();
        }

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            Seed<T>(version, kind, group);
        }

        if (GetSelectedNamespaces().Any())
        {
            return Objects[key].Values.Cast<T>().Where(x => string.IsNullOrEmpty(x.Namespace()) || GetSelectedNamespaces().Contains(x.Namespace())).ToList();
        }

        return Objects[key].Values.Cast<T>();
    }

    public IEnumerable<T> GetObjects<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        return GetObjects<T>(attribute.ApiVersion, attribute.Kind, attribute.Group);
    }

    public abstract void Seed<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    public void Seed<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        Seed<T>(attribute.ApiVersion, attribute.Kind, attribute.Group);
    }

    public T? GetObject<T>(string version, string kind, string @namespace, string name, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/').ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            Seed<T>(version, kind, group);
        }

        var obj = Objects[key][$"{@namespace}|{name}"];

        return obj as T;
    }

    public T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var attribute = GroupApiVersionKind.From<T>();

        return GetObject<T>(attribute.ApiVersion, attribute.Kind, @namespace, name, attribute.Group);
    }

    public Type? GetResourceType(GroupApiVersionKind type)
    {
        return GetResourceType(type.Group, type.ApiVersion, type.Kind);
    }

    public Type? GetResourceType(string group, string version, string kind)
    {
        if (string.IsNullOrEmpty(group) && version == "v1" && kind == "endpoint")
        {
            return typeof(V1Endpoint);
        }

        foreach (var item in AssemblyLoader.Cache.Keys)
        {
            foreach (var type in item.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(KubernetesEntityAttribute), true);

                if (attributes.Length > 0)
                {
                    var attribute = (KubernetesEntityAttribute)attributes[0];

                    if ((string.IsNullOrEmpty(group) || attribute.Group.Equals(group, StringComparison.InvariantCultureIgnoreCase)) && attribute.ApiVersion.Equals(version, StringComparison.InvariantCultureIgnoreCase) && attribute.Kind.Equals(kind, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return type;
                    }
                }
            }
        }

        return null;
    }

    public async Task GenerateCRDAssembly(V1CustomResourceDefinition crd)
    {
        var assembly = await CRDGenerator.GenerateAssembly(crd, "k8s.Models");

        if (assembly.Item1 != null)
        {
            AssemblyLoader.AddToCache(assembly.Item1, assembly.Item2);
        }
    }

    public IEnumerable<string> GetSelectedNamespaces()
    {
        return SelectedNamespaces;
    }

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
        var types = GenerateTypeMap();

        var serializer = new SerializerBuilder()
            .JsonCompatible()
            .Build();
        var deserializer = new DeserializerBuilder().Build();
        var method = typeof(KubernetesJson).GetMethod(nameof(KubernetesJson.Deserialize), BindingFlags.Static | BindingFlags.Public, new[] { typeof(string) });

        using var reader = new StreamReader(stream);

        var parser = new Parser(reader);

        parser.Consume<StreamStart>();

        while (parser.Accept<DocumentStart>(out var start))
        {
            var doc = deserializer.Deserialize(parser);
            var json = serializer.Serialize(doc);

            var meta = KubernetesJson.Deserialize<KubernetesObject>(json);
            var key = $"{meta.ApiVersion}/{meta.Kind}";

            if (types.ContainsKey(key))
            {
                try
                {
                    var type = types[key];
                    var generic = method.MakeGenericMethod(type);
                    var obj = generic.Invoke(null, new[] { json });

                    AddObject((IKubernetesObject<V1ObjectMeta>)obj);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error converting from Json to {type} {json}", key, json);
                }
            }
            else
            {
                Logger.LogWarning("Missing Type: {type}", key);
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
                await ImportYaml(file.OpenRead());
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
                    await ImportYaml(entry.Open());
                }
            }
        }
    }

    private IDictionary<string, Type> GenerateTypeMap()
    {
        var type = typeof(KubernetesEntityAttribute);

        var assType = AssemblyLoader.Cache.Keys.First().GetTypes().First(x => x.FullName == type.FullName);

        var ModelTypeMap = AssemblyLoader.Cache.Keys
            .SelectMany(x => x.GetTypes())
            .Where(t => t.GetCustomAttribute<KubernetesEntityAttribute>() != null)
            .GroupBy(x => new { x.GetCustomAttribute<KubernetesEntityAttribute>().Group, x.GetCustomAttribute<KubernetesEntityAttribute>().ApiVersion, x.GetCustomAttribute<KubernetesEntityAttribute>().Kind }, (key, g) => g.First())
            .ToDictionary(
                type =>
                {
                    var groupProp = type.GetField(nameof(V1Deployment.KubeGroup));
                    var apiVersionProp = type.GetField(nameof(V1Deployment.KubeApiVersion));
                    var kindProp = type.GetField(nameof(V1Deployment.KubeKind));

                    return $"{groupProp.GetValue(null)}/{apiVersionProp.GetValue(null)}/{kindProp.GetValue(null)}".TrimStart('/');
                },
                t => t);

        return ModelTypeMap;
    }
}
