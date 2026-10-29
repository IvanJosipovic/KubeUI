using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Presentation;

public sealed class ResourceGraphNodeContentTests
{
    [AvaloniaFact]
    public void Create_without_icon_uses_fixed_height_and_centers_labels()
    {
        var kind = new TextBlock();
        var name = new TextBlock();

        var content = ResourceGraphNodeContent.Create(null, kind, name);

        content.Width.ShouldBe(128);
        content.Height.ShouldBe(128);
        content.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
        content.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
        content.Children.Count.ShouldBe(2);
        content.Children.ShouldContain(kind);
        content.Children.ShouldContain(name);
        kind.TextAlignment.ShouldBe(TextAlignment.Center);
        kind.TextWrapping.ShouldBe(TextWrapping.Wrap);
        name.TextAlignment.ShouldBe(TextAlignment.Center);
        name.TextWrapping.ShouldBe(TextWrapping.Wrap);
    }

    [AvaloniaFact]
    public void Create_with_icon_sizes_it_to_64px_and_can_omit_fixed_height()
    {
        var icon = new TextBlock();
        var kind = new TextBlock();
        var name = new TextBlock();

        var content = ResourceGraphNodeContent.Create(icon, kind, name, fixedHeight: false);

        double.IsNaN(content.Height).ShouldBeTrue();
        content.Children.Count.ShouldBe(3);
        content.Children[0].ShouldBeSameAs(icon);
        icon.Width.ShouldBe(64);
        icon.Height.ShouldBe(64);
        icon.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
        content.Children.ShouldContain(kind);
        content.Children.ShouldContain(name);
    }
}
