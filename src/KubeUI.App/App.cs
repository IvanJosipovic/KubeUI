using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Markup.Xaml.Styling;
using Dock.Settings;
using KubeUI.App.Infrastructure;
using KubeUI.App.Infrastructure.DependencyInjection;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Services.Settings;
using KubeUI.App.Shell.Main;
using KubeUI.App.Styles;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Themes;

[assembly: GenerateMarkupExtensionsForAvalonia(false)]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(LiveChartsCore.SkiaSharpView.Avalonia.PieChart), true)]

namespace KubeUI.App;

public partial class App : Application, IServiceProviderHost
{
    public static TopLevel? TopLevel { get; private set; }

    public IServiceProvider Services { get; private set; } = null!;
    private ILogger<App> _logger = null!;
    private IApplicationEventSource _applicationEventSource = null!;
    private ApplicationErrorHandler _applicationErrorHandler = null!;
    private ApplicationShutdownCoordinator _shutdownCoordinator = null!;

    public App(IServiceProvider serviceProvider)
    {
        InitializeApplication(serviceProvider);
    }

    protected App()
    {
    }

    protected void InitializeApplication(IServiceProvider serviceProvider)
    {
        Name = "KubeUI";

        Services = serviceProvider;
        _logger = Services.GetRequiredService<ILogger<App>>();
        _applicationEventSource = Services.GetRequiredService<IApplicationEventSource>();
        _applicationErrorHandler = Services.GetRequiredService<ApplicationErrorHandler>();
        _shutdownCoordinator = Services.GetRequiredService<ApplicationShutdownCoordinator>();

        DataTemplates.Add(Services.GetRequiredService<ViewLocator>());

        _applicationEventSource.CurrentDomainUnhandledException += CurrentDomain_UnhandledException;
        _applicationEventSource.DispatcherUnhandledException += OnUnhandledException;
        _applicationEventSource.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        _applicationEventSource.KubernetesExecStdError += KubernetesClientConfiguration_ExecStdError;
    }

    protected void DisposeApplication()
    {
        _applicationEventSource.CurrentDomainUnhandledException -= CurrentDomain_UnhandledException;
        _applicationEventSource.DispatcherUnhandledException -= OnUnhandledException;
        _applicationEventSource.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
        _applicationEventSource.KubernetesExecStdError -= KubernetesClientConfiguration_ExecStdError;
    }

    [RequiresUnreferencedCode("Application initialization loads Avalonia styles from runtime XAML resources.")]
    public override void Initialize()
    {
        DockSettings.MinimumHorizontalDragDistance = 8;
        DockSettings.MinimumVerticalDragDistance = 8;

        // Gauge arcs (cluster CPU/memory/pods) use flat ends; the default theme rounds them.
        LiveCharts.Configure(settings => settings.AddDefaultTheme(theme => theme.HasRuleForGaugeSeries(series => series.CornerRadius = 0)));

        var fluent = new Fluent();
        Styles.Add(fluent);
        Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://LiveMarkdown.Avalonia/Defaults.axaml"))
        {
            Source = new Uri("avares://LiveMarkdown.Avalonia/Defaults.axaml")
        });
        Resources.MergedDictionaries.Add(fluent.CreateMarkdownResourceOverrides());

        Services.GetRequiredService<Instrumentation>().AppOpened.Add(1);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.LogInformation("Creating main window");
            var mainWindow = Services.GetRequiredService<MainWindow>();
            _logger.LogInformation("Main window created");
            desktop.MainWindow = mainWindow;
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            TopLevel = desktop.MainWindow;
            mainWindow.Opened += (_, _) => _logger.LogInformation("Main window opened");
            mainWindow.Closed += (_, _) => _logger.LogInformation("Main window closed");
            desktop.ShutdownRequested += (_, _) =>
            {
                _logger.LogInformation("Avalonia shutdown requested");
                mainViewModel.CloseLayoutCommand.Execute(null);
                GracefulShutdown();
            };

            Dispatcher.UIThread.Post(mainViewModel.Initialize, DispatcherPriority.Background);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Services.GetRequiredService<MainView>();
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
        }

        Services.GetRequiredService<ISettingsService>().ApplySettings();

        base.OnFrameworkInitializationCompleted();
    }

    internal void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        RecordUnhandledException(e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()), e.IsTerminating);
    }

    internal void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        RecordUnhandledException(e.Exception, isTerminating: false);
        e.SetObserved();
    }

    private void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
    {
        _applicationErrorHandler.HandleKubernetesExecStdError(e.Data);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleUiThreadException(e.Exception);
        e.Handled = true;
    }

    internal void HandleUiThreadException(Exception exception)
    {
        RecordUnhandledException(exception, isTerminating: false);
    }

    internal void RecordUnhandledException(Exception exception, bool isTerminating)
    {
        _applicationErrorHandler.ReportUnhandledException(exception, isTerminating);

        if (isTerminating)
        {
            GracefulShutdown();
        }
    }

    internal void GracefulShutdown()
    {
        _shutdownCoordinator.RequestShutdown(() =>
            _applicationEventSource.KubernetesExecStdError -= KubernetesClientConfiguration_ExecStdError);
    }
}
