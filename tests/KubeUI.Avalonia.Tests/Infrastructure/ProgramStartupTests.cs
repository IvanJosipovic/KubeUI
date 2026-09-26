using KubeUI.Desktop;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure;

public sealed class ProgramStartupTests
{
    [Fact]
    public void StartHostAfterAvaloniaSetup_sets_up_Avalonia_before_host_start()
    {
        Mock<IHost> host = new(MockBehavior.Strict);
        List<string> events = [];
        host.Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("host-start"))
            .Returns(Task.CompletedTask);

        Program.StartHostAfterAvaloniaSetup(
            host.Object,
            () => events.Add("avalonia-setup"),
            () => events.Add("avalonia-run"),
            NullLogger.Instance);

        Assert.Equal(["avalonia-setup", "host-start", "avalonia-run"], events);
        host.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void StartHostAfterAvaloniaSetup_logs_startup_failure_at_critical_with_exception_details()
    {
        Mock<IHost> host = new(MockBehavior.Strict);
        using var loggerProvider = new CapturingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider));
        var exception = CreateException("startup crash");

        Should.Throw<InvalidOperationException>(() => Program.StartHostAfterAvaloniaSetup(
            host.Object,
            () => throw exception,
            () => { },
            loggerFactory.CreateLogger("KubeUI.Desktop.Program")));

        loggerProvider.Records.ShouldContain(record =>
            record.LogLevel == LogLevel.Critical &&
            ReferenceEquals(record.Exception, exception) &&
            record.Message.Contains("Avalonia startup failed", StringComparison.Ordinal) &&
            exception.ToString().Contains("startup crash", StringComparison.Ordinal));
        host.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static InvalidOperationException CreateException(string message)
    {
        try
        {
            throw new InvalidOperationException(message);
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<CapturedLog> Records { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Records);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(List<CapturedLog> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            records.Add(new CapturedLog(logLevel, exception, formatter(state, exception)));
        }
    }

    private sealed record CapturedLog(LogLevel LogLevel, Exception? Exception, string Message);
}
