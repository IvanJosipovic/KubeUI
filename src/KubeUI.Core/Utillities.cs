using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KubeUI
{
    public static class Utillities
    {
        public static string GetVersion()
        {
            var assembly = Type.GetType("KubeUI.Wasm.Startup, KubeUI.Wasm")?.Assembly ?? Type.GetType("KubeUI.WebWindow.Startup, KubeUI.WebWindow")?.Assembly;

            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IDictionary<T, V> OrEmptyIfNull<T, V>(this IDictionary<T, V> source)
        {
            return source ?? new Dictionary<T, V>();
        }
    }
}
