using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
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
    private static readonly object HostLock = new();
    private static IHost? _host;

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        EnsureMacOsPath();

        _host = EnsureHostInitialized();

        var builder = AppBuilder.Configure(() => new App(_host.Services))
            .UsePlatformDetect()
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
            .WithInterFont()
            .UseServiceProvider(_host.Services)
            .UseComponentControlFactory(type => (Control)ActivatorUtilities.CreateInstance(_host.Services, type))
            .UseViewInitializationStrategy(ViewInitializationStrategy.Lazy)
#if DEBUG
            .UseHotReload()
#endif
            ;

        builder.StartWithClassicDesktopLifetime(args);

        _host.StopAsync().GetAwaiter().GetResult();

        _host.Dispose();
        _host = null;
    }

    //public static AppBuilder BuildAvaloniaApp()
    //        => AppBuilder.Configure(() => new App(EnsureHostInitialized().Services))
    //        .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
    //        .WithInterFont()
    //        .UsePlatformDetect();

    private static IHost EnsureHostInitialized()
    {
        if (_host != null)
        {
            return _host;
        }

        lock (HostLock)
        {
            if (_host == null)
            {
                _host = CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
                _host.Services.ConfigureKubeUIKubernetesJsonLogging();
                _host.StartAsync().GetAwaiter().GetResult();
            }
        }

        return _host;
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
