using Avalonia.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrutor;

namespace KubeUI.Desktop;

[ServiceDescriptor(typeof(ILoggerSink), ServiceLifetime.Singleton)]
public class ILoggerSink : ILogSink
{
    private readonly ILogger<App> _logger;

    public ILoggerSink(ILogger<App> logger)
    {
        _logger = logger;
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        return _logger.IsEnabled(GetLogLevel(level));
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        _logger.Log(GetLogLevel(level), messageTemplate);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        _logger.Log(GetLogLevel(level), messageTemplate, propertyValues);
    }

    private static LogLevel GetLogLevel(LogEventLevel level)
    {
        var logLevel = LogLevel.None;

        switch (level)
        {
            case LogEventLevel.Verbose:
                logLevel = LogLevel.Trace;
                break;
            case LogEventLevel.Debug:
                logLevel = LogLevel.Debug;
                break;
            case LogEventLevel.Information:
                logLevel = LogLevel.Information;
                break;
            case LogEventLevel.Warning:
                logLevel = LogLevel.Warning;
                break;
            case LogEventLevel.Error:
                logLevel = LogLevel.Error;
                break;
            case LogEventLevel.Fatal:
                logLevel = LogLevel.Critical;
                break;
        }

        return logLevel;
    }
}
