using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI
{
    public static class Utillities
    {
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
