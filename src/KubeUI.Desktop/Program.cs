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
using KubeUI.Kubernetes;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
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
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App(BuildLauncherServices()))
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
            .WithInterFont()
            .UsePlatformDetect();

    private static ServiceProvider BuildLauncherServices()
    {
        var services = new ServiceCollection();

        ConfigureLiveCharts();

        TypeAdapterConfig.GlobalSettings
            .Default
            .MaxDepth(1)
            .ShallowCopyForSameType(true)
            .PreserveReference(true);

        var settings = SettingsService.LoadSettingsFromFile();

        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.AddSingleton<IThreadDispatcher, AvaloniaThreadDispatcher>();
        services.AddLogging(x => x.SetMinimumLevel(LogLevel.Debug));

        if (!Design.IsDesignMode)
        {
            if (settings.TelemetryEnabled)
            {
                services.AddTelemetry();
            }

            if (settings.LoggingEnabled)
            {
                services.AddFileLogging();
            }
        }

        services.AddSingleton<IDialogFactory, FluentDialogFactory>(_ => (FluentDialogFactory)new DialogFactory().AddFluent());
        services.AddSingleton<IDialogManager, DialogManager>(x => new MyDialogManager(
            dialogFactory: x.GetRequiredService<IDialogFactory>(),
            logger: x.GetRequiredService<ILogger<DialogManager>>()));
        services.AddSingleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>()));

        services.AddSingleton<IFactory>(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>())));
        services.AddSingleton<INotificationManager>(_ => Dispatcher.UIThread.Invoke(() => (INotificationManager)new WindowNotificationManager(App.TopLevel) { MaxItems = 4 }));
        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        var provider = services.BuildServiceProvider();
        provider.ConfigureKubeUIKubernetesJson();
        return provider;
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
                    .AddOtlpExporter((e, _) =>
                    {
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


