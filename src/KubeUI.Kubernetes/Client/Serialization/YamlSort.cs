using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace KubeUI.Kubernetes.Serialization;

public class SortedTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector _innerTypeInspector;

    public SortedTypeInspector(ITypeInspector innerTypeInspector)
    {
        _innerTypeInspector = innerTypeInspector;
    }

    public override string GetEnumName(Type enumType, string name)
    {
        return _innerTypeInspector.GetEnumName(enumType, name);
    }

    public override string GetEnumValue(object enumValue)
    {
        return _innerTypeInspector.GetEnumValue(enumValue);
    }

    public override bool HasParseMethod(Type type)
    {
        return _innerTypeInspector.HasParseMethod(type);
    }

    public override object? Parse(string value, Type expectedType)
    {
        return _innerTypeInspector.Parse(value, expectedType);
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return _innerTypeInspector.GetProperties(type, container).OrderBy(x => x.Name);
    }
}
