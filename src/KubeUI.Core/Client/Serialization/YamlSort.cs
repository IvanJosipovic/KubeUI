using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization;

namespace KubeUI.Core.Client.Serialization
{
    public class SortedTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeInspector;

        public SortedTypeInspector(ITypeInspector innerTypeInspector)
        {
            _innerTypeInspector = innerTypeInspector;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return _innerTypeInspector.GetProperties(type, container).OrderBy(x => x.Name);
        }
    }
}
