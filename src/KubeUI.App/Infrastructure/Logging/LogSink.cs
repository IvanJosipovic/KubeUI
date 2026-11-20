using Avalonia.Logging;

namespace KubeUI.App.Infrastructure.Logging;

public class LogSink : ILogSink
{
    private readonly ILogger<Application> _logger;

    public LogSink(ILogger<Application> logger)
    {
        _logger = logger;
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        var logLevel = GetLogLevel(level);

        return logLevel > LogLevel.Information;
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        var logLevel = GetLogLevel(level);
        // Guard avoids evaluating the log arguments when the level is disabled.
        if (_logger.IsEnabled(logLevel))
        {
#pragma warning disable CA2254 // Forwarding sink: Avalonia supplies the template and it must be passed through unchanged.
            _logger.Log(logLevel, messageTemplate);
#pragma warning restore CA2254
        }
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        var logLevel = GetLogLevel(level);
        // Guard avoids evaluating the log arguments when the level is disabled.
        if (_logger.IsEnabled(logLevel))
        {
#pragma warning disable CA2254 // Forwarding sink: Avalonia supplies the template and it must be passed through unchanged.
            _logger.Log(logLevel, messageTemplate, propertyValues);
#pragma warning restore CA2254
        }
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

