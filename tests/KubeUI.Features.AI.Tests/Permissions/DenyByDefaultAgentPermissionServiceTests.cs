using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Permissions;
using Shouldly;

namespace KubeUI.Features.AI.Tests.Permissions;

public sealed class DenyByDefaultAgentPermissionServiceTests
{
    [Fact]
    public async Task Requests_are_denied_by_default_with_a_configured_reason()
    {
        var service = new DenyByDefaultAgentPermissionService();

        var result = await service.RequestPermissionAsync(
            new AgentPermissionRequest("read_file", "C:\\some\\path"),
            TestContext.Current.CancellationToken);

        result.Allowed.ShouldBeFalse();
        result.Reason.ShouldBe("No interactive permission service is configured.");
    }

    [Fact]
    public async Task Null_requests_are_rejected()
    {
        var service = new DenyByDefaultAgentPermissionService();

        await Should.ThrowAsync<ArgumentNullException>(async () => await service.RequestPermissionAsync(
            null!,
            TestContext.Current.CancellationToken));
    }
}
