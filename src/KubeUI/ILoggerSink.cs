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
        return level switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.None,
        };
    }
}
