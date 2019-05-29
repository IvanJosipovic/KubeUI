using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace KubeUI.Core
{
    /// <summary>
    /// Utility class to provide documentation for various types where available with the assembly
    /// </summary>
    public static class DocumenationExtensions
    {
        /// <summary>
        /// Provides the documentation comments for a specific method
        /// </summary>
        /// <param name="methodInfo">The MethodInfo (reflection data ) of the member to find documentation for</param>
        /// <returns>The XML fragment describing the method</returns>
        public static XmlElement GetDocumentation(this MethodInfo methodInfo)
        {
            // Calculate the parameter string as this is in the member name in the XML
            var parametersString = "";
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parametersString.Length > 0)
                {
                    parametersString += ",";
                }

                parametersString += parameterInfo.ParameterType.FullName;
            }

            //AL: 15.04.2008 ==> BUG-FIX remove “()” if parametersString is empty
            if (parametersString.Length > 0)
                return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name + "(" + parametersString + ")");
            else
                return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name);
        }

        /// <summary>
        /// Provides the documentation comments for a specific member
        /// </summary>
        /// <param name="memberInfo">The MemberInfo (reflection data) or the member to find documentation for</param>
        /// <returns>The XML fragment describing the member</returns>
        public static XmlElement GetDocumentation(this MemberInfo memberInfo)
        {
            // First character [0] of member type is prefix character in the name in the XML
            return XmlFromName(memberInfo.DeclaringType, memberInfo.MemberType.ToString()[0], memberInfo.Name);
        }

        /// <summary>
        /// Returns the Xml documenation summary comment for this member
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetSummary(this MemberInfo memberInfo)
        {
            var element = memberInfo.GetDocumentation();
            var summaryElm = element?.SelectSingleNode("summary");
            if (summaryElm == null) return "";
            return summaryElm.InnerText.Trim();
        }

        /// <summary>
        /// Provides the documentation comments for a specific type
        /// </summary>
        /// <param name="type">Type to find the documentation for</param>
        /// <returns>The XML fragment that describes the type</returns>
        public static XmlElement GetDocumentation(this Type type)
        {
            // Prefix in type names is T
            return XmlFromName(type, 'T', "");
        }

        /// <summary>
        /// Gets the summary portion of a type's documenation or returns an empty string if not available
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSummary(this Type type)
        {
            var element = type.GetDocumentation();
            var summaryElm = element?.SelectSingleNode("summary");
            if (summaryElm == null) return "";
            return summaryElm.InnerText.Trim();
        }

        /// <summary>
        /// Obtains the XML Element that describes a reflection element by searching the 
        /// members for a member that has a name that describes the element.
        /// </summary>
        /// <param name="type">The type or parent type, used to fetch the assembly</param>
        /// <param name="prefix">The prefix as seen in the name attribute in the documentation XML</param>
        /// <param name="name">Where relevant, the full name qualifier for the element</param>
        /// <returns>The member that has a name that describes the specified reflection element</returns>
        private static XmlElement XmlFromName(this Type type, char prefix, string name)
        {
            string fullName;

            if (string.IsNullOrEmpty(name))
                fullName = prefix + ":" + type.FullName;
            else
                fullName = prefix + ":" + type.FullName + "." + name;

            var xmlDocument = XmlFromAssembly(type.Assembly);

            var matchedElement = xmlDocument["doc"]["members"].SelectSingleNode("member[@name='" + fullName + "']") as XmlElement;

            return matchedElement;
        }

        /// <summary>
        /// A cache used to remember Xml documentation for assemblies
        /// </summary>
        private static readonly Dictionary<Assembly, XmlDocument> Cache = new Dictionary<Assembly, XmlDocument>();

        /// <summary>
        /// A cache used to store failure exceptions for assembly lookups
        /// </summary>
        private static readonly Dictionary<Assembly, Exception> FailCache = new Dictionary<Assembly, Exception>();

        /// <summary>
        /// Obtains the documentation file for the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly to find the XML document for</param>
        /// <returns>The XML document</returns>
        /// <remarks>This version uses a cache to preserve the assemblies, so that 
        /// the XML file is not loaded and parsed on every single lookup</remarks>
        public static XmlDocument XmlFromAssembly(this Assembly assembly)
        {
            if (FailCache.ContainsKey(assembly))
            {
                throw FailCache[assembly];
            }

            try
            {
                if (!Cache.ContainsKey(assembly))
                {
                    // load the document into the cache
                    Cache[assembly] = XmlFromAssemblyNonCached(assembly);
                }

                return Cache[assembly];
            }
            catch (Exception exception)
            {
                FailCache[assembly] = exception;
                throw;
            }
        }

        /// <summary>
        /// Loads and parses the documentation file for the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly to find the XML document for</param>
        /// <returns>The XML document</returns>
        private static XmlDocument XmlFromAssemblyNonCached(Assembly assembly)
        {
            var assemblyName = assembly.ManifestModule.Name.Replace(".dll", "");
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    assembly.GetManifestResourceStream($"{assemblyName}.{assemblyName}.xml").CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(ms);
                    return xmlDocument;
                }
            }
            catch (NullReferenceException exception)
            {
                throw new Exception("XML documentation no found", exception);
            }
        }
    }

    public static class StringExtension
    {
        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1]))
                       || (!preserveAcronyms && char.IsUpper(text[i - 1])
                        && i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }

                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string TrimVersionNumbers(this string text)
        {
            return text.TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');
        }
    }

    public static class DictionaryExtensions
    {
        /// <summary>
        /// Renames a key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public static bool ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict,
                                       TKey oldKey, TKey newKey)
        {
            if (!dict.TryGetValue(oldKey, out TValue value))
                return false;

            dict.Remove(oldKey);  // do not change order
            dict[newKey] = value;  // or dict.Add(newKey, value) depending on ur comfort
            return true;
        }
    }

    public static class ObjectExtensiion
    {
        public static T GutObject<T>(this T t)
        {
            if (IsSimpleType(typeof(T)))
            {
                return t;
            }

            foreach (var property in t.GetType().GetProperties())
            {
                if (property.GetValue(t) != null)
                {
                    if (property.PropertyType.GetInterface(nameof(IDictionary<string, string>)) != null)
                    {
                        var dict = property.GetValue(t) as IDictionary<string, string>;
                        if (dict.Count == 0)
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
                    else if (property.PropertyType.GetInterface(nameof(IList)) != null)
                    {
                        var obj = property.GetValue(t);

                        var count = (int)obj.GetType().GetProperty("Count").GetValue(obj);
                        if (count == 0)
                        {
                            property.SetValue(t, null);
                        }
                        else
                        {
                            var type = property.PropertyType.GenericTypeArguments[0];
                            if (type.FullName.StartsWith("KubeUI."))
                            {
                                bool isnull = true;
                                for (int i = 0; i < count; i++)
                                {
                                    object[] index = { i };
                                    var itm = property.PropertyType.GetProperty("Item").GetValue(obj, index);

                                    itm.GutObject();

                                    if (itm.GetType()
                                        .GetProperties()
                                        .All(x => x.GetValue(itm) == null))
                                    {
                                        property.PropertyType.GetMethod("RemoveAt").Invoke(obj, index);
                                        i--;
                                        count--;
                                    }
                                    else
                                    {
                                        isnull = false;
                                    }
                                }

                                if (isnull)
                                {
                                    property.SetValue(t, null);
                                }
                            }
                        }
                    }
                    else if(property.PropertyType.FullName.StartsWith("KubeUI."))
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
    }
}
