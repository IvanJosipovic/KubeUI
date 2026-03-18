using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Views;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace KubeUI.Avalonia;

public partial class App : Application
{
    public IServiceProvider Services { get; set; }

    public static TopLevel TopLevel { get; private set; }

    private readonly ILogger<App> _logger;

    public App(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        _logger = Services.GetRequiredService<ILogger<App>>();

        Resources["AppearanceSettings"] = Services.GetRequiredService<ISettingsService>().Appearance;
        Resources["DataGridRowHeight"] = Convert.ToDouble(Services.GetRequiredService<ISettingsService>().Appearance.ListRowHeight);
        Resources["DataGridFontSize"] = Convert.ToDouble(Services.GetRequiredService<ISettingsService>().Appearance.FontSize);
        Resources[typeof(IServiceProvider)] = Services;
        DataTemplates.Add(Services.GetRequiredService<ViewLocator>());

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

        _logger.LogInformation("Application Started");
        Services.GetRequiredService<Instrumentation>().AppOpened.Add(1);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DisableAvaloniaDataAnnotationValidation();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;
            desktop.ShutdownRequested += (_, _) => GracefulShutdown();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Services.GetRequiredService<MainView>();
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
        }

        Services.GetRequiredService<ISettingsService>().ApplySettings();

        base.OnFrameworkInitializationCompleted();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception (terminating: {IsTerminating})", e.IsTerminating);
        GracefulShutdown();
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.LogCritical(e.Exception, "UI Thread Unhandled Exception");
        e.Handled = true;
    }

    private void GracefulShutdown()
    {
        Services.GetService<LoggerProvider>()?.ForceFlush();
        Services.GetService<MeterProvider>()?.ForceFlush();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}



