using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Assets;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Dialogs;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SkiaSharp;
using Velopack;

namespace KubeUI.Desktop;

internal static class Program
{
    private static IHost? _host;
    private static IServiceProvider? _designTimeServices;

    [STAThread]
    public static async Task Main(string[] args)
    {
        VelopackApp.Build().Run();

        _host = CreateHostBuilder(args).Build();
        _host.Services.ConfigureKubeUIKubernetesJsonLogging();

        await _host.StartAsync();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        await _host.StopAsync();

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
        if (Design.IsDesignMode)
        {
            _designTimeServices ??= BuildDesignTimeServices();
            return _designTimeServices;
        }

        return _host?.Services ?? throw new InvalidOperationException("Application host has not been initialized.");
    }

    private static HostApplicationBuilder CreateHostBuilder(string[] args)
    {
        ConfigureLiveCharts();
        ConfigureTypeAdapter();

        var builder = Host.CreateApplicationBuilder(args);
        var settings = SettingsService.LoadSettingsFromFile();

        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        builder.Services.AddKubeUIAvaloniaServices();
        builder.Services.AddKubeUIKubernetesServices();
        builder.Services.AddDialogServices();

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

    private static ServiceProvider BuildDesignTimeServices()
    {
        ConfigureLiveCharts();
        ConfigureTypeAdapter();

        var services = new ServiceCollection();
        services.AddLogging(x => x.SetMinimumLevel(LogLevel.Debug));
        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.AddDialogServices();
        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        var provider = services.BuildServiceProvider();
        provider.ConfigureKubeUIKubernetesJsonLogging();
        return provider;
    }

    private static void ConfigureTypeAdapter()
    {
        TypeAdapterConfig.GlobalSettings
            .Default
            .MaxDepth(1)
            .ShallowCopyForSameType(true)
            .PreserveReference(true);
    }

    private static IServiceCollection AddDialogServices(this IServiceCollection services)
    {
        services.AddSingleton<IDialogFactory, FluentDialogFactory>(_ => (FluentDialogFactory)new DialogFactory().AddFluent());
        services.AddSingleton<IDialogManager, DialogManager>(x => new MyDialogManager(
            dialogFactory: x.GetRequiredService<IDialogFactory>(),
            logger: x.GetRequiredService<ILogger<DialogManager>>()));
        services.AddSingleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>()));

        services.AddSingleton<IFactory>(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>())));
        services.AddSingleton<INotificationManager>(_ => Dispatcher.UIThread.Invoke(() => (INotificationManager)new WindowNotificationManager(App.TopLevel) { MaxItems = 4 }));
        return services;
    }

    private static void ConfigureLiveCharts()
    {
        LiveCharts.Configure(config => config
            .AddSkiaSharp()
            .AddDefaultMappers()
            .AddDefaultTheme(theme =>
                theme.OnInitialized(() =>
                {
                    theme.AnimationsSpeed = TimeSpan.FromMilliseconds(800);
                    theme.EasingFunction = EasingFunctions.ExponentialOut;

                    if (theme.IsDark)
                    {
                        theme.VirtualBackroundColor = LvcColor.Parse("#1E1E1E");
                        theme.LegendTextPaint = new SolidColorPaint(SKColors.White);
                    }
                    else
                    {
                        theme.VirtualBackroundColor = new(255, 255, 255);
                        theme.LegendTextPaint = new SolidColorPaint(SKColors.Black);
                    }
                })));
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
                    });
            })
#endif
            ;

        return services;
    }
}


