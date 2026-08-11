using Avalonia;
using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AgentDockingTests
{
    [AvaloniaFact]
    public void agent_chat_starts_as_a_right_pinned_dockable()
    {
        var services = Application.Current.GetTestServices();
        services.GetRequiredService<ISettingsService>().Settings.McpServerEnabled = true;
        var factory = services.GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);

        var chat = layout.RightPinnedDockables
            .Single(dockable => dockable is AgentChatViewModel)
            .ShouldBeOfType<AgentChatViewModel>();
        var rightDock = factory.GetDockable<IToolDock>("RightDock")!;

        chat.CanPin.ShouldBeTrue();
        chat.GetPinnedBounds(out _, out _, out var width, out var height);
        width.ShouldBe(420);
        height.ShouldBe(0);
        rightDock.VisibleDockables.ShouldNotContain(chat);
    }

    [AvaloniaFact]
    public void agent_chat_is_not_added_when_mcp_is_disabled()
    {
        var services = Application.Current.GetTestServices();
        services.GetRequiredService<ISettingsService>().Settings.McpServerEnabled = false;
        var factory = services.GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();

        layout.RightPinnedDockables.ShouldNotContain(dockable => dockable is AgentChatViewModel);
    }
}
