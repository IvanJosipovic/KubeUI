using Dock.Model.Core;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Desktop;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Diagnostics.Metrics;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Shell.Documents.Updates;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Kubernetes.Client;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Presentation;

public sealed class ViewLocatorTests
{
    [AvaloniaFact]
    public void Build_ResolvesClusterSettingsViewFromGeneratedFactory()
    {
        var services = Application.Current.GetTestServices();
        var locator = services.GetRequiredService<ViewLocator>();
        var viewModel = services.GetRequiredService<ClusterSettingsViewModel>();

        var view = locator.Build(viewModel);

        view.ShouldBeOfType<ClusterSettingsView>();
    }

    [AvaloniaFact]
    public void Build_ResolvesHomeViewFromGeneratedViewFactory()
    {
        var services = Application.Current.GetTestServices();
        var locator = services.GetRequiredService<ViewLocator>();

        var view = locator.Build(new HomeViewModel());

        view.ShouldBeOfType<HomeView>();
    }

    [AvaloniaFact]
    public void Build_ResolvesUpdateAvailableViewFromGeneratedViewFactory()
    {
        var services = Application.Current.GetTestServices();
        var locator = services.GetRequiredService<ViewLocator>();
        var viewModel = services.GetRequiredService<UpdateAvailableViewModel>();

        var view = locator.Build(viewModel);

        view.ShouldBeOfType<UpdateAvailableView>();
    }

    [AvaloniaFact]
    public void Build_ResolvesCrossplaneDiffViewFromGeneratedViewFactory()
    {
        var services = Application.Current.GetTestServices();
        var locator = services.GetRequiredService<ViewLocator>();
        using var viewModel = services.GetRequiredService<MRDiffDetectionViewModel>();

        var view = locator.Build(viewModel);

        view.ShouldBeOfType<MRDiffDetectionView>();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Build_ResolvesServiceMapViewFromGeneratedFactory(bool disableReflection)
    {
        using var host = CreateHubbleHost(disableReflection);
        await using var scope = host.Services.CreateAsyncScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<ServiceMapViewModel>();

        var view = scope.ServiceProvider.GetRequiredService<ViewLocator>().Build(viewModel).ShouldBeOfType<ServiceMapView>();
        view.DataContext = viewModel;
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        view.GetVisualDescendants().OfType<ServiceMapGraphControl>().Count().ShouldBe(1);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Build_ResolvesFlowListViewFromGeneratedFactory(bool disableReflection)
    {
        using var host = CreateHubbleHost(disableReflection);
        await using var scope = host.Services.CreateAsyncScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<FlowListViewModel>();

        scope.ServiceProvider.GetRequiredService<ViewLocator>().Build(viewModel).ShouldBeOfType<FlowListView>();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Build_ResolvesServiceMapNodePropertiesViewFromGeneratedFactory(bool disableReflection)
    {
        using var host = CreateHubbleHost(disableReflection);
        using var scope = host.Services.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<ServiceMapNodePropertiesViewModel>();

        scope.ServiceProvider.GetRequiredService<ViewLocator>().Build(viewModel).ShouldBeOfType<ServiceMapNodePropertiesView>();
    }

    [AvaloniaFact]
    public void Build_EmitsGenericResourceListViewMetricName()
    {
        var measurements = new List<string>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == KubeInstrumentation.MeterName && instrument.Name == KubeInstrumentation.MeterName + "_view_opened")
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            foreach (var tag in tags)
            {
                if (tag.Key == "view" && tag.Value is string view)
                {
                    measurements.Add(view);
                }
            }
        });
        listener.Start();

        var services = Application.Current.GetTestServices();
        var locator = services.GetRequiredService<ViewLocator>();
        var viewModel = services.GetRequiredService<ResourceListViewModel<V1Pod>>();

        var view = locator.Build(viewModel);

        view.ShouldBeOfType<ResourceListView>();
        measurements.ShouldContain("ResourceListView<V1Pod>");
    }

    [AvaloniaFact]
    public void Build_ResolvesGenericResourcePropertiesViewWithoutRuntimeTypeDiscovery()
    {
        var services = Application.Current.GetTestServices();
        var locator = services.GetRequiredService<ViewLocator>();
        using var viewModel = new ResourcePropertiesViewModel<V1Pod>();

        var view = locator.Build(viewModel);

        view.ShouldBeOfType<ResourcePropertiesView<V1Pod>>();
    }

    [AvaloniaFact]
    public void Build_UsesFallbackForRepeatedUnmappedViewModel()
    {
        var services = Application.Current.GetTestServices();
        var locator = new ViewLocator(
            services,
            services.GetRequiredService<ILogger<ViewLocator>>(),
            services.GetRequiredService<Instrumentation>(),
            []);
        var viewModel = new UnmappedViewModel();

        locator.Build(viewModel).ShouldBeOfType<TextBlock>();
        locator.Build(viewModel).ShouldBeOfType<TextBlock>();
    }

    private static IHost CreateHubbleHost(bool disableReflection)
    {
        SettingsPersistenceData settings = new();
        settings.Settings.PreviewFeatures.Hubble = true;
        settings.Settings.PreviewFeatures.DisableJsonSerializationReflection = disableReflection;
        settings.Settings.PreviewFeatures.DisableYamlSerializationReflection = disableReflection;
        var persistence = new Mock<ISettingsPersistence>();
        persistence.Setup(value => value.Load()).Returns(settings);
        return Program.CreateHostBuilder(
            [],
            includeOptionalServices: false,
            configureServices: services =>
            {
                services.AddSingleton(Mock.Of<IFactory>());
                services.AddSingleton(persistence.Object);
            },
            settingsOverride: settings).Build();
    }

    private sealed class UnmappedViewModel;
}
