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

    [AvaloniaFact]
    public void Create_binds_observable_header_instead_of_showing_binding_adaptor_type()
    {
        var section = ResourcePropertiesSection.Create(new ImmediateObservable("Metadata"));

        section.Header.ShouldBe("Metadata");
    }

    private sealed class ImmediateObservable(string? value) : IObservable<string?>
    {
        public IDisposable Subscribe(IObserver<string?> observer)
        {
            observer.OnNext(value);
            return EmptyDisposable.Instance;
        }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
