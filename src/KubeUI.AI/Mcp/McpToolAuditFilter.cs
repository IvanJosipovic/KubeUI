using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace KubeUI.AI.Mcp;

internal enum McpToolOutcome
{
    Failed,
    Oversized,
    Cancelled,
    Denied,
    Succeeded
}

/// <summary>Outcome of a single audited MCP tool call.</summary>
/// <param name="Outcome">Audit category recorded for the call.</param>
/// <param name="Result">Result returned to the caller, when the call completed without throwing.</param>
internal sealed record McpToolAuditOutcome(McpToolOutcome Outcome, CallToolResult? Result);

internal static partial class McpToolAuditFilter
{
    private const int MaximumOutputBytes = 1024 * 1024;

    internal static void Configure(IMcpRequestFilterBuilder filters)
    {
        filters.AddCallToolFilter(next => async (context, cancellationToken) =>
        {
            var identity = context.Services.GetRequiredService<McpRequestIdentityAccessor>().Current;
            var toolName = context.Params.Name;
            var logger = context.Services.GetRequiredService<ILoggerFactory>().CreateLogger("KubeUI.Mcp.Audit");
            var timeProvider = context.Services.GetRequiredService<TimeProvider>();

            var outcome = await ExecuteAsync(
                identity,
                toolName,
                logger,
                timeProvider,
                cancellationToken => next(context, cancellationToken),
                cancellationToken).ConfigureAwait(false);

            return outcome.Result!;
        });
    }

    /// <summary>
    /// Runs a single MCP tool call through the audit policy.
    /// </summary>
    /// <param name="identity">Authenticated caller, or <see langword="null"/> for an unauthenticated request.</param>
    /// <param name="toolName">Name of the invoked tool.</param>
    /// <param name="logger">Audit logger.</param>
    /// <param name="timeProvider">Time provider used to measure the call duration.</param>
    /// <param name="handler">Invokes the tool.</param>
    /// <param name="cancellationToken">Token used to cancel the call.</param>
    /// <returns>The audit outcome and the result returned to the caller.</returns>
    /// <exception cref="OperationCanceledException">The call was cancelled.</exception>
    /// <exception cref="ArgumentException">The request was invalid.</exception>
    /// <exception cref="InvalidOperationException">The call failed unexpectedly.</exception>
    internal static async Task<McpToolAuditOutcome> ExecuteAsync(
        McpClientIdentity? identity,
        string toolName,
        ILogger logger,
        TimeProvider timeProvider,
        Func<CancellationToken, ValueTask<CallToolResult>> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(handler);

        var clientId = identity?.Id ?? "unauthenticated";
        var started = timeProvider.GetTimestamp();
        var outcome = McpToolOutcome.Failed;
        try
        {
            if (identity is null)
                throw new UnauthorizedAccessException();

            var result = await handler(cancellationToken).ConfigureAwait(false);
            if (ExceedsOutputLimit(result))
            {
                outcome = McpToolOutcome.Oversized;
                return new McpToolAuditOutcome(outcome, new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContentBlock
                    {
                        Text = "MCP output exceeds the 1 MiB response limit. Narrow the request and retry."
                    }]
                });
            }

            outcome = result.IsError == true
                ? McpToolOutcome.Failed
                : McpToolOutcome.Succeeded;

            return new McpToolAuditOutcome(outcome, result);
        }
        catch (OperationCanceledException exception)
        {
            outcome = McpToolOutcome.Cancelled;
            LogToolCancelled(logger, exception, toolName, clientId);
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            outcome = McpToolOutcome.Denied;
            LogToolDenied(logger, toolName, clientId);
            return new McpToolAuditOutcome(outcome, new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = "The MCP tool request was denied by KubeUI." }]
            });
        }
        catch (ArgumentException exception)
        {
            LogToolInvalidRequest(logger, exception, toolName, clientId);
            throw;
        }
        catch (Exception exception)
        {
            LogToolFailed(logger, exception, toolName, clientId);
            throw new InvalidOperationException("MCP tool request could not be completed.", exception);
        }
        finally
        {
            LogToolCompleted(logger, toolName, clientId, outcome, timeProvider.GetElapsedTime(started).TotalMilliseconds);
        }
    }

    internal static bool ExceedsOutputLimit(CallToolResult result)
    {
        long outputBytes = result.StructuredContent is null
            ? 0
            : Encoding.UTF8.GetByteCount(result.StructuredContent.Value.GetRawText());
        foreach (var content in result.Content)
        {
            if (content is not TextContentBlock text)
                return true;
            outputBytes += Encoding.UTF8.GetByteCount(text.Text);
            if (outputBytes > MaximumOutputBytes)
                return true;
        }
        return outputBytes > MaximumOutputBytes;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "MCP tool {ToolName} for client {ClientId} was cancelled")]
    private static partial void LogToolCancelled(ILogger logger, Exception exception, string toolName, string clientId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "MCP tool {ToolName} for client {ClientId} was denied")]
    private static partial void LogToolDenied(ILogger logger, string toolName, string clientId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "MCP tool {ToolName} for client {ClientId} rejected an invalid request")]
    private static partial void LogToolInvalidRequest(ILogger logger, Exception exception, string toolName, string clientId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "MCP tool {ToolName} for client {ClientId} failed")]
    private static partial void LogToolFailed(ILogger logger, Exception exception, string toolName, string clientId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "MCP tool {ToolName} for client {ClientId} ended with {Outcome} in {ElapsedMilliseconds} ms")]
    private static partial void LogToolCompleted(ILogger logger, string toolName, string clientId, McpToolOutcome outcome, double elapsedMilliseconds);
}
