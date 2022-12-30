using KubernetesCRDModelGen;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Compression;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

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

    protected void NotifyStateChanged(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item) => OnChange?.Invoke(eventType, type, item);

    public void AddInternalObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        Objects[key][$"{@object.Namespace()}|{@object.Name()}"] = @object;

        NotifyStateChanged(WatchEventType.Added, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    public void UpdateInternalObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        Objects[key][$"{@object.Namespace()}|{@object.Name()}"] = @object;

        NotifyStateChanged(WatchEventType.Modified, GroupApiVersionKind.From(@object.GetType()), @object);
    }

    public void DeleteInternalObject(IKubernetesObject<V1ObjectMeta> @object)
    {
        var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        Objects[key].TryRemove($"{@object.Namespace()}|{@object.Name()}", out _);

        NotifyStateChanged(WatchEventType.Deleted, GroupApiVersionKind.From(@object.GetType()), @object);
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

    public long CountObjects(string version, string kind, string group = "")
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/').ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            Seed(version, kind, group);
        }

        if (GetSelectedNamespaces().Any())
        {
            return Objects[key].Values.Where(x => string.IsNullOrEmpty(x.Namespace()) || GetSelectedNamespaces().Contains(x.Namespace())).Count();
        }

        return Objects[key].Count;
    }

    public long CountObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/').ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            Seed<T>(version, kind, group);
        }

        if (GetSelectedNamespaces().Any())
        {
            return Objects[key].Values.Where(x => string.IsNullOrEmpty(x.Namespace()) || GetSelectedNamespaces().Contains(x.Namespace())).Count();
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

    public void Seed(string version, string kind, string group = "")
    {
        var type = GetResourceType(group, version, kind);

        if (type == null)
        {
            return;
        }

        var mi = this.GetType().GetMethods().First(x => x.Name == nameof(Seed) && x.IsGenericMethod && x.GetParameters().Length == 0);
        var fooRef = mi.MakeGenericMethod(type);
        fooRef.Invoke(this, null);
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

        foreach (var item in AssemblyLoader.Cache.Keys.ToList())
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

        var reader = new StreamReader(stream);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<StreamStart>();

        while (parser.Accept<DocumentStart>(out _))
        {
            var doc = Seralization.KubernetesYaml.Deserializer.Deserialize(parser);
            var yaml = Seralization.KubernetesYaml.Serialize(doc);

            var obj = Seralization.KubernetesYaml.Deserialize<KubernetesObject>(yaml);
            try
            {
                var type = types[obj.ApiVersion + "/" + obj.Kind];

                var model = Seralization.KubernetesYaml.Deserializer.Deserialize(yaml, type);

                if (model != null)
                {
                    AddInternalObject((IKubernetesObject<V1ObjectMeta>)model);
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

    private IDictionary<string, Type> GenerateTypeMap()
    {
        var type = typeof(KubernetesEntityAttribute);

        var assType = AssemblyLoader.Cache.Keys.First().GetTypes().First(x => x.FullName == type.FullName);

        var ModelTypeMap = AssemblyLoader.Cache.Keys
            .SelectMany(x => x.GetTypes())
            .Where(t => t.GetCustomAttribute<KubernetesEntityAttribute>() != null)
            .GroupBy(x =>
            {
                var attr = x.GetCustomAttribute<KubernetesEntityAttribute>();
                return new { attr.Group, attr.ApiVersion, attr.Kind };
            }
            ).ToDictionary(x => $"{x.Key.Group}/{x.Key.ApiVersion}/{x.Key.Kind}".TrimStart('/'), y => y.First());

        return ModelTypeMap;
    }
}
