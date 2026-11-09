using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace KubeUI.Kubernetes.Serialization;

/// <summary>Orders serialized properties by name while preserving instance-specific metadata.</summary>
public class SortedTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector _innerTypeInspector;

    /// <summary>Creates an inspector that sorts properties returned by an inner inspector.</summary>
    /// <param name="innerTypeInspector">Inspector that supplies metadata for the current container.</param>
    public SortedTypeInspector(ITypeInspector innerTypeInspector)
    {
        _innerTypeInspector = innerTypeInspector;
    }

    /// <inheritdoc />
    public override string GetEnumName(Type enumType, string name)
    {
        return _innerTypeInspector.GetEnumName(enumType, name);
    }

    /// <inheritdoc />
    public override string GetEnumValue(object enumValue)
    {
        return _innerTypeInspector.GetEnumValue(enumValue);
    }

    /// <inheritdoc />
    public override bool HasParseMethod(Type type)
    {
        return _innerTypeInspector.HasParseMethod(type);
    }

    /// <inheritdoc />
    public override object? Parse(string value, Type expectedType)
    {
        return _innerTypeInspector.Parse(value, expectedType);
    }

    /// <inheritdoc />
    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return _innerTypeInspector.GetProperties(type, container)
            .OrderBy(static property => property.Name);
    }
}
