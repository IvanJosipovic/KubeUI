using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public sealed class YamlEditorIntelliSenseTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void DocumentationViewFactory_RendersFieldSummaryOnlyForFieldDocumentation()
    {
        var documentation = new YamlDocumentationInfo(
            "spec",
            typeof(TestDocType),
            "Field summary.",
            "Type summary.",
            "unused");

        var control = YamlDocumentationViewFactory.Create(documentation);

        var panel = control.ShouldBeOfType<StackPanel>();
        panel.Children.Count.ShouldBe(3);

        panel.Children[0].ShouldBeOfType<TextBlock>().Text.ShouldBe("spec");
        panel.Children[1].ShouldBeOfType<TextBlock>().Text.ShouldContain(typeof(TestDocType).FullName);
        panel.Children[2].ShouldBeOfType<TextBlock>().Text.ShouldBe("Field summary.");
    }

    [AvaloniaFact]
    public void DocumentationViewFactory_UsesTypeSummaryWhenFieldSummaryIsMissing()
    {
        var documentation = new YamlDocumentationInfo(
            "metadata",
            typeof(TestDocType),
            string.Empty,
            "Type summary only.",
            "unused");

        var control = YamlDocumentationViewFactory.Create(documentation);

        var panel = control.ShouldBeOfType<StackPanel>();
        panel.Children.Count.ShouldBe(3);
        panel.Children[2].ShouldBeOfType<TextBlock>().Text.ShouldBe("Type summary only.");
    }

    [AvaloniaFact]
    public void DocumentationViewFactory_FormatsGenericTypeNamesWithoutAssemblyDetails()
    {
        var documentation = new YamlDocumentationInfo(
            "ownerReferences",
            typeof(List<V1OwnerReference>),
            "Owner references.",
            string.Empty,
            "unused");

        var control = YamlDocumentationViewFactory.Create(documentation);

        var panel = control.ShouldBeOfType<StackPanel>();
        var typeText = panel.Children[1].ShouldBeOfType<TextBlock>().Text;
        typeText.ShouldBe("System.Collections.Generic.List<k8s.Models.V1OwnerReference>");
        typeText.ShouldNotContain("Version=");
        typeText.ShouldNotContain("Culture=");
        typeText.ShouldNotContain("PublicKeyToken=");
    }

    private sealed class TestDocType;
}
