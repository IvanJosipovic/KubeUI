using System.Text.Json.Serialization.Metadata;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using k8s;
using KubeUI;
using KubeUI.Client;
using KubeUI.Views;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace KubeUI;

public partial class App : Application
{
    public IServiceProvider Services { get; set; }

    public static TopLevel TopLevel { get; private set; }

    private readonly ILogger<App> _logger;

    public App(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        _logger = Services.GetRequiredService<ILogger<App>>();

        Resources[typeof(IServiceProvider)] = Services;
        DataTemplates.Add(Services.GetRequiredService<ViewLocator>());

        //Logger.Sink = Services.GetRequiredService<ILogSink>();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Dispatcher.UIThread.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif

        KubernetesJson.AddJsonOptions(x =>
        {
            x.TypeInfoResolver = JsonTypeInfoResolver.Combine(CustomSourceGenerationContext.Default, SourceGenerationContext.Default, new DefaultJsonTypeInfoResolver
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
                            _logger.LogDebug("Type is Serialized using Reflection: {type}", jsonTypeInfo.Type);
                        }
                    }
                }
            });
        });

        _logger.LogInformation("Application Started");

        Services.GetRequiredService<Instrumentation>().AppOpened.Add(1);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception (terminating: {IsTerminating})", e.IsTerminating);
        GracefulShutdown();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DisableAvaloniaDataAnnotationValidation();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;

            desktop.ShutdownRequested += (sender, e) => GracefulShutdown();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Services.GetRequiredService<MainView>();
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
        }

        Services.GetRequiredService<ISettingsService>().ApplySettings();

        base.OnFrameworkInitializationCompleted();
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unobserved task exception");

        // Prevent the exception from terminating the process
        e.SetObserved();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.LogCritical(e.Exception, "UI Thread Unhandled Exception");
    }

    private void GracefulShutdown()
    {
        Services.GetService<LoggerProvider>()?.ForceFlush();
        Services.GetService<MeterProvider>()?.ForceFlush();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
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
