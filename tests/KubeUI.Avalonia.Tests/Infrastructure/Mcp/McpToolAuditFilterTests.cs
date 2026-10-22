using KubeUI.AI.Mcp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

/// <summary>
/// Behaviour tests for the MCP tool audit filter, which records the audit category of every
/// tool call and converts denials into an error result for the caller.
/// </summary>
public sealed class McpToolAuditFilterTests
{
    private static readonly McpClientIdentity s_identity = new(
        "client-1",
        "Test client",
        IsInternal: false,
        new HashSet<string>(StringComparer.Ordinal));

    private static readonly TimeProvider s_timeProvider = TimeProvider.System;

    [Fact]
    public async Task unauthenticated_call_is_denied_and_returns_an_error_result()
    {
        var invoked = false;

        var outcome = await McpToolAuditFilter.ExecuteAsync(
            identity: null,
            "kubeui_list_clusters",
            NullLogger.Instance,
            s_timeProvider,
            _ =>
            {
                invoked = true;
                return new ValueTask<CallToolResult>(new CallToolResult { Content = [] });
            },
            TestContext.Current.CancellationToken);

        outcome.Outcome.ShouldBe(McpToolOutcome.Denied);
        outcome.Result.ShouldNotBeNull();
        outcome.Result.IsError.ShouldBe(true);
        outcome.Result.Content.ShouldHaveSingleItem()
            .ShouldBeOfType<TextContentBlock>()
            .Text.ShouldBe("The MCP tool request was denied by KubeUI.");

        invoked.ShouldBeFalse("the tool must not run for an unauthenticated caller");
    }

    [Fact]
    public async Task successful_call_is_recorded_as_succeeded_and_returns_the_tool_result()
    {
        var toolResult = new CallToolResult
        {
            Content = [new TextContentBlock { Text = "ok" }]
        };

        var outcome = await McpToolAuditFilter.ExecuteAsync(
            s_identity,
            "kubeui_list_clusters",
            NullLogger.Instance,
            s_timeProvider,
            _ => new ValueTask<CallToolResult>(toolResult),
            TestContext.Current.CancellationToken);

        outcome.Outcome.ShouldBe(McpToolOutcome.Succeeded);
        outcome.Result.ShouldBeSameAs(toolResult);
        outcome.Result.IsError.ShouldNotBe(true);
    }

    [Fact]
    public async Task tool_error_result_is_recorded_as_failed_and_returned_unchanged()
    {
        var toolResult = new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = "cluster not found" }]
        };

        var outcome = await McpToolAuditFilter.ExecuteAsync(
            s_identity,
            "kubeui_get_resource_yaml",
            NullLogger.Instance,
            s_timeProvider,
            _ => new ValueTask<CallToolResult>(toolResult),
            TestContext.Current.CancellationToken);

        outcome.Outcome.ShouldBe(McpToolOutcome.Failed);
        outcome.Result.ShouldBeSameAs(toolResult);
    }

    [Fact]
    public async Task oversized_result_is_replaced_with_a_limit_error_result()
    {
        var outcome = await McpToolAuditFilter.ExecuteAsync(
            s_identity,
            "kubeui_list_events",
            NullLogger.Instance,
            s_timeProvider,
            _ => new ValueTask<CallToolResult>(new CallToolResult
            {
                Content = [new TextContentBlock { Text = new string('x', 1024 * 1024 + 1) }]
            }),
            TestContext.Current.CancellationToken);

        outcome.Outcome.ShouldBe(McpToolOutcome.Oversized);
        outcome.Result.ShouldNotBeNull();
        outcome.Result.IsError.ShouldBe(true);
        outcome.Result.Content.ShouldHaveSingleItem()
            .ShouldBeOfType<TextContentBlock>()
            .Text.ShouldContain("1 MiB");
    }

    [Fact]
    public async Task cancelled_call_is_recorded_as_cancelled_and_reraises_the_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var handlerEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = McpToolAuditFilter.ExecuteAsync(
            s_identity,
            "kubeui_list_events",
            NullLogger.Instance,
            s_timeProvider,
            async token =>
            {
                handlerEntered.TrySetResult();
                using (token.Register(() => release.TrySetCanceled(token)))
                {
                    await release.Task.ConfigureAwait(false);
                }

                return new CallToolResult();
            },
            cancellation.Token);

        await handlerEntered.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () => await pending);
    }

    [Fact]
    public async Task invalid_request_is_recorded_and_reraises_the_argument_exception()
    {
        var thrown = await Should.ThrowAsync<ArgumentException>(async () =>
            await McpToolAuditFilter.ExecuteAsync(
                s_identity,
                "kubeui_list_resources",
                NullLogger.Instance,
                s_timeProvider,
                _ => throw new ArgumentException("namespace is invalid", "namespace"),
                TestContext.Current.CancellationToken));

        thrown.Message.ShouldContain("namespace is invalid");
    }

    [Fact]
    public async Task unexpected_failure_is_reported_as_a_generic_invalid_operation()
    {
        var thrown = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await McpToolAuditFilter.ExecuteAsync(
                s_identity,
                "kubeui_list_resources",
                NullLogger.Instance,
                s_timeProvider,
                _ => throw new InvalidOperationException("connection reset by peer"),
                TestContext.Current.CancellationToken));

        thrown.Message.ShouldBe("MCP tool request could not be completed.");
        thrown.InnerException.ShouldBeOfType<InvalidOperationException>()
            .Message.ShouldBe("connection reset by peer");
    }

    [Fact]
    public async Task audit_log_records_the_client_and_tool_without_leaking_the_result()
    {
        var logger = new RecordingLogger();

        await McpToolAuditFilter.ExecuteAsync(
            s_identity,
            "kubeui_get_resource_yaml",
            logger,
            s_timeProvider,
            _ => new ValueTask<CallToolResult>(new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = "secret value" }]
            }),
            TestContext.Current.CancellationToken);

        logger.Entries.ShouldContain(entry =>
            entry.Level == LogLevel.Error
            && entry.Message.Contains("kubeui_get_resource_yaml")
            && entry.Message.Contains("client-1"));
        logger.Entries.ShouldNotContain(entry => entry.Message.Contains("secret value"));
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
