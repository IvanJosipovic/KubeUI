using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using KubeUI.Features.Resources.Properties;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Properties;

public sealed class ResourcePropertiesSectionTests
{
    [AvaloniaFact]
    public void Create_builds_an_expanded_section_with_the_supplied_header_and_items()
    {
        Control first = new TextBlock { Text = "first" };
        Control second = new TextBlock { Text = "second" };

        var section = ResourcePropertiesSection.Create("Details", first, second);

        section.Header.ShouldBe("Details");
        section.IsExpanded.ShouldBeTrue();
        var content = section.Content.ShouldBeOfType<StackPanel>();
        content.Children.ShouldBe(new Control[] { first, second });
    }
}
