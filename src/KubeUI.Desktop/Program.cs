using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Assets;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Velopack;

namespace KubeUI.Desktop;

internal static class Program
{
    private static IHost? _host;

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        _host = CreateHostBuilder(args).Build();
        _host.Services.ConfigureKubeUIKubernetesJsonLogging();

        _host.StartAsync().GetAwaiter().GetResult();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        _host.StopAsync().GetAwaiter().GetResult();

        _host.Dispose();
        _host = null;
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App(GetAppServices()))
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
            .WithInterFont()
            .UsePlatformDetect();

    private static IServiceProvider GetAppServices()
    {
        return _host?.Services ?? throw new InvalidOperationException("Application host has not been initialized.");
    }

    private static HostApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var settings = SettingsService.LoadSettingsFromFile();

        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        builder.Services.AddKubeUIAppServices();

        if (settings.TelemetryEnabled)
        {
            builder.Services.AddTelemetry();
        }

        if (settings.LoggingEnabled)
        {
            builder.Services.AddFileLogging();
        }

        builder.Services.AddSingleton<ServiceDescriptor[]>([.. builder.Services]);
        return builder;
    }

    private static IServiceCollection AddFileLogging(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            if (SettingsService.EnsureSettingDirExists())
            {
                loggingBuilder.AddFile(Path.Combine(SettingsService.GetSettingsPath(), "app.log"), x =>
                {
                    x.Append = false;
                    x.FileSizeLimitBytes = 1024L * 1024 * 1024;
                    x.MaxRollingFiles = 2;
                });
            }
        });

        return services;
    }

    private static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

#if !DEBUG
        const string key = "ff9c67da-5f13-46e9-9450-7e1dda139c08";
        services.AddLogging(x => x.AddFilter<OpenTelemetryLoggerProvider>("*", LogLevel.Warning));
#endif

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("Desktop", "com.KubeUI.Desktop", serviceVersion: version)
            .AddOperatingSystemDetector()
                .AddAttributes(new Dictionary<string, object>(StringComparer.Ordinal)
                {
#if DEBUG
                    { "deployment.environment", "Development" },
#else
                    { "deployment.environment", "Production" },
#endif
                }))
            .WithLogging(loggingProvider =>
            {
                loggingProvider.AddOtlpExporter(e =>
                {
#if DEBUG
                    e.Endpoint = new Uri("http://localhost:4317");
#else
                    e.Endpoint = new Uri("https://otel.kubeui.com/v1/logs");
                    e.Headers = $"key={key}";
                    e.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
#endif
                });
            },
            opt =>
            {
                opt.IncludeFormattedMessage = true;
                opt.IncludeScopes = true;
            })
            .WithMetrics(meterProvider =>
            {
                meterProvider
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(Instrumentation.MeterName)
                    .AddOtlpExporter((e, readerOptions) =>
                    {
                        readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
                        readerOptions.PeriodicExportingMetricReaderOptions.ExportTimeoutMilliseconds = 30000;
#if DEBUG
                        e.Endpoint = new Uri("http://localhost:4317");
#else
                        e.Endpoint = new Uri("https://otel.kubeui.com/v1/metrics");
                        e.Headers = $"key={key}";
                        e.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
#endif
                    });
            })
#if DEBUG
            .WithTracing(tracingProvider =>
            {
                tracingProvider
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(e =>
                    {
                        e.Endpoint = new Uri("http://localhost:4317");
                    });
            })
#endif
            ;

        return services;
    }
}


