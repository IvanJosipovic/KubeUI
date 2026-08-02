using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
#if DEBUG
using Declarative.Avalonia.AgentTools;
#endif
using KubeUI.Avalonia;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Platform;
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
    public static ActivitySource Source { get; } = new ActivitySource("com.KubeUI.Desktop", "1.0.0");

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        EnsureMacOsPath();

        var host = CreateHostBuilder(args).Build();
        host.Services.ConfigureKubeUIKubernetesJsonLogging();
        host.Start();

        var builder = AppBuilder.Configure(() => new App(host.Services))
            .UsePlatformDetect()
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
            .WithInterFont()
            .UseServiceProvider(host.Services)
            .UseComponentControlFactory(type => (Control)ActivatorUtilities.CreateInstance(host.Services, type))
            .UseViewInitializationStrategy(ViewInitializationStrategy.Lazy)
#if DEBUG
            .UseHotReload()
            .UseAgentInspector(o =>
            {
                o.EnableInteraction = true;
                o.Services = host.Services;
            })
#endif
            ;

        builder.StartWithClassicDesktopLifetime(args);

        host.WaitForShutdown();

        host.Dispose();
    }

    private static HostApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings()
        {
            ApplicationName = "KubeUI",
            Args = args
        });
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        var settings = SettingsPersistenceLoader.Load();
        builder.Services.AddKubeUIAppServices();

        if (settings.Settings.TelemetryEnabled)
        {
            builder.Services.AddTelemetry();
        }

        if (settings.Settings.LoggingEnabled)
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
            var settingsDirectory = SettingsPersistenceLoader.SettingsDirectory;
            if (SettingsPersistenceLoader.EnsureDirectoryExists())
            {
                loggingBuilder.AddFile(Path.Combine(settingsDirectory, "app.log"), x =>
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
                    e.Endpoint = new Uri("https://otel-grpc.kubeui.com");
                    e.Headers = $"x-otlp-api-key={key}";
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
#if DEBUG
                        e.Endpoint = new Uri("http://localhost:4317");
#else
                        e.Endpoint = new Uri("https://otel-grpc.kubeui.com");
                        e.Headers = $"x-otlp-api-key={key}";
#endif
                    });
            })
#if DEBUG
            .WithTracing(tracingProvider =>
            {
                tracingProvider
                    .AddSource(Source.Name)
                    .AddSource(Kubernetes.Client.KubeInstrumentation.SourceName)
                    .AddSource(Instrumentation.SourceName)
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

    private static void EnsureMacOsPath()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        var macOsDefaultPaths = new[]
        {
            "/opt/homebrew/bin",
            "/opt/homebrew/sbin",
            "/usr/local/bin",
            "/usr/bin",
            "/bin",
            "/usr/sbin",
            "/sbin"
        };

        var existingPath = Environment.GetEnvironmentVariable("PATH");

        var paths = existingPath?
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList()
            ?? [];

        foreach (var path in macOsDefaultPaths)
        {
            if (!paths.Contains(path, StringComparer.Ordinal))
                paths.Add(path);
        }

        Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, paths));
    }
}
