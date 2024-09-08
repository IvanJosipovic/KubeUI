using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubernetesCRDModelGen;
using KubeUI.Desktop;
using KubeUI.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using NReco.Logging.File;

namespace KubeUI;

public partial class App : Application
{
    private ILogger<App> logger;
    private IServiceProvider _serviceProvider;

    public static TopLevel TopLevel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDevTools();
#endif

        LiveCharts.Configure(config => config
                                        .AddDarkTheme()
                                        .AddLightTheme()
        );

        ServiceCollection services = new();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter("Default", LogLevel.Information);
            loggingBuilder.AddFilter("System", LogLevel.Warning);
            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);

            if (!Design.IsDesignMode && SettingsService.GetSettings().LoggingEnabled
                && SettingsService.EnsureSettingDirExists())
            {
                loggingBuilder.AddFile(Path.Combine(SettingsService.GetSettingsPath(), "app.log"), x =>
                {
                    x.Append = false;
                    x.FileSizeLimitBytes = 10_73_741_824;
                    x.MinLevel = LogLevel.Information;
                    x.MaxRollingFiles = 2;
                });
            }
        });

        services.Scan(x => x.FromCallingAssembly().AddClasses().UsingAttributes());

        services.Scan(scan => scan
            .FromCallingAssembly()
                .AddClasses(classes => classes.AssignableToAny([typeof(UserControl), typeof(ObservableObject), typeof(ViewModelBase), typeof(MyViewBase<>)]))
                .AsSelf()
                .WithTransientLifetime()
        );

        // Services
        services.AddSingleton<IGenerator, Generator>();

        // Dialog
        services.AddSingleton<IDialogFactory, FluentDialogFactory>(x => (FluentDialogFactory)new DialogFactory().AddFluent());
        services.AddSingleton<IDialogManager, MyDialogManager>(x => new MyDialogManager(dialogFactory: x.GetRequiredService<IDialogFactory>(), logger: x.GetRequiredService<ILogger<DialogManager>>()));
        services.AddSingleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>()));

        _serviceProvider = services.BuildServiceProvider();

        Resources[typeof(IServiceProvider)] = _serviceProvider;

        Logger.Sink = _serviceProvider.GetRequiredService<ILoggerSink>();

        logger = _serviceProvider.GetRequiredService<ILogger<App>>();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        var settings = _serviceProvider.GetRequiredService<SettingsService>();

        settings.LoadSettings();

        logger.LogInformation("App Started");
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        logger.LogCritical(e.ExceptionObject as Exception, "Unhandled Exception");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = _serviceProvider.GetRequiredService<MainViewModel>();

            desktop.MainWindow = MainWindow.Build(vm);
            desktop.MainWindow.DataContext = vm;
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = _serviceProvider.GetRequiredService<MainView>();
            singleViewPlatform.MainView.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
