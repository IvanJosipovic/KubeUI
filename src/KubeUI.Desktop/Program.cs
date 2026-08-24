using System.Diagnostics;
using System.Net;
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

internal static class Program
{
    public static ActivitySource Source { get; } = new ActivitySource("com.KubeUI.Desktop", "1.0.0");

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        EnsureMacOsPath();

        using var host = CreateStartedHost(args);

        var builder = CreateAppBuilder(host.Services);

        try
        {
            builder.StartWithClassicDesktopLifetime(args);
        }
        finally
        {
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
            builder.WebHost.ConfigureKestrel(options => ConfigureKestrelEndpoints(options, port));
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

    private static void ConfigureKestrelEndpoints(KestrelServerOptions options, int port)
    {
        if (port == McpServerConfiguration.DynamicPort)
        {
            // ListenLocalhost does not support dynamic ports; bind the IPv4 loopback directly
            // and let the operating system assign an available port.
            options.Listen(IPAddress.Loopback, port);
            return;
        }

        options.ListenLocalhost(port);
    }

    /// <summary>
    /// Builds and starts the application host. When the configured MCP port cannot be bound
    /// (already in use or blocked by the operating system), the host is rebuilt with a dynamically
    /// assigned port so the app still starts.
    /// </summary>
    internal static WebApplication CreateStartedHost(
        string[] args,
        bool includeOptionalServices = true,
        Action<IServiceCollection>? configureServices = null,
        int? mcpPortOverride = null,
        bool? mcpEnabledOverride = null)
    {
        var application = CreateApplication(args, includeOptionalServices, configureServices, mcpPortOverride, mcpEnabledOverride);
        try
        {
            application.Start();
            RecordMcpBoundPort(application);
            return application;
        }
        catch (Exception exception) when (IsPortBindFailure(exception))
        {
            application.DisposeAsync().AsTask().GetAwaiter().GetResult();

            application = CreateApplication(args, includeOptionalServices, configureServices, McpServerConfiguration.DynamicPort, mcpEnabledOverride);
            application.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(Program))
                .LogWarning(exception, "The configured MCP server port could not be bound; retrying with a dynamically assigned port.");
            application.Start();
            RecordMcpBoundPort(application);
            return application;
        }
    }

    private static WebApplication CreateApplication(
        string[] args,
        bool includeOptionalServices,
        Action<IServiceCollection>? configureServices,
        int? mcpPortOverride,
        bool? mcpEnabledOverride)
    {
        var application = CreateHostBuilder(
            args,
            includeOptionalServices,
            configureServices,
            mcpPortOverride,
            mcpEnabledOverride).Build();
        ConfigureMcpEndpoint(application);
        application.Services.ConfigureKubeUIKubernetesJsonLogging();
        return application;
    }

    private static void RecordMcpBoundPort(WebApplication application)
        => RecordMcpBoundPort(
            application.Services,
            application.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()?.Addresses);

    internal static void RecordMcpBoundPort(IServiceProvider services, IEnumerable<string?>? addresses)
    {
        var state = services.GetService<McpServerState>();
        if (state is null || addresses is null)
            return;

        foreach (var address in addresses)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) || !uri.IsLoopback)
                continue;

            state.SetBoundPort(uri.Port);
            return;
        }
    }

    internal static bool IsPortBindFailure(Exception exception)
    {
        for (var current = (Exception?)exception; current is not null; current = current.InnerException)
        {
            if (current is SocketException)
                return true;
        }

        return false;
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
