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

internal static class McpToolAuditFilter
{
    private const int MaximumOutputBytes = 1024 * 1024;

    internal static void Configure(IMcpRequestFilterBuilder filters)
    {
        filters.AddCallToolFilter(next => async (context, cancellationToken) =>
        {
            var identity = context.Services.GetRequiredService<McpRequestIdentityAccessor>().Current;
            var clientId = identity?.Id ?? "unauthenticated";
            var toolName = context.Params.Name;
            var logger = context.Services.GetRequiredService<ILoggerFactory>().CreateLogger("KubeUI.Mcp.Audit");
            var timeProvider = context.Services.GetRequiredService<TimeProvider>();
            var started = timeProvider.GetTimestamp();
            var outcome = McpToolOutcome.Failed;
            try
            {
                if (identity is null)
                    throw new UnauthorizedAccessException();
                var result = await next(context, cancellationToken).ConfigureAwait(false);
                if (ExceedsOutputLimit(result))
                {
                    outcome = McpToolOutcome.Oversized;
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = [new TextContentBlock
                        {
                            Text = "MCP output exceeds the 1 MiB response limit. Narrow the request and retry."
                        }]
                    };
                }
                outcome = result.IsError == true
                    ? McpToolOutcome.Failed
                    : McpToolOutcome.Succeeded;
                if (result.IsError == true)
                {
                    logger.LogError(
                        "MCP tool {ToolName} for client {ClientId} returned an error: {ErrorSummary}",
                        toolName, clientId, GetErrorSummary(result));
                }
                return result;
            }
            catch (OperationCanceledException exception)
            {
                outcome = McpToolOutcome.Cancelled;
                logger.LogInformation(
                    exception,
                    "MCP tool {ToolName} for client {ClientId} was cancelled",
                    toolName, clientId);
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                outcome = McpToolOutcome.Denied;
                logger.LogWarning(
                    "MCP tool {ToolName} for client {ClientId} was denied",
                    toolName, clientId);
                return new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContentBlock { Text = "The MCP tool request was denied by KubeUI." }]
                };
            }
            catch (ArgumentException exception)
            {
                logger.LogWarning(
                    exception,
                    "MCP tool {ToolName} for client {ClientId} rejected an invalid request",
                    toolName, clientId);
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "MCP tool {ToolName} for client {ClientId} failed",
                    toolName, clientId);
                throw new InvalidOperationException("MCP tool request could not be completed.");
            }
            finally
            {
                logger.LogInformation(
                    "MCP tool {ToolName} for client {ClientId} ended with {Outcome} in {ElapsedMilliseconds} ms",
                    toolName, clientId, outcome, timeProvider.GetElapsedTime(started).TotalMilliseconds);
            }
        });
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

    private static string GetErrorSummary(CallToolResult result)
    {
        const int maximumSummaryLength = 2048;
        var summary = new StringBuilder();
        foreach (var content in result.Content)
        {
            if (content is not TextContentBlock text || string.IsNullOrWhiteSpace(text.Text))
                continue;

            if (summary.Length > 0)
                summary.Append(" | ");
            var remainingLength = maximumSummaryLength - summary.Length;
            if (remainingLength <= 0)
                break;
            summary.Append(text.Text.AsSpan(0, Math.Min(text.Text.Length, remainingLength)));
        }

        if (summary.Length == 0)
            return "MCP server returned an error result without text details.";

        if (summary.Length == maximumSummaryLength)
            summary.Append("…");
        return summary.ToString();
    }
}
