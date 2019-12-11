using KubeUI.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KubeUI.Core.Components
{
    public static class Common
    {
        public static bool ShouldShow(AttributeCollection attributes, KubeUI.UILevel uiLevel)
        {
            if (attributes != null)
            {
                var attr = attributes.OfType<UILevelAttribute>().FirstOrDefault();

                if (attr != null)
                {
                    return (int)uiLevel >= (int)attr.UILevel;
                }
            }

            return true;
        }

        public static bool IsIgnore(AttributeCollection attributes)
        {
            if (attributes != null)
            {
                return attributes.OfType<IgnoreAttribute>().Any();
            }
            return false;
        }

        public static bool IsDisplayInTree(AttributeCollection attributes)
        {
            if (attributes != null)
            {
                return attributes.OfType<DisplayInTreeAttribute>().Any();
            }
            return false;
        }

        public static string GetDisplayInTreeName(AttributeCollection attributes)
        {
            var result = attributes.OfType<DisplayInTreeAttribute>().FirstOrDefault();

            if (result == null)
            {
                throw new Exception("GetDisplayInTreeName Attribute is missing");
            }

            return result.DisplayName;
        }

        public static bool IsSelectList(AttributeCollection attributes)
        {
            return attributes.OfType<SelectListAttribute>().Any();
        }

        public static string[] GetSelectListOptions(AttributeCollection attributes)
        {
            return attributes.OfType<SelectListAttribute>().First().Options;
        }

        public static bool IsReadOnly(AttributeCollection attributes)
        {
            return attributes.OfType<ReadOnlyAttribute>().Any();
        }

        public static bool ShouldHideLink(Type type)
        {
            return !type.GetProperties().Any(Show);

            bool Show(PropertyInfo property)
            {
                var attritbutes = TypeDescriptor.GetProperties(property.PropertyType)[property.Name]?.Attributes;

                return !property.PropertyType.FullName.StartsWith("k8s.")
                && !IsDisplayInTree(attritbutes)
                && !IsIgnore(attritbutes);
            };
        }
    }
}
