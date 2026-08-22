using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using k8s.Models;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public sealed class YamlEditorIntelliSenseTests
{
    [AvaloniaFact]
    public void DocumentationViewFactory_RendersFieldSummaryOnlyForFieldDocumentation()
    {
        var documentation = new YamlDocumentationInfo(
            "spec",
            nameof(TestDocType),
            "Field summary.");

        var control = YamlDocumentationViewFactory.Create(documentation);

        var panel = control.ShouldBeOfType<StackPanel>();
        panel.Children.Count.ShouldBe(3);

        panel.Children[0].ShouldBeOfType<TextBlock>().Text.ShouldBe("spec");
        panel.Children[1].ShouldBeOfType<TextBlock>().Text.ShouldBe(nameof(TestDocType));
        panel.Children[2].ShouldBeOfType<TextBlock>().Text.ShouldBe("Field summary.");
    }

    [AvaloniaFact]
    public void DocumentationViewFactory_FormatsGenericTypeNamesWithoutAssemblyDetails()
    {
        var documentation = new YamlDocumentationInfo(
            "ownerReferences",
            "System.Collections.Generic.List<k8s.Models.V1OwnerReference>",
            "Owner references.");

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
