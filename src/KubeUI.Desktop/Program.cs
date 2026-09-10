using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Declarative;
using Avalonia.Threading;
#if DEBUG
using Declarative.Avalonia.AgentTools;
#endif
using KubeUI.Avalonia;
using KubeUI.AI.Diagnostics;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Services.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Velopack;

namespace KubeUI.Desktop;

internal static class Program
{
    public static ActivitySource Source { get; } = new ActivitySource("com.KubeUI.Desktop", "1.0.0");

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        EnsureMacOsPath();

        using var host = CreateStartedHost(args);

        try
        {
            CreateAppBuilder(host.Services).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("KubeUI.Desktop.Program").LogCritical(
                exception,
                "Avalonia startup failed");
            throw;
        }
        finally
        {
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("KubeUI.Desktop.Program").LogWarning(
                "Avalonia lifetime ended; stopping host");
            Task.Run(async () =>
            {
                await host.StopAsync().ConfigureAwait(false);
            }).GetAwaiter().GetResult();
        }
    }

    internal static AppBuilder CreateAppBuilder(
        IServiceProvider services,
        Func<AppBuilder, AppBuilder>? configurePlatform = null,
        bool enableDevelopmentTools = true)
    {
        RegisterAvaloniaShutdown(services);

        var builder = AppBuilder.Configure(() => new App(services));
        builder = (configurePlatform ?? (static builder => builder.UsePlatformDetect()))(builder);

        builder = builder
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
            .WithInterFont()
            .UseServiceProvider(services)
            .UseComponentControlFactory(type => (Control)ActivatorUtilities.CreateInstance(services, type))
            .UseViewInitializationStrategy(ViewInitializationStrategy.Lazy);
#if DEBUG
        if (enableDevelopmentTools)
        {
            builder = builder
                .UseHotReload()
                .UseAgentInspector(o =>
                {
                    o.EnableInteraction = true;
                    o.Services = services;
                });
        }
#endif
        return builder;
    }

    internal static void RegisterAvaloniaShutdown(IServiceProvider services, Action? shutdownAvalonia = null)
    {
        shutdownAvalonia ??= static () =>
        {
            static void ShutdownAvalonia()
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    desktop.TryShutdown();
            }

            if (Dispatcher.UIThread.CheckAccess())
                ShutdownAvalonia();
            else
                Dispatcher.UIThread.Post(ShutdownAvalonia);
        };

        services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(shutdownAvalonia);
    }

    internal static HostApplicationBuilder CreateHostBuilder(
        string[] args,
        bool includeOptionalServices = true,
        Action<IServiceCollection>? configureServices = null)
    {
        return CreateDesktopHostBuilder(args, includeOptionalServices, configureServices, null, null);
    }

    private static HostApplicationBuilder CreateDesktopHostBuilder(
        string[] args,
        bool includeOptionalServices,
        Action<IServiceCollection>? configureServices,
        int? mcpPortOverride,
        bool? mcpEnabledOverride)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            ApplicationName = "KubeUI",
            Args = args
        });
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        var settings = SettingsPersistenceLoader.Load();
        builder.Services.AddKubeUIAppServices();

        if (mcpEnabledOverride ?? settings.Settings.McpServerEnabled)
        {
            builder.Services.AddRouting();
            builder.Services.AddSingleton(
                static _ => new DiagnosticListener("KubeUI.Mcp"));
            builder.Services.AddMcpServer()
                .WithHttpTransport(options => options.Stateless = true)
                .WithTools<McpTools>();
            var port = mcpPortOverride ?? settings.Settings.McpServerPort;
            builder.Services.AddSingleton<IHostedService>(services =>
                new McpServerHostedService(services, port));
        }

        if (includeOptionalServices && settings.Settings.TelemetryEnabled)
            builder.Services.AddTelemetry();

        if (includeOptionalServices && settings.Settings.LoggingEnabled)
            builder.Services.AddFileLogging();

        configureServices?.Invoke(builder.Services);
        return builder;
    }

    /// <summary>
    /// Builds and starts the desktop application host. MCP bind failure does not stop desktop startup.
    /// </summary>
    internal static IHost CreateStartedHost(
        string[] args,
        bool includeOptionalServices = true,
        Action<IServiceCollection>? configureServices = null,
        int? mcpPortOverride = null,
        bool? mcpEnabledOverride = null)
    {
        var host = CreateDesktopHostBuilder(
            args,
            includeOptionalServices,
            configureServices,
            mcpPortOverride,
            mcpEnabledOverride).Build();
        host.Start();
        return host;
    }

    internal static bool IsPortBindFailure(Exception exception)
    {
        return exception switch
        {
            SocketException => true,
            AggregateException aggregate => aggregate.InnerExceptions.Any(IsPortBindFailure),
            _ when exception.InnerException is not null => IsPortBindFailure(exception.InnerException),
            _ => false
        };
    }

    private static IServiceCollection AddFileLogging(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            var settingsDirectory = SettingsPersistenceLoader.SettingsDirectory;
            if (SettingsPersistenceLoader.EnsureDirectoryExists())
            {
                loggingBuilder.AddFile(Path.Combine(settingsDirectory, "app.log"), x =>
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
                    e.Endpoint = new Uri("https://otel-grpc.kubeui.com");
                    e.Headers = $"x-otlp-api-key={key}";
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
                    .AddOtlpExporter((e, readerOptions) =>
                    {
#if DEBUG
                        e.Endpoint = new Uri("http://localhost:4317");
#else
                        e.Endpoint = new Uri("https://otel-grpc.kubeui.com");
                        e.Headers = $"x-otlp-api-key={key}";
#endif
                    });
            })
#if DEBUG
            .WithTracing(tracingProvider =>
            {
                tracingProvider
                    .AddSource(Source.Name)
                    .AddSource(AgentActivitySource.SourceName)
                    .AddSource(Kubernetes.Client.KubeInstrumentation.SourceName)
                    .AddSource(Instrumentation.SourceName)
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

    private static void EnsureMacOsPath()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        var macOsDefaultPaths = new[]
        {
            "/opt/homebrew/bin",
            "/opt/homebrew/sbin",
            "/usr/local/bin",
            "/usr/bin",
            "/bin",
            "/usr/sbin",
            "/sbin"
        };

        var existingPath = Environment.GetEnvironmentVariable("PATH");

        var paths = existingPath?
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList()
            ?? [];

        foreach (var path in macOsDefaultPaths)
        {
            if (!paths.Contains(path, StringComparer.Ordinal))
                paths.Add(path);
        }

        Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, paths));
    }
}
