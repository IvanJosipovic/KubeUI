using KubeUI.Kubernetes.Serialization;
using Moq;
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

    /// <summary>Verifies descriptor sorting requests metadata for each container.</summary>
    [Fact]
    public void GetProperties_SortsDescriptorsForEachContainer()
    {
        var laterProperty = new Mock<IPropertyDescriptor>();
        laterProperty.SetupGet(static property => property.Name).Returns("zeta");
        var earlierProperty = new Mock<IPropertyDescriptor>();
        earlierProperty.SetupGet(static property => property.Name).Returns("alpha");
        var inner = new TestTypeInspector([laterProperty.Object, earlierProperty.Object]);
        var inspector = new SortedTypeInspector(inner);

        var properties = inspector.GetProperties(typeof(string), new object()).ToArray();
        var secondContainerProperties = inspector.GetProperties(typeof(string), new object()).ToArray();

        properties.Select(static property => property.Name).ShouldBe(["alpha", "zeta"]);
        inner.PropertyRequests.ShouldBe(2);
        secondContainerProperties[0].ShouldBeSameAs(earlierProperty.Object);
        secondContainerProperties[1].ShouldBeSameAs(laterProperty.Object);
    }

    private sealed class TestTypeInspector : TypeInspectorSkeleton
    {
        private readonly IPropertyDescriptor[] _properties;

        public TestTypeInspector(IPropertyDescriptor[]? properties = null)
        {
            _properties = properties ?? [];
        }

        public int PropertyRequests { get; private set; }

        public override string GetEnumName(Type enumType, string name) => name;

        public override string GetEnumValue(object enumValue) => enumValue.ToString()!;

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            PropertyRequests++;
            return _properties;
        }

        public override bool HasParseMethod(Type type) => type == typeof(string);

        public override object? Parse(string value, Type expectedType) => value;
    }
}
