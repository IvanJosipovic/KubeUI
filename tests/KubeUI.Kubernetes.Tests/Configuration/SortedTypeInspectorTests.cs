using KubeUI.Kubernetes.Serialization;
using Shouldly;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace KubeUI.Kubernetes.Tests.Configuration;

public class SortedTypeInspectorTests
{
    [Fact]
    public void ParseMethodsAreForwardedToInnerInspector()
    {
        var inner = new TestTypeInspector();
        var inspector = new SortedTypeInspector(inner);

        inspector.HasParseMethod(typeof(string)).ShouldBeTrue();
        inspector.Parse("value", typeof(string)).ShouldBe("value");
    }

    private sealed class TestTypeInspector : TypeInspectorSkeleton
    {
        public override string GetEnumName(Type enumType, string name) => name;

        public override string GetEnumValue(object enumValue) => enumValue.ToString()!;

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container) => [];

        public override bool HasParseMethod(Type type) => type == typeof(string);

        public override object? Parse(string value, Type expectedType) => value;
    }
}
