using KubeUI.AI.Agents;
using KubeUI.Avalonia.Features.AI;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AgentContextServiceTests
{
    [Fact]
    public void context_is_owned_and_cleared_only_by_the_current_owner()
    {
        var service = new AgentContextService();
        var firstOwner = new object();
        var secondOwner = new object();
        var context = new AgentContext { Namespace = "default" };

        service.SetContext(firstOwner, context);
        service.Context.ShouldBe(context);

        service.ClearContext(secondOwner);
        service.Context.ShouldBe(context);

        service.ClearContext(firstOwner);
        service.Context.ShouldBeNull();
    }

    [Fact]
    public void prompt_context_lists_all_selected_resources()
    {
        var context = new AgentContext
        {
            SelectedResources =
            [
                new KubernetesResourceReference("v1", "Pod", "api", "default"),
                new KubernetesResourceReference("apps/v1", "Deployment", "web", "default")
            ]
        };

        context.ToPromptContext().ShouldBe(
            "Selected resource: v1/Pod default/api" + Environment.NewLine +
            "Selected resource: apps/v1/Deployment default/web");
    }
}
