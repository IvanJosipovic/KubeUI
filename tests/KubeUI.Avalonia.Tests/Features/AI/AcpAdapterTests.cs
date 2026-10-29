using System.Text.Json;
using KubeUI.AI.Acp;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;
using Shouldly;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Agent.Models;
using SvcSystems.AI.AgentClientProtocol.Client.Models;
using SvcSystems.AI.AgentClientProtocol.Models;

namespace KubeUI.Avalonia.Tests.Features.AI;

/// <summary>
/// Adapter tests for the ACP host layer. Every supported ACP update variant and permission
/// classification must be covered so the mapping between protocol types and KubeUI domain
/// behaviour cannot drift.
/// </summary>
public sealed class AcpAdapterTests
{
    private static readonly IReadOnlySet<string> s_trustedServers = new HashSet<string>(StringComparer.Ordinal) { "kubeui" };

    [Fact]
    public void permission_mapper_describes_a_tool_by_its_title()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate { ToolCallId = "call-1", Title = "Read pod logs", Name = "read_logs" });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.Action.ShouldBe("Read pod logs");
        mapped.RequiresApproval.ShouldBeTrue();
    }

    [Fact]
    public void permission_mapper_describes_an_mcp_tool_when_no_title_is_supplied()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate
            {
                ToolCallId = "call-1",
                Title = "",
                Name = "kubeui_get",
                RawInput = JsonDocument.Parse("""{"server":"kubeui","tool":"kubeui_get"}""").RootElement,
                Meta = new Dictionary<string, JsonElement>
                {
                    ["is_mcp_tool_call"] = JsonSerializer.SerializeToElement(true)
                }
            });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.Action.ShouldBe("MCP kubeui/kubeui_get");
        mapped.Resource.ShouldBe("""{"server":"kubeui","tool":"kubeui_get"}""");
    }

    [Theory]
    [InlineData("read", false)]
    [InlineData("search", false)]
    [InlineData("fetch", false)]
    [InlineData("think", false)]
    [InlineData("edit", true)]
    [InlineData("delete", true)]
    [InlineData("move", true)]
    [InlineData("execute", true)]
    [InlineData("unknown-kind", true)]
    public void permission_mapper_classifies_tool_kinds(string kind, bool expectedDestructive)
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate { ToolCallId = "call-1", Title = "Tool", Kind = kind });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.IsDestructive.ShouldBe(expectedDestructive);
    }

    [Fact]
    public void permission_mapper_treats_an_unknown_tool_kind_as_destructive()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate { ToolCallId = "call-1", Title = "Tool" });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.IsDestructive.ShouldBeTrue();
    }

    [Fact]
    public void permission_mapper_does_not_require_approval_for_a_trusted_mcp_server()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate
            {
                ToolCallId = "call-1",
                Title = "",
                Name = "kubeui_get",
                RawInput = JsonDocument.Parse("""{"server":"kubeui","tool":"kubeui_get"}""").RootElement,
                Meta = new Dictionary<string, JsonElement>
                {
                    ["is_mcp_tool_call"] = JsonSerializer.SerializeToElement(true)
                }
            });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.RequiresApproval.ShouldBeFalse();
    }

    [Fact]
    public void permission_mapper_requires_approval_for_an_untrusted_mcp_server()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate
            {
                ToolCallId = "call-1",
                Title = "",
                Name = "kubeui_get",
                RawInput = JsonDocument.Parse("""{"server":"other","tool":"kubeui_get"}""").RootElement,
                Meta = new Dictionary<string, JsonElement>
                {
                    ["is_mcp_tool_call"] = JsonSerializer.SerializeToElement(true)
                }
            });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.RequiresApproval.ShouldBeTrue();
    }

    [Fact]
    public void permission_mapper_requires_approval_when_the_tool_identity_is_incomplete()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate
            {
                ToolCallId = "call-1",
                Title = "",
                Name = "kubeui_get",
                RawInput = JsonDocument.Parse("""{"server":"kubeui"}""").RootElement,
                Meta = new Dictionary<string, JsonElement>
                {
                    ["is_mcp_tool_call"] = JsonSerializer.SerializeToElement(true)
                }
            });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.RequiresApproval.ShouldBeTrue();
    }

    [Fact]
    public void permission_mapper_accepts_a_string_encoded_mcp_marker()
    {
        var request = CreateRequest(
            toolCall: new ToolCallUpdate
            {
                ToolCallId = "call-1",
                Title = "",
                Name = "kubeui_get",
                RawInput = JsonDocument.Parse("""{"server":"kubeui","tool":"kubeui_get"}""").RootElement,
                Meta = new Dictionary<string, JsonElement>
                {
                    ["is_mcp_tool_call"] = JsonSerializer.SerializeToElement("true")
                }
            });

        var mapped = new PermissionMapper().Map(request, s_trustedServers);

        mapped.RequiresApproval.ShouldBeFalse();
    }

    [Fact]
    public async Task permission_handler_selects_the_allow_option_when_the_service_allows()
    {
        var service = new StubPermissionService(allowed: true);
        var handler = new PermissionHandler(service, s_trustedServers);
        var request = CreateRequest(
            toolCall: new ToolCallUpdate { ToolCallId = "call-1", Title = "Read pod logs" });

        var decision = await handler.RequestPermissionAsync(request, TestContext.Current.CancellationToken);

        decision.Cancelled.ShouldBeFalse();
        decision.SelectedOptionId.ShouldBe("allow-once");
        service.LastRequest.ShouldNotBeNull();
        service.LastRequest!.Action.ShouldBe("Read pod logs");
    }

    [Fact]
    public async Task permission_handler_selects_the_reject_option_when_the_service_denies()
    {
        var service = new StubPermissionService(allowed: false);
        var handler = new PermissionHandler(service, s_trustedServers);
        var request = CreateRequest(
            toolCall: new ToolCallUpdate { ToolCallId = "call-1", Title = "Delete pod" });

        var decision = await handler.RequestPermissionAsync(request, TestContext.Current.CancellationToken);

        decision.Cancelled.ShouldBeFalse();
        decision.SelectedOptionId.ShouldBe("reject-once");
    }

    [Fact]
    public async Task permission_handler_cancels_when_no_matching_option_exists()
    {
        var service = new StubPermissionService(allowed: true);
        var handler = new PermissionHandler(service, s_trustedServers);
        var request = new RequestPermissionRequest
        {
            SessionId = "session-1",
            ToolCall = new ToolCallUpdate { ToolCallId = "call-1", Title = "Read pod logs" },
            Options = [new PermissionOption { OptionId = "other", Name = "Other", Kind = "other" }]
        };

        var decision = await handler.RequestPermissionAsync(request, TestContext.Current.CancellationToken);

        decision.Cancelled.ShouldBeTrue();
        decision.SelectedOptionId.ShouldBeNull();
    }

    [Fact]
    public async Task permission_handler_auto_allows_a_trusted_mcp_tool_without_prompting()
    {
        var service = new StubPermissionService(allowed: false);
        var handler = new PermissionHandler(service, s_trustedServers);
        var request = CreateRequest(
            toolCall: new ToolCallUpdate
            {
                ToolCallId = "call-1",
                Title = "",
                Name = "kubeui_get",
                RawInput = JsonDocument.Parse("""{"server":"kubeui","tool":"kubeui_get"}""").RootElement,
                Meta = new Dictionary<string, JsonElement>
                {
                    ["is_mcp_tool_call"] = JsonSerializer.SerializeToElement(true)
                }
            });

        var decision = await handler.RequestPermissionAsync(request, TestContext.Current.CancellationToken);

        decision.Cancelled.ShouldBeFalse();
        decision.SelectedOptionId.ShouldBe("allow-once");
        service.LastRequest.ShouldBeNull("a trusted MCP tool must not prompt the user");
    }

    [Fact]
    public async Task permission_handler_returns_a_cancelled_decision_when_the_wait_is_cancelled()
    {
        var service = new CancellingPermissionService();
        var handler = new PermissionHandler(service, s_trustedServers);
        var request = CreateRequest(
            toolCall: new ToolCallUpdate { ToolCallId = "call-1", Title = "Read pod logs" });

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var decision = await handler.RequestPermissionAsync(request, cancellation.Token);

        decision.Cancelled.ShouldBeTrue();
        decision.SelectedOptionId.ShouldBeNull();
    }

    [Fact]
    public void tool_call_tracker_forwards_a_tool_call_start_once()
    {
        var tracker = new AcpToolCallTracker();
        var update = new SessionUpdateEvent(new SessionUpdate(new ToolCall { ToolCallId = "call-1", Title = "Read pod" }));

        tracker.ShouldForward(update).ShouldBeTrue();
        tracker.ShouldForward(update).ShouldBeFalse();
    }

    [Fact]
    public void tool_call_tracker_forwards_a_tool_call_completion_once()
    {
        var tracker = new AcpToolCallTracker();
        var update = new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate
        {
            ToolCallId = "call-1",
            Status = "completed",
            Title = "Read pod"
        }));

        tracker.ShouldForward(update).ShouldBeTrue();
        tracker.ShouldForward(update).ShouldBeFalse();
    }

    [Fact]
    public void tool_call_tracker_forwards_a_failed_tool_call_completion_once()
    {
        var tracker = new AcpToolCallTracker();
        var update = new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate
        {
            ToolCallId = "call-1",
            Status = "failed",
            Title = "Read pod"
        }));

        tracker.ShouldForward(update).ShouldBeTrue();
        tracker.ShouldForward(update).ShouldBeFalse();
    }

    [Fact]
    public void tool_call_tracker_forwards_every_tool_call_update_status()
    {
        var tracker = new AcpToolCallTracker();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate
        {
            ToolCallId = "call-1",
            Status = "pending"
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate
        {
            ToolCallId = "call-1",
            Status = "in_progress"
        }))).ShouldBeTrue();
    }

    [Fact]
    public void tool_call_tracker_allows_a_tool_call_to_restart_after_it_completed()
    {
        var tracker = new AcpToolCallTracker();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new ToolCall
        {
            ToolCallId = "call-1",
            Title = "Read pod"
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate
        {
            ToolCallId = "call-1",
            Status = "completed"
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new ToolCall
        {
            ToolCallId = "call-1",
            Title = "Read pod"
        }))).ShouldBeTrue("a new turn may start the same tool call again");

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate
        {
            ToolCallId = "call-1",
            Status = "completed"
        }))).ShouldBeTrue("the restarted tool call must report its own completion");
    }

    [Fact]
    public void tool_call_tracker_forwards_every_other_update_variant()
    {
        var tracker = new AcpToolCallTracker();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new SessionUpdateVariant0UserMessageChunk
        {
            Content = new ContentBlock(new TextContent { Text = "hello" })
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new SessionUpdateVariant1AgentMessageChunk
        {
            Content = new ContentBlock(new TextContent { Text = "hi" })
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new SessionUpdateVariant2AgentThoughtChunk
        {
            Content = new ContentBlock(new TextContent { Text = "thinking" })
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new Plan
        {
            Entries = [new PlanEntry { Content = "Inspect pod", Priority = "high", Status = "pending" }]
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new UsageUpdate
        {
            Used = 1,
            Size = 2
        }))).ShouldBeTrue();

        tracker.ShouldForward(new SessionUpdateEvent(new SessionUpdate(new Notice
        {
            Severity = "info",
            Title = "heads up"
        }))).ShouldBeTrue();
    }

    [Fact]
    public void tool_call_tracker_forwards_the_prompt_completion_event()
    {
        var tracker = new AcpToolCallTracker();

        tracker.ShouldForward(new Completed(new PromptResponse { StopReason = "end_turn" })).ShouldBeTrue();
    }

    private static RequestPermissionRequest CreateRequest(ToolCallUpdate toolCall) => new()
    {
        SessionId = "session-1",
        ToolCall = toolCall,
        Options =
        [
            new PermissionOption { OptionId = "allow-once", Name = "Allow", Kind = "allow_once" },
            new PermissionOption { OptionId = "allow-always", Name = "Always", Kind = "allow_always" },
            new PermissionOption { OptionId = "reject-once", Name = "Reject", Kind = "reject_once" },
            new PermissionOption { OptionId = "reject-always", Name = "Never", Kind = "reject_always" }
        ]
    };

    private sealed class StubPermissionService(bool allowed) : IAgentPermissionService
    {
        public AgentPermissionRequest? LastRequest { get; private set; }

        public Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new AgentPermissionResult(allowed));
        }
    }

    private sealed class CancellingPermissionService : IAgentPermissionService
    {
        public async Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            return new AgentPermissionResult(true);
        }
    }
}
