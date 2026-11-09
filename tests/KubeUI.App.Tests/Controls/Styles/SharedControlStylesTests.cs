using Avalonia.Headless.XUnit;
using KubeUI.Controls.Styles;
using Shouldly;

namespace KubeUI.App.Tests.Controls.Styles;

public sealed class SharedControlStylesTests
{
    [AvaloniaFact]
    public void Shared_styles_can_be_composed_without_application_styles()
    {
        var styles = new SharedControlStyles();

        styles.Count.ShouldBe(3);
        styles.Resources.ShouldBeEmpty();
    }
}
