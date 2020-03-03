using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace KubeUI
{
    public static class Utilities
    {
        public static string GetVersion()
        {
            var assembly = Type.GetType("KubeUI.Wasm.Program, KubeUI.Wasm")?.Assembly ?? Type.GetType("KubeUI.WebWindow.Program, KubeUI")?.Assembly;

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

        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string ToYaml(object item)
        {
            var serializer = new Serializer();
            var jsonString = JsonConvert.SerializeObject(item);
            var expConverter = new ExpandoObjectConverter();
            var expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(jsonString, expConverter);
            return serializer.Serialize(expandoObject);
        }
    }
}
