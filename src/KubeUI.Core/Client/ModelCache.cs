using KubeUI.Core.Client;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using ZSpitz.Util;

namespace KubeUI
{
    public static class ModelCache
    {
        public static readonly ConcurrentDictionary<Assembly, XmlDocument> Cache = new();
        public static readonly ConcurrentDictionary<string, Type> TypeCache = new();

        public static void AddToCache(Assembly assembly, XmlDocument xmlDocument)
        {
            if (!Cache.ContainsKey(assembly))
            {
                Cache[assembly] = xmlDocument;

                TypeCache.AddRange(GetTypes(assembly));
            }
        }

        public static Type? GetResourceType(GroupApiVersionKind type)
        {
            return GetResourceType(type.Group, type.ApiVersion, type.Kind);
        }

        public static Type? GetResourceType(string group, string version, string kind)
        {
            if (TypeCache.TryGetValue($"{group}/{version}/{kind}", out var value))
            {
                return value;
            }

            return null;
        }

        public static Dictionary<string, Type> GetTypes(Assembly assembly)
        {
            var dic = new Dictionary<string, Type>();

            foreach (var item in assembly.GetExportedTypes())
            {
                var attributes = item.GetCustomAttributes(typeof(KubernetesEntityAttribute), true);

                if (attributes.Length > 0)
                {
                    var attribute = (KubernetesEntityAttribute)attributes[0];

                    dic[$"{attribute.Group}/{attribute.ApiVersion}/{attribute.Kind}"] = item;
                }
            }

            return dic;
        }
    }
}