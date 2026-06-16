using System.Diagnostics;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using KubeUI.Avalonia.Features.Clusters.Error.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Main.ViewModels;
using KubeUI.Avalonia.Shell.Main.Views;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

[assembly: GenerateMarkupExtensionsForAssembly(typeof(DockControl))]

namespace KubeUI.Avalonia;

public partial class App : Application, IServiceProviderHost
{
    public static TopLevel? TopLevel { get; private set; }

    public IServiceProvider Services { get; }
    private readonly ILogger<App> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private int _shutdownRequested;

    public App(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        _logger = Services.GetRequiredService<ILogger<App>>();
        _hostApplicationLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        Resources["AppearanceSettings"] = Services.GetRequiredService<ISettingsService>().Appearance;
        Resources["DataGridRowHeight"] = Convert.ToDouble(Services.GetRequiredService<ISettingsService>().Appearance.ListRowHeight);
        Resources["DataGridColumnHeaderMinHeight"] = Convert.ToDouble(Services.GetRequiredService<ISettingsService>().Appearance.ListRowHeight + 4m);
        Resources["DataGridFontSize"] = Convert.ToDouble(Services.GetRequiredService<ISettingsService>().Appearance.FontSize);
        DataTemplates.Add(Services.GetRequiredService<ViewLocator>());

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Dispatcher.UIThread.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        KubernetesClientConfiguration.ExecStdError += KubernetesClientConfiguration_ExecStdError;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif

        Services.GetRequiredService<Instrumentation>().AppOpened.Add(1);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = mainWindow;
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

    private void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data))
        {
            _logger.LogError("Cluster ExecStdError: no data");
            return;
        }

        _logger.LogError("Cluster ExecStdError: {Data}", e.Data);

        Dispatcher.UIThread.Post(() => ShowClusterError(e.Data));
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
        _logger.LogCritical(e.Exception, "UI Thread Unhandled Exception");
        e.Handled = true;
    }

    private void GracefulShutdown()
    {
        if (Interlocked.Exchange(ref _shutdownRequested, 1) == 1)
        {
            return;
        }

        KubernetesClientConfiguration.ExecStdError -= KubernetesClientConfiguration_ExecStdError;
        Services.GetService<LoggerProvider>()?.ForceFlush();
        Services.GetService<MeterProvider>()?.ForceFlush();
        Services.GetService<TracerProvider>()?.ForceFlush();
        _hostApplicationLifetime.StopApplication();
    }

}
