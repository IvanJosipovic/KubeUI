using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using KubeUI.Avalonia.Styles;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class FluentResourcesTests
{
    [AvaloniaFact]
    public void returns_typed_resource_and_rejects_missing_or_incompatible_resources()
    {
        var application = new Application();
        var value = new object();
        application.Resources["existing"] = value;

        FluentResources.GetResource<object>(application, "existing", ThemeVariant.Default).ShouldBeSameAs(value);
        Should.Throw<InvalidOperationException>(() =>
                FluentResources.GetResource<object>(application, "missing", ThemeVariant.Default))
            .Message.ShouldBe("Required Fluent resource 'missing' was not found.");
        Should.Throw<InvalidOperationException>(() =>
                FluentResources.GetResource<string>(application, "existing", ThemeVariant.Default))
            .Message.ShouldBe("Required Fluent resource 'existing' was not found.");
    }
}
