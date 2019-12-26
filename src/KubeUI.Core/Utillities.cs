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

        public static string ToPrettyDate(this DateTime value)
        {
            TimeSpan timesince = DateTime.UtcNow - value;

            if (timesince.Days > 365)
            {
                int years = (timesince.Days / 365);
                if (timesince.Days % 365 != 0)
                    years += 1;
                return string.Format("{0} {1}", years, years == 1 ? "year" : "years");
            }
            else if (timesince.Days > 30)
            {
                int months = (timesince.Days / 30);
                if (timesince.Days % 31 != 0)
                    months += 1;
                return string.Format("{0} {1}", months, months == 1 ? "month" : "months");
            }
            else if (timesince.Days > 0)
                return string.Format("{0} {1}", timesince.Days, timesince.Days == 1 ? "day" : "days");
            else if (timesince.Hours > 0)
                return string.Format("{0} {1}", timesince.Hours, timesince.Hours == 1 ? "hour" : "hours");
            else if (timesince.Minutes > 0)
                return string.Format("{0} {1}", timesince.Minutes, timesince.Minutes == 1 ? "minute" : "minutes");
            else if (timesince.Seconds > 5)
                return string.Format("{0} seconds", timesince.Seconds);
            else
                return "now";
        }
    }
}
