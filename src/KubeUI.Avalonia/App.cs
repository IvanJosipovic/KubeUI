using System.Diagnostics;
using Avalonia.Markup.Xaml.Styling;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using KubeUI.Avalonia.Features.Clusters.Error;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Styles;
using LiveMarkdown.Avalonia;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

[assembly: GenerateMarkupExtensionsForAssembly(typeof(Avalonia.Skia.SkiaPlatform))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Avalonia.Svg.Skia.SvgImage))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Avalonia.Xaml.Interactions.Core.DataTrigger))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Avalonia.Xaml.Interactions.Events.PointerPressedEventTrigger))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Avalonia.Xaml.Interactivity.EventTriggerBase))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(AvaloniaEdit.TextEditor))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(DataGrid))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Dock.Avalonia.Controls.DockableControl))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Dock.Controls.DeferredContentControl.DeferredContentControl))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(FluentAvalonia.UI.Controls.FABitmapIcon))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(FluentIcons.Avalonia.FluentIcon))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(LiveChartsCore.SkiaSharpView.Avalonia.PieChart))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(SvcSystems.UI.Terminal.Terminal))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(Ursa.Controls.Anchor))]

namespace KubeUI.Avalonia;

public partial class App : Application, IServiceProviderHost
{
    public static TopLevel? TopLevel { get; private set; }

    public IServiceProvider Services { get; private set; } = null!;
    private ILogger<App> _logger = null!;
    private IHostApplicationLifetime _hostApplicationLifetime = null!;
    private int _shutdownRequested;

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
        _hostApplicationLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        DataTemplates.Add(Services.GetRequiredService<ViewLocator>());

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Dispatcher.UIThread.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        KubernetesClientConfiguration.ExecStdError += KubernetesClientConfiguration_ExecStdError;
    }

    protected void DisposeApplication()
    {
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        Dispatcher.UIThread.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
        KubernetesClientConfiguration.ExecStdError -= KubernetesClientConfiguration_ExecStdError;
    }

    public override void Initialize()
    {
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
            var mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            TopLevel = desktop.MainWindow;
            desktop.ShutdownRequested += (_, _) => GracefulShutdown();

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

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        RecordUnhandledException(e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()), e.IsTerminating);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        RecordUnhandledException(e.Exception, isTerminating: false);
        e.SetObserved();
    }

    private void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data))
        {
            _logger.LogError("Cluster ExecStdError: no data");
            return;
        }

        _logger.LogError("Cluster ExecStdError: {Data}", e.Data);

        Dispatcher.UIThread.Post(() => ShowClusterError(e.Data), DispatcherPriority.Background);
    }

    private void ShowClusterError(string error)
    {
        var factory = Services.GetRequiredService<IFactory>();
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        if (documents == null)
        {
            return;
        }

        if (factory.FindDockableById("cluster-error") is ClusterErrorViewModel existing)
        {
            existing.Error = error;
            factory.SetActiveDockable(existing);
            factory.SetFocusedDockable(documents, existing);
            return;
        }

        var vm = Services.GetRequiredService<ClusterErrorViewModel>();
        vm.Id = "cluster-error";
        vm.Error = error;
        factory.AddDockable(documents, vm);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        RecordUnhandledException(e.Exception, isTerminating: false);
        e.Handled = true;
    }

    internal void RecordUnhandledException(Exception exception, bool isTerminating)
    {
        _logger.LogCritical(exception, "Unhandled exception (terminating: {IsTerminating})", isTerminating);
        FlushTelemetry();

        if (isTerminating)
        {
            GracefulShutdown();
        }
    }

    internal void GracefulShutdown()
    {
        if (Interlocked.Exchange(ref _shutdownRequested, 1) == 1)
        {
            return;
        }

        KubernetesClientConfiguration.ExecStdError -= KubernetesClientConfiguration_ExecStdError;
        FlushTelemetry();
        _hostApplicationLifetime.StopApplication();
    }

    protected virtual void FlushTelemetry()
    {
        foreach (var loggerProvider in Services.GetServices<ILoggerProvider>().OfType<LoggerProvider>())
        {
            loggerProvider.ForceFlush();
        }
        Services.GetService<MeterProvider>()?.ForceFlush();
        Services.GetService<TracerProvider>()?.ForceFlush();
    }
}
