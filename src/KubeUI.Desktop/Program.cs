using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NReco.Logging.File;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Velopack;

namespace KubeUI.Desktop;

internal static partial class Program
{
    public static ActivitySource Source { get; } = new ActivitySource("com.KubeUI.Desktop", "1.0.0");

    [STAThread]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Host is disposed in finally; WebApplication implements IDisposable explicitly so the analyzer cannot track it.")]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        EnsureMacOsPath();

        var host = StartHost(args);

        try
        {
            var builder = CreateAppBuilder(host.Services);
            builder.StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            Task.Run(async () =>
            {
                await host.StopAsync().ConfigureAwait(false);
                await host.DisposeAsync().ConfigureAwait(false);
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

    internal static WebApplicationBuilder CreateHostBuilder(
        string[] args,
        bool includeOptionalServices = true,
        Action<IServiceCollection>? configureServices = null,
        int? mcpPortOverride = null,
        bool? mcpEnabledOverride = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = "KubeUI",
            Args = args
        });
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        var settings = SettingsPersistenceLoader.Load();
        builder.Services.AddKubeUIAppServices();

        if (mcpEnabledOverride ?? settings.Settings.McpServerEnabled)
        {
            builder.Services.AddMcpServer()
                .WithHttpTransport(options => options.Stateless = true)
                .WithTools<McpTools>();
            var port = mcpPortOverride ?? McpServerConfiguration.GetValidatedPort(settings.Settings);
            builder.WebHost.ConfigureKestrel(options => ConfigureMcpListenAddresses(options, port));
        }

        if (includeOptionalServices && settings.Settings.TelemetryEnabled)
        {
            builder.Services.AddTelemetry();
        }

        if (includeOptionalServices && settings.Settings.LoggingEnabled)
        {
            builder.Services.AddFileLogging();
        }

        configureServices?.Invoke(builder.Services);
        return builder;
    }

    internal static void ConfigureMcpEndpoint(WebApplication application)
    {
        var settings = application.Services.GetRequiredService<ISettingsService>().Settings;
        if (settings.McpServerEnabled)
        {
            application.MapMcp(McpServerConfiguration.Path);
        }
    }

    internal static WebApplication CreateAndConfigureMcpEndpoint(WebApplicationBuilder builder)
    {
        var application = builder.Build();
        ConfigureMcpEndpoint(application);
        return application;
    }

    private static void ConfigureMcpListenAddresses(KestrelServerOptions options, int port)
    {
        if (port == 0)
        {
            options.Listen(IPAddress.Loopback, port);
            options.Listen(IPAddress.IPv6Loopback, port);
            return;
        }

        options.ListenLocalhost(port);
    }

    /// <summary>
    /// Builds and starts the application host. If the MCP server port cannot be bound,
    /// the host is restarted on an ephemeral port and <see cref="Options.Settings.McpServerPort"/>
    /// is updated so the reported MCP endpoint matches the listening port.
    /// </summary>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Host disposal is guaranteed via try/finally; WebApplication implements IDisposable explicitly so the analyzer cannot track it.")]
    internal static WebApplication StartHost(
        string[] args,
        bool includeOptionalServices = true,
        Action<IServiceCollection>? configureServices = null,
        int? mcpPortOverride = null,
        bool? mcpEnabledOverride = null)
    {
        const int ephemeralPort = 0;

        WebApplication? application = BuildApplication(args, includeOptionalServices, configureServices, mcpPortOverride, mcpEnabledOverride);
        try
        {
            var configuredPort = mcpPortOverride ?? McpServerConfiguration.GetValidatedPort(application.Services.GetRequiredService<ISettingsService>().Settings);

            var failure = TryStart(application);
            if (failure is not null)
            {
                if (!IsMcpBindFailure(failure) || !IsMcpServerEnabled(application))
                {
                    ExceptionDispatchInfo.Capture(failure).Throw();
                }

                LogMcpPortFallback(application.Logger, failure, McpServerConfiguration.Host, configuredPort);

                ((IDisposable)application).Dispose();
                application = BuildApplication(args, includeOptionalServices, configureServices, ephemeralPort, mcpEnabledOverride);

                failure = TryStart(application);
                if (failure is not null)
                {
                    ExceptionDispatchInfo.Capture(failure).Throw();
                }
            }

            SynchronizeMcpPort(application);
            var owned = application;
            application = null;
            return owned;
        }
        finally
        {
            if (application is not null)
            {
                ((IDisposable)application).Dispose();
            }
        }
    }

    private static Exception? TryStart(WebApplication application)
    {
        try
        {
            application.Start();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to bind the MCP server to http://{Host}:{Port}, falling back to an ephemeral port")]
    private static partial void LogMcpPortFallback(ILogger logger, Exception exception, string host, int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "MCP server listening on http://{Host}:{Port}{Path}")]
    private static partial void LogMcpListening(ILogger logger, string host, int port, string path);

    private static WebApplication BuildApplication(
        string[] args,
        bool includeOptionalServices,
        Action<IServiceCollection>? configureServices,
        int? mcpPortOverride,
        bool? mcpEnabledOverride)
    {
        var application = CreateAndConfigureMcpEndpoint(CreateHostBuilder(args, includeOptionalServices, configureServices, mcpPortOverride, mcpEnabledOverride));
        application.Services.ConfigureKubeUIKubernetesJsonLogging();
        return application;
    }

    private static void SynchronizeMcpPort(WebApplication application)
    {
        var settings = application.Services.GetRequiredService<ISettingsService>().Settings;
        if (!settings.McpServerEnabled)
        {
            return;
        }

        var boundPort = GetBoundPort(application);
        if (boundPort.HasValue && boundPort.Value != settings.McpServerPort)
        {
            LogMcpListening(application.Logger, McpServerConfiguration.Host, boundPort.Value, McpServerConfiguration.Path);
            settings.McpServerPort = boundPort.Value;
        }
    }

    private static int? GetBoundPort(WebApplication application)
    {
        var addresses = application.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;

        foreach (var address in addresses ?? [])
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Port > 0)
            {
                return uri.Port;
            }
        }

        return null;
    }

    private static bool IsMcpServerEnabled(WebApplication application)
    {
        return application.Services.GetRequiredService<ISettingsService>().Settings.McpServerEnabled;
    }

    internal static bool IsMcpBindFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SocketException socketException &&
                socketException.SocketErrorCode is SocketError.AddressAlreadyInUse or SocketError.AccessDenied)
            {
                return true;
            }

            if (current is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    if (IsMcpBindFailure(innerException))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
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
