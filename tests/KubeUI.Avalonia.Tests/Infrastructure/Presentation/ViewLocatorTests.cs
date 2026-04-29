using System.Diagnostics.Metrics;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Presentation;

public sealed class ViewLocatorTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void Build_EmitsGenericResourceListViewMetricName()
    {
        var measurements = new List<string>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == Instrumentation.MeterName && instrument.Name == Instrumentation.MeterName + "_view_opened")
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

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var locator = services.GetRequiredService<ViewLocator>();
        var viewModel = services.GetRequiredService<ResourceListViewModel<V1Pod>>();

        var view = locator.Build(viewModel);

        view.ShouldBeOfType<ResourceListView>();
        measurements.ShouldContain("ResourceListView<V1Pod>");
    }
}
