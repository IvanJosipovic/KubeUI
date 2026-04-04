using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public class ResourceInformerCallbackGuardTests
{
    [Fact]
    public void Execute_CatchesCallbackException_AndLogsError()
    {
        var logger = new CapturingLogger();
        var item = new V1Pod
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default"
            }
        };
        var exception = new InvalidOperationException("boom");

        Action act = () => ResourceInformerCallbackGuard.Execute(
            logger,
            WatchEventType.Modified,
            GroupApiVersionKind.From<V1Pod>(),
            item,
            () => throw exception);

        act.ShouldNotThrow();

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Error);
        logger.Entries[0].Exception.ShouldBe(exception);
        logger.Entries[0].Message.ShouldContain("Error processing Modified notification");
        logger.Entries[0].Message.ShouldContain("pod-1");
        logger.Entries[0].Message.ShouldContain("default");
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new CapturedLogEntry(logLevel, eventId, formatter(state, exception), exception));
        }
    }

    private sealed record CapturedLogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
