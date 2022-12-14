using System.Collections;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace KubeUI.Core;

public static class Utilities
{
    public static string? GetVersion()
    {
        return Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }

    public static IEnumerable<T> EnsureNotEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable ?? Enumerable.Empty<T>();
    }

    public static string JsonPrettifyString(string json)
    {
        var jDoc = JsonDocument.Parse(json);
        return JsonPrettify(jDoc);
    }

    public static string JsonPrettify(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }

    public static object? CreateInstance(Type type)
    {
        try
        {
            var newType = type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                newType = typeof(List<>).MakeGenericType(type.GenericTypeArguments);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                newType = typeof(Dictionary<,>).MakeGenericType(type.GenericTypeArguments);
            }
            else if (type == typeof(string))
            {
                return string.Empty;
            }

            return Activator.CreateInstance(newType);
        }
        catch (Exception)
        {
            return null;
            //throw;
        }
    }

    public static T CreateInstance<T>()
    {
        return (T)CreateInstance(typeof(T));
    }

    public static T GutObject<T>(this T t)
    {
        if (IsSimpleType(typeof(T)) || t == null)
        {
            return t;
        }

        foreach (var property in t.GetType().GetProperties())
        {
            if (IsSimpleType(property.PropertyType))
            {
                continue;
            }

            if (property.GetValue(t) != null)
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var dict = (IDictionary)property.GetValue(t);

                    if (dict?.Count == 0)
                    {
                        property.SetValue(t, null);
                    }
                    else
                    {
                        foreach (var item in dict.Values)
                        {
                            item.GutObject();
                        }
                    }
                }
                else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    var list = (IList)property.GetValue(t);

                    var count = list?.Count;
                    if (count == 0)
                    {
                        property.SetValue(t, null);
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var itm = list[i];

                            itm.GutObject();
                        }
                    }
                }
                else if (property.PropertyType.FullName.StartsWith("k8s.") || property.PropertyType.FullName.StartsWith("KubernetesCRDModelGen.Models."))
                {
                    var obj = property.GetValue(t)?.GutObject();

                    if (obj.GetType()
                        .GetProperties()
                        .All(x => x.GetValue(obj) == null))
                    {
                        property.SetValue(t, null);
                    }
                }
            }
        }
        return t;
    }

    public static bool IsSimpleType(Type type)
    {
        return
            type.IsPrimitive
            || new Type[] {
                typeof(Enum),
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            }.Contains(type)
            || Convert.GetTypeCode(type) != TypeCode.Object
            || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
    }

    public static T CloneObject<T>(T obj)
    {
        var source = KubernetesJson.Serialize(obj);

        return KubernetesJson.Deserialize<T>(source);
    }

    public static string ToYaml(this IKubernetesObject<V1ObjectMeta> obj)
    {
        return KubeUI.Core.Client.Seralization.KubernetesYaml.Serialize(obj).TrimEnd();
    }

    public static string ToJson(this IKubernetesObject<V1ObjectMeta> obj)
    {
        return JsonPrettifyString(KubernetesJson.Serialize(obj));
    }
}
