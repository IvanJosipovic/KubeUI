using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Presentation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Controls.Presentation;

public sealed class ViewFactoryDiscoveryTests
{
    [AvaloniaFact]
    public void Create_MatchesOnlyItsViewModelAndResolvesViewFromProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<TestView>();
        using var provider = services.BuildServiceProvider();

        var factory = ViewFactoryDiscovery.Create<TestView>();

        factory.CanCreate(new TestViewModel()).ShouldBeTrue();
        factory.CanCreate(new OtherViewModel()).ShouldBeFalse();
        Should.Throw<ArgumentNullException>(() => factory.CanCreate(null!));

        factory.Create(provider).ShouldBeOfType<TestView>();
    }

    [AvaloniaFact]
    public void Create_ThrowsWhenViewIsNotRegistered()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        var factory = ViewFactoryDiscovery.Create<TestView>();

        Should.Throw<InvalidOperationException>(() => factory.Create(provider));
    }

    [AvaloniaFact]
    public void AddViewFactories_RegistersDiscoveredFactoriesAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddTransient<TestView>();
        services.AddViewFactories([ViewFactoryDiscovery.Create<TestView>()]);
        using var provider = services.BuildServiceProvider();

        services.Count(descriptor => descriptor.ServiceType == typeof(IViewFactory)).ShouldBe(1);
        services.Where(descriptor => descriptor.ServiceType == typeof(IViewFactory))
            .ShouldAllBe(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton);

        provider.GetRequiredService<IViewFactory>().Create(provider).ShouldBeOfType<TestView>();
    }

    private sealed class TestViewModel;

    private sealed class OtherViewModel;

    private sealed class TestView : KubeUIView<TestViewModel>
    {
        protected override object Build(TestViewModel vm) => new StackPanel();
    }
}
