using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Dock.Avalonia.Themes.Fluent;
using FluentAvalonia.Styling;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using KubeUI.Client;
using KubeUI.Views;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.Themes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NReco.Logging.File;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SkiaSharp;

namespace KubeUI;

public partial class App : Application
{
    public static IHost Host { get; private set; }

    public static TopLevel TopLevel { get; private set; }

    private INotificationManager? NotificationManager { get; set; }

    private ILogger<App> logger;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif

        LiveCharts.Configure(config => config
                                        .AddSkiaSharp()
                                        .AddDefaultMappers()
                                        .AddDefaultTheme(
                                        // if necessary, override the default theme, for more info see:
                                        // https://github.com/beto-rodriguez/LiveCharts2/blob/dev/samples/ViewModelsSamples/LiveChartsThemeExtensions.cs
                                        theme =>
                                            theme.OnInitialized(() =>
                                            {
                                                //theme.RequestedTheme = requestedTheme;
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

        var builder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(new()
        {
            ApplicationName = "KubeUI.Desktop",
            Configuration = new ConfigurationManager(),
            ContentRootPath = Directory.GetCurrentDirectory(),
#if DEBUG
            EnvironmentName = "Development",
#else
            EnvironmentName = "Production",
#endif
        });

        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Debug);

            // Send only Warnings and above to OTEL
#if !DEBUG
            loggingBuilder.AddFilter<OpenTelemetryLoggerProvider>("*", LogLevel.Warning);
#endif
            if (!Design.IsDesignMode && SettingsService.LoadSettingsFromFile().LoggingEnabled
                && SettingsService.EnsureSettingDirExists())
            {
                loggingBuilder.AddFile(Path.Combine(SettingsService.GetSettingsPath(), "app.log"), x =>
                {
                    x.Append = false;
                    x.FileSizeLimitBytes = 1 * 1024 * 1024 * 1024; // 1GB
                    x.MaxRollingFiles = 2;
                });
            }
        });

        if (SettingsService.LoadSettingsFromFile().TelemetryEnabled)
        {
            const string key = "ff9c67da-5f13-46e9-9450-7e1dda139c08";
            var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            builder.Services.AddOpenTelemetry()
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
        }

        // Services
        builder.Services.AddServices();

        // Dialog
        builder.Services.AddSingleton<IDialogFactory, FluentDialogFactory>(x => (FluentDialogFactory)new DialogFactory().AddFluent());
        builder.Services.AddSingleton<IDialogManager, MyDialogManager>(x => new MyDialogManager(dialogFactory: x.GetRequiredService<IDialogFactory>(), logger: x.GetRequiredService<ILogger<DialogManager>>()));
        builder.Services.AddSingleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>()));

        builder.Services.AddSingleton<INotificationManager>(_ => NotificationManager!);

        builder.Services.AddSingleton<ServiceDescriptor[]>([.. builder.Services]);

        Host = builder.Build();
        Resources[typeof(IServiceProvider)] = Host.Services;
        AddStyles(Host.Services);
        DataTemplates.Add(Host.Services.GetRequiredService<ViewLocator>());

        _ = Host.RunAsync();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        //Logger.Sink = Host.Services.GetRequiredService<ILoggerSink>();

        Host.Services.GetRequiredService<ISettingsService>().LoadSettings();

        logger = Host.Services.GetRequiredService<ILogger<App>>();

        // https://github.com/kubernetes-client/csharp/issues/1674
        var ctxType = typeof(k8s.Kubernetes).Assembly.GetType("k8s.SourceGenerationContext");
        var ctxProp = ctxType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
        var k8sResolver = ctxProp.GetValue(null) as IJsonTypeInfoResolver;

        KubernetesJson.AddJsonOptions(x =>
        {
            x.TypeInfoResolver = JsonTypeInfoResolver.Combine(k8sResolver, new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    jsonTypeInfo =>
                    {
                        if (jsonTypeInfo.Type?.Namespace?.StartsWith("KubeUI.Models") == true)
                        {
                            foreach (var prop in jsonTypeInfo.Properties)
                            {
                                // Mark all properties as optional to allow deserialization with missing fields
                                prop.IsRequired = false;
                            }
                        }

                        if (jsonTypeInfo.OriginatingResolver is DefaultJsonTypeInfoResolver)
                        {
                            logger.LogDebug("Type is Serialized using Reflection: {type}", jsonTypeInfo.Type);
	                    }
                    }
                }
            });
        });

        logger.LogInformation("Application Started");

        Host.Services.GetRequiredService<Instrumentation>().AppOpened.Add(1);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        logger.LogCritical(e.ExceptionObject as Exception, "Unhandled Exception");
        GracefulShutdown();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DisableAvaloniaDataAnnotationValidation();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Host.Services.GetRequiredService<MainWindow>();
            desktop.MainWindow.DataContext = Host.Services.GetRequiredService<MainViewModel>();
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow);

            desktop.ShutdownRequested += (sender, e) => GracefulShutdown();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Host.Services.GetRequiredService<MainView>();
            singleViewPlatform.MainView.DataContext = Host.Services.GetRequiredService<MainViewModel>();
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView);
        }

        Host.Services.GetRequiredService<ISettingsService>().ApplySettings();

        NotificationManager = new WindowNotificationManager(TopLevel) { MaxItems = 4 };

        base.OnFrameworkInitializationCompleted();
    }

    private void AddStyles(IServiceProvider serviceProvider)
    {
        Styles.Add(new Ursa.Themes.Semi.SemiTheme(serviceProvider));
        Styles.Add(new Semi.Avalonia.SemiTheme(serviceProvider));
        Styles.Add(new FluentAvaloniaTheme());

        var fluent = new FluentTheme(serviceProvider)
        {
            DensityStyle = DensityStyle.Compact,
        };

        var lightPalette = new ColorPaletteResources
        {
            Accent = Color.Parse("#ff0073cf"),
            AltHigh = Colors.White,
            AltLow = Colors.White,
            AltMedium = Colors.White,
            AltMediumHigh = Colors.White,
            AltMediumLow = Colors.White,
            BaseHigh = Colors.Black,
            BaseLow = Color.Parse("#ffcccccc"),
            BaseMedium = Color.Parse("#ff898989"),
            BaseMediumHigh = Color.Parse("#ff5d5d5d"),
            BaseMediumLow = Color.Parse("#ff737373"),
            ChromeAltLow = Color.Parse("#ff5d5d5d"),
            ChromeBlackHigh = Colors.Black,
            ChromeBlackLow = Color.Parse("#ffcccccc"),
            ChromeBlackMedium = Color.Parse("#ff5d5d5d"),
            ChromeBlackMediumLow = Color.Parse("#ff898989"),
            ChromeDisabledHigh = Color.Parse("#ffcccccc"),
            ChromeDisabledLow = Color.Parse("#ff898989"),
            ChromeGray = Color.Parse("#ff737373"),
            ChromeHigh = Color.Parse("#ffcccccc"),
            ChromeLow = Color.Parse("#ffececec"),
            ChromeMedium = Color.Parse("#ffe6e6e6"),
            ChromeMediumLow = Color.Parse("#ffececec"),
            ChromeWhite = Colors.White,
            ListLow = Color.Parse("#ffe6e6e6"),
            ListMedium = Color.Parse("#ffcccccc"),
            RegionColor = Color.Parse("#EEEEF2")
        };

        var darkPalette = new ColorPaletteResources
        {
            Accent = Color.Parse("#ff0073cf"),
            AltHigh = Colors.Black,
            AltLow = Colors.Black,
            AltMedium = Colors.Black,
            AltMediumHigh = Colors.Black,
            AltMediumLow = Colors.Black,
            BaseHigh = Colors.White,
            BaseLow = Color.Parse("#ff333333"),
            BaseMedium = Color.Parse("#ff9a9a9a"),
            BaseMediumHigh = Color.Parse("#ffb4b4b4"),
            BaseMediumLow = Color.Parse("#ff676767"),
            ChromeAltLow = Color.Parse("#ffb4b4b4"),
            ChromeBlackHigh = Colors.Black,
            ChromeBlackLow = Color.Parse("#ffb4b4b4"),
            ChromeBlackMedium = Colors.Black,
            ChromeBlackMediumLow = Colors.Black,
            ChromeDisabledHigh = Color.Parse("#ff333333"),
            ChromeDisabledLow = Color.Parse("#ff9a9a9a"),
            ChromeGray = Colors.Gray,
            ChromeHigh = Colors.Gray,
            ChromeLow = Color.Parse("#ff151515"),
            ChromeMedium = Color.Parse("#ff1d1d1d"),
            ChromeMediumLow = Color.Parse("#ff2c2c2c"),
            ChromeWhite = Colors.White,
            ListLow = Color.Parse("#ff1d1d1d"),
            ListMedium = Color.Parse("#ff333333"),
            RegionColor = Color.Parse("#1E1E1E")
        };

        fluent.Palettes.Add(ThemeVariant.Light, lightPalette);
        fluent.Palettes.Add(ThemeVariant.Dark, darkPalette);

        Styles.Add(fluent);

        Styles.Add(new StyleInclude(new Uri("avares://KubeUI"))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        });
        Styles.Add(new StyleInclude(new Uri("avares://KubeUI"))
        {
            Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml")
        });
        Styles.Add(new DockFluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://KubeUI"))
        {
            Source = new Uri("avares://NodeEditorAvalonia/Themes/NodeEditorTheme.axaml")
        });
        Styles.Add(new StyleInclude(new Uri("avares://KubeUI"))
        {
            Source = new Uri("avares://AvaloniaTerminal/Styles/Colors.axaml")
        });
        Styles.Add(new StyleInclude(new Uri("avares://KubeUI"))
        {
            Source = new Uri("avares://KubeUI/Styles/Fluent.axaml")
        });
    }

    private static void GracefulShutdown()
    {
        Host.Services.GetService<LoggerProvider>()?.ForceFlush();
        Host.Services.GetService<MeterProvider>()?.ForceFlush();
        Host.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
