using System.Diagnostics;
using KubernetesClient.Informer.Client;
using KubeUI.App.Services.Settings;
using KubeUI.Desktop;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Shouldly;

namespace KubeUI.App.Tests.Desktop;

public sealed class TelemetrySourceRegistrationTests
{
    [Fact]
    public void telemetry_listens_to_informer_activities()
    {
#if DEBUG
        using var host = Program.CreateHostBuilder(
            [],
            includeOptionalServices: true,
            settingsOverride: new SettingsPersistenceData()).Build();
        using var tracerProvider = host.Services.GetRequiredService<TracerProvider>();
        using var informerSource = new ActivitySource(InformerDiagnostics.ActivitySourceName);

        informerSource.HasListeners().ShouldBeTrue();
#endif
    }
}
