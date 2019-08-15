using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace KubeUI
{
    public static class TypeDescriptorProviderGenerator
    {
        public static void AddTypeDescriptorProviders(string nameSpace, string metadataNameSpace)
        {
            IEnumerable<Type> metadataTypes = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == metadataNameSpace);

            foreach (Type metadataType in metadataTypes)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == nameSpace && t.Name == metadataType.Name)
                       .First();

                TypeDescriptor.AddProvider(new AssociatedMetadataTypeTypeDescriptionProvider(type, metadataType), type);
            }
        }
    }
}
