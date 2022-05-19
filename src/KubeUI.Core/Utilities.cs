using k8s;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

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

    public static object CreateInstance(Type type)
    {
        try
        {
            var newType = type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                newType = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                newType = typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments()[0], type.GetGenericArguments()[1]);
            }

            if (type == typeof(string))
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
                    //else
                    //{
                    //    for (int i = 0; i < dict.Keys.Count; i++)
                    //    {
                    //        dict.Values.ElementAt(i).GutObject();
                    //    }
                    //}
                }
                else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    var obj = (IList)property.GetValue(t);

                    var count = obj?.Count;
                    if (count == 0)
                    {
                        property.SetValue(t, null);
                    }
                    else
                    {
                        //var type = property.PropertyType.GenericTypeArguments[0];

                        //if (type.FullName.StartsWith("k8s."))
                        //{
                        //    var isnull = true;

                        //    for (int i = 0; i < count; i++)
                        //    {
                        //        var itm = obj[i];

                        //        itm.GutObject();

                        //        if (itm.GetType()
                        //            .GetProperties()
                        //            .All(x => x.GetValue(itm) == null))
                        //        {
                        //            obj.RemoveAt(i);
                        //            i--;
                        //            count--;
                        //        }
                        //        else
                        //        {
                        //            isnull = false;
                        //        }
                        //    }

                        //    if (isnull)
                        //    {
                        //        property.SetValue(t, null);
                        //    }
                        //}
                    }
                }
                else if (property.PropertyType.FullName.StartsWith("k8s."))
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
        return KubernetesJson.Deserialize<T>(KubernetesJson.Serialize(obj));
    }
}
