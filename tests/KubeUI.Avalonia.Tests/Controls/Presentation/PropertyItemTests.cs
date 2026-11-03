using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Controls.Presentation;

public sealed class PropertyItemTests
{
    [AvaloniaFact]
    public void Constructor_creates_content_and_preserves_property_values()
    {
        var item = new PropertyItem
        {
            Key = "Name",
            Value = "worker",
        };

        item.Content.ShouldBeOfType<Border>();
        item.Key.ShouldBe("Name");
        item.Value.ShouldBe("worker");
    }
}
