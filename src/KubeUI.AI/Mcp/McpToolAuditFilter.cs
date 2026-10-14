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
                    ? McpToolOutcome.Denied
                    : McpToolOutcome.Succeeded;
                return result;
            }
            catch (OperationCanceledException)
            {
                outcome = McpToolOutcome.Cancelled;
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                outcome = McpToolOutcome.Denied;
                throw new UnauthorizedAccessException("MCP client is not authorized for this tool.");
            }
            catch
            {
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
}
