using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubernetesCRDModelGen;
using KubeUI.Client;
using KubeUI.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NReco.Logging.File;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace KubeUI;

public partial class App : Application
{
    public static IHost Host { get; private set; }

    public static TopLevel TopLevel { get; private set; }

    private INotificationManager? NotificationManager { get; set; }

    private ILogger<App> logger;

    public override void Initialize()
    {
#if DEBUG
        this.AttachDevTools();
#endif

        LiveCharts.Configure(config => config
                                        .AddDarkTheme()
                                        .AddLightTheme()
        );

        var builder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(new()
        {
            ApplicationName = "KubeUI.Desktop",
            Configuration = new ConfigurationManager(),
            ContentRootPath = Directory.GetCurrentDirectory(),
        });

        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);

            loggingBuilder.AddFilter("System", LogLevel.Warning);
            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);
            loggingBuilder.AddFilter<OpenTelemetryLoggerProvider>("*", LogLevel.Warning);
            loggingBuilder.AddFilter<FileLoggerProvider>("*", LogLevel.Information);

            if (!Design.IsDesignMode && SettingsService.LoadSettingsFromFile().LoggingEnabled
                && SettingsService.EnsureSettingDirExists())
            {
                loggingBuilder.AddFile(Path.Combine(SettingsService.GetSettingsPath(), "app.log"), x =>
                {
                    x.Append = false;
                    x.FileSizeLimitBytes = 10_73_741_824;
                    x.MinLevel = LogLevel.Trace;
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
                    .AddAttributes(new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                    #if DEBUG
                        { "deployment.environment", "Development" },
                    #else
                        { "deployment.environment", "Production" },
                    #endif
                        { "host.type", RuntimeInformation.OSArchitecture.ToString() },
                        { "host.os", RuntimeInformation.OSDescription },
                    })
                )
                .WithLogging(loggingProvider =>
                {
                    loggingProvider.AddOtlpExporter((e) =>
                    {
                        e.Endpoint = new Uri("https://otel.kubeui.com/v1/logs");
                        e.Headers = $"key={key}";
                        e.Protocol = OtlpExportProtocol.HttpProtobuf;
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
                        b.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                        e.Endpoint = new Uri("https://otel.kubeui.com/v1/metrics");
                        e.Headers = $"key={key}";
                        e.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });
        }

        builder.Services.Scan(scan => scan
            .FromCallingAssembly()
                .AddClasses(classes => classes.AssignableToAny([typeof(UserControl), typeof(ObservableObject), typeof(ViewModelBase), typeof(MyViewBase<>)]))
                .AsSelf()
                .WithTransientLifetime()
        );

        builder.Services.Scan(x => x.FromCallingAssembly().AddClasses().UsingAttributes());

        // Services
        builder.Services.AddSingleton<IGenerator, Generator>();

        // Dialog
        builder.Services.AddSingleton<IDialogFactory, FluentDialogFactory>(x => (FluentDialogFactory)new DialogFactory().AddFluent());
        builder.Services.AddSingleton<IDialogManager, MyDialogManager>(x => new MyDialogManager(dialogFactory: x.GetRequiredService<IDialogFactory>(), logger: x.GetRequiredService<ILogger<DialogManager>>()));
        builder.Services.AddSingleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>()));

        builder.Services.AddSingleton<INotificationManager>(_ => NotificationManager!);

        Host = builder.Build();
        Resources[typeof(IServiceProvider)] = Host.Services;
        _ = Host.RunAsync();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        //Logger.Sink = Host.Services.GetRequiredService<ILoggerSink>();

        logger = Host.Services.GetRequiredService<ILogger<App>>();

        var settings = Host.Services.GetRequiredService<ISettingsService>();

        settings.LoadSettings();
        AvaloniaXamlLoader.Load(this);

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
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Host.Services.GetRequiredService<MainViewModel>();

            desktop.MainWindow = MainWindow.Build(vm);
            desktop.MainWindow.DataContext = vm;
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow);

            desktop.ShutdownRequested += (sender, e) =>
            {
                GracefulShutdown();
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Host.Services.GetRequiredService<MainView>();
            singleViewPlatform.MainView.DataContext = Host.Services.GetRequiredService<MainViewModel>();
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView);
        }

        NotificationManager = new WindowNotificationManager(TopLevel) { MaxItems = 4 };

        base.OnFrameworkInitializationCompleted();
    }

    private static void GracefulShutdown()
    {
        Host.Services.GetService<LoggerProvider>()?.ForceFlush();
        Host.Services.GetService<MeterProvider>()?.ForceFlush();
        Host.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
    }
}
