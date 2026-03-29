using Avalonia.Logging;

namespace KubeUI.Avalonia;

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

