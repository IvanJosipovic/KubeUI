using k8s;
using k8s.Models;
using KubeCRDGenerator;
using System.Collections.Concurrent;
using System.Reflection;

namespace KubeUI.Core.Client
{
    public abstract class ClusterBase
    {
        public string Name { get; set; }

        private ICRDGenerator CRDGenerator { get; set; }

        protected ClusterBase(ICRDGenerator cRDGenerator)
        {
            CRDGenerator = cRDGenerator;
        }

        protected ConcurrentDictionary<string, ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>> Objects { get; set; } = new();

        protected List<Assembly> Assemblies = new() { { typeof(V1Namespace).Assembly } };

        public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>> OnChange;

        private List<string> SelectedNamespaces = new();

        private void NotifyStateChanged(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item) => OnChange?.Invoke(eventType, type, item);

        public void AddObject(IKubernetesObject<V1ObjectMeta> @object)
        {
            var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

            if (!Objects.ContainsKey(key))
            {
                Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            }

            Objects[key][$"{@object.Namespace()}-{@object.Name()}"] = @object;

            NotifyStateChanged(WatchEventType.Added, GroupApiVersionKind.From(@object.GetType()), @object);
        }

        public void UpdateObject(IKubernetesObject<V1ObjectMeta> @object)
        {
            var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

            if (!Objects.ContainsKey(key))
            {
                Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            }

            Objects[key][$"{@object.Namespace()}-{@object.Name()}"] = @object;

            NotifyStateChanged(WatchEventType.Modified, GroupApiVersionKind.From(@object.GetType()), @object);
        }

        public void DeleteObject(IKubernetesObject<V1ObjectMeta> @object)
        {
            var key = @object.ApiVersion.ToLower() + "/" + @object.Kind.ToLower();

            if (!Objects.ContainsKey(key))
            {
                Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
            }

            Objects[key].TryRemove($"{@object.Namespace()}-{@object.Name()}", out _);

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

            var obj = Objects[key][$"{@namespace}-{name}"];

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

            foreach (var item in Assemblies)
            {
                foreach (Type type in item.GetTypes())
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

        public void GenerateCRDAssembly(V1CustomResourceDefinition crd)
        {
            var assembly = CRDGenerator.GenerateAssembly(crd);

            if (assembly.Item1 != null)
            {
                Assemblies.Add(assembly.Item1);
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
    }
}
