using System.Reflection;
using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Client;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;
using Mapster;
using NReco.Logging.File;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SkiaSharp;

namespace KubeUI;

public static class Program
{
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    => BuildAvaloniaAppWithServices();

    public static AppBuilder BuildAvaloniaAppWithServices()
        => AppBuilder.Configure(() => new App(BuildLauncherServices()))
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
            .WithInterFont();

    private static ServiceProvider BuildLauncherServices()
    {
        // retrieve service collection, add all required services & view models
        var services = new ServiceCollection();

        LiveCharts.Configure(config => config
                                .AddSkiaSharp()
                                .AddDefaultMappers()
                                .AddDefaultTheme(
                                // if necessary, override the default theme, for more info see:
                                // https://github.com/beto-rodriguez/LiveCharts2/blob/dev/samples/ViewModelsSamples/LiveChartsThemeExtensions.cs
                                theme =>
                                    theme.OnInitialized(() =>
                                    {
                                        theme.AnimationsSpeed = TimeSpan.FromMilliseconds(800);
                                        theme.EasingFunction = EasingFunctions.ExponentialOut;

                                        if (theme.IsDark)
                                        {
                                            //theme.Colors = ColorPalletes.MaterialDesign200;
                                            theme.VirtualBackroundColor = LvcColor.Parse("#1E1E1E");
                                            //theme.TooltipBackgroundPaint =
                                            //    new SolidColorPaint(new(45, 45, 45, 230))
                                            //    {
                                            //        ImageFilter = new DropShadow(4, 4, 12, 12, new(0, 0, 0, 255))
                                            //    };
                                            //theme.TooltipTextPaint = new SolidColorPaint(new(245, 245, 245));
                                            theme.LegendTextPaint = new SolidColorPaint(SKColors.White);
                                        }
                                        else
                                        {
                                            //theme.Colors = ColorPalletes.MaterialDesign500;
                                            theme.VirtualBackroundColor = new(255, 255, 255);
                                            //theme.TooltipBackgroundPaint =
                                            //    new SolidColorPaint(new(235, 235, 235, 230))
                                            //    {
                                            //        ImageFilter = new DropShadow(2, 2, 6, 6, new(0, 0, 0, 100))
                                            //    };
                                            //theme.TooltipTextPaint = new SolidColorPaint(new(30, 30, 30));
                                            theme.LegendTextPaint = new SolidColorPaint(SKColors.Black);
                                        }
                                    })
                                )
);

        TypeAdapterConfig.GlobalSettings
            .Default
            .MaxDepth(1)
            .ShallowCopyForSameType(true)
            .PreserveReference(true);

        var settings = SettingsService.LoadSettingsFromFile();

        services.AddServices();
        services.AddLogging(x => x.SetMinimumLevel(LogLevel.Debug));

        if (!Design.IsDesignMode)
        {
            if (settings.TelemetryEnabled)
                services.AddTelemetry();

            if (settings.LoggingEnabled)
                services.AddFileLogging();
        }

        services.AddSingleton<IDialogFactory, FluentDialogFactory>(x => (FluentDialogFactory)new DialogFactory().AddFluent());
        services.AddSingleton<IDialogManager, MyDialogManager>(x => new MyDialogManager(dialogFactory: x.GetRequiredService<IDialogFactory>(), logger: x.GetRequiredService<ILogger<DialogManager>>()));
        services.AddSingleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>()));

        services.AddSingleton<IFactory>(sp => Dispatcher.UIThread.Invoke<IFactory>(() => new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>())));

        services.AddSingleton<INotificationManager>(_ => Dispatcher.UIThread.Invoke<INotificationManager>(() => new WindowNotificationManager(App.TopLevel) { MaxItems = 4 }));

        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        return services.BuildServiceProvider();
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
                    x.FileSizeLimitBytes = 1 * 1024 * 1024 * 1024; // 1GB
                    x.MaxRollingFiles = 2;
                });
            }
        });

        return services;
    }

    private static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        const string key = "ff9c67da-5f13-46e9-9450-7e1dda139c08";
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

#if !DEBUG
        // Send only Warnings and above to OTEL
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
                })
            )
            .WithLogging(loggingProvider =>
            {
                loggingProvider.AddOtlpExporter((e) =>
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
                .AddOtlpExporter((e, b) =>
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
