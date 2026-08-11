using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia;
using LiveMarkdown.Avalonia;
using KubeUI.AI.Configuration;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AgentChatViewTests
{
    [AvaloniaFact]
    public void prompt_editor_and_send_button_stay_at_bottom_of_chat_view()
    {
        var view = new AgentChatView
        {
            DataContext = new AgentChatViewModel(new AcpAgentRegistry([
                new AcpAgentDefinition
                {
                    Id = "test",
                    Name = "Test agent",
                    Executable = Environment.ProcessPath!
                }
            ]))
        };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var prompt = view.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "PromptEditor");
        var send = view.GetVisualDescendants().OfType<Button>().Single(control => control.Name == "SendButton");
        view.GetVisualDescendants().OfType<TextBlock>().ShouldContain(control => control.Text == "Test agent");
        prompt.ShouldNotBeNull();
        send.ShouldNotBeNull();
        prompt!.AcceptsReturn.ShouldBeFalse();
        prompt.KeyBindings.OfType<KeyBinding>().ShouldContain(binding =>
            binding.Gesture.ShouldBeOfType<KeyGesture>().Key == Key.Enter);
        var midpoint = view.Bounds.Height / 2;

        prompt!.TranslatePoint(new Point(0, 0), view)!.Value.Y.ShouldBeGreaterThan(midpoint);
        send!.TranslatePoint(new Point(0, 0), view)!.Value.Y.ShouldBeGreaterThan(midpoint);
    }

    [AvaloniaFact]
    public async Task conversation_messages_are_hosted_in_a_vertical_scroll_viewer()
    {
        await using var vm = new AgentChatViewModel(new AcpAgentRegistry([
            new AcpAgentDefinition
            {
                Id = "test",
                Name = "Test agent",
                Executable = Environment.ProcessPath!
            }
        ]));
        for (var index = 0; index < 30; index++)
            vm.Messages.Add(new AgentChatMessage("assistant", $"Message {index} with enough text to exceed the viewport."));

        var view = new AgentChatView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = GetConversationScrollViewer(view);
        scrollViewer.VerticalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Auto);
        scrollViewer.Extent.Height.ShouldBeGreaterThan(scrollViewer.Viewport.Height);
        view.GetVisualDescendants().OfType<MarkdownRenderer>().Count().ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task conversation_follows_new_messages_when_already_at_bottom()
    {
        await using var vm = CreateScrollableChatViewModel();
        var view = new AgentChatView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = GetConversationScrollViewer(view);
        scrollViewer.ScrollToEnd();
        Dispatcher.UIThread.RunJobs();
        var previousMaximum = scrollViewer.ScrollBarMaximum.Y;

        vm.Messages.Add(new AgentChatMessage("assistant", "A new message."));
        Dispatcher.UIThread.RunJobs();

        scrollViewer.Offset.Y.ShouldBeGreaterThanOrEqualTo(previousMaximum);
        scrollViewer.Offset.Y.ShouldBe(scrollViewer.ScrollBarMaximum.Y);
    }

    [AvaloniaFact]
    public async Task conversation_follows_repeated_streaming_updates_when_already_at_bottom()
    {
        await using var vm = CreateScrollableChatViewModel();
        var view = new AgentChatView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = GetConversationScrollViewer(view);
        scrollViewer.ScrollToEnd();
        Dispatcher.UIThread.RunJobs();
        var response = string.Empty;

        for (var index = 0; index < 20; index++)
        {
            response += $"Streaming response chunk {index}. ";
            vm.Messages[^1] = new AgentChatMessage("assistant", response);
            Dispatcher.UIThread.RunJobs();

            scrollViewer.Offset.Y.ShouldBe(scrollViewer.ScrollBarMaximum.Y);
        }
    }

    [AvaloniaFact]
    public async Task conversation_follows_a_message_added_from_a_background_continuation()
    {
        await using var vm = CreateScrollableChatViewModel();
        var view = new AgentChatView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = GetConversationScrollViewer(view);
        scrollViewer.ScrollToEnd();
        Dispatcher.UIThread.RunJobs();

        await Task.Run(() => vm.Messages.Add(new AgentChatMessage("assistant", "Background response.")));
        Dispatcher.UIThread.RunJobs();

        scrollViewer.Offset.Y.ShouldBe(scrollViewer.ScrollBarMaximum.Y);
    }

    [AvaloniaFact]
    public async Task conversation_does_not_follow_new_messages_when_not_at_bottom()
    {
        await using var vm = CreateScrollableChatViewModel();
        var view = new AgentChatView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = GetConversationScrollViewer(view);
        scrollViewer.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();

        vm.Messages.Add(new AgentChatMessage("assistant", "A new message."));
        Dispatcher.UIThread.RunJobs();

        scrollViewer.Offset.Y.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task streaming_message_updates_append_only_to_the_markdown_buffer()
    {
        await using var vm = new AgentChatViewModel(new AcpAgentRegistry([
            new AcpAgentDefinition
            {
                Id = "test",
                Name = "Test agent",
                Executable = Environment.ProcessPath!
            }
        ]));
        var view = new AgentChatView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(400, 600, view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.Messages.Add(new AgentChatMessage("assistant", "hello"));
        Dispatcher.UIThread.RunJobs();
        var renderer = view.GetVisualDescendants().OfType<MarkdownRenderer>().Single();
        var versionBeforeUpdate = renderer.MarkdownBuilder!.Version;

        vm.Messages[0] = new AgentChatMessage("assistant", "hello world");
        Dispatcher.UIThread.RunJobs();

        renderer.MarkdownBuilder.Version.ShouldBe(versionBeforeUpdate + 1);
        renderer.MarkdownBuilder.ToString().ShouldBe("**assistant**\n\nhello world");
    }

    private static AgentChatViewModel CreateScrollableChatViewModel()
    {
        var vm = new AgentChatViewModel(new AcpAgentRegistry([
            new AcpAgentDefinition
            {
                Id = "test",
                Name = "Test agent",
                Executable = Environment.ProcessPath!
            }
        ]));
        for (var index = 0; index < 30; index++)
            vm.Messages.Add(new AgentChatMessage("assistant", $"Message {index} with enough text to exceed the viewport."));
        return vm;
    }

    private static ScrollViewer GetConversationScrollViewer(AgentChatView view)
        => view.GetVisualDescendants().OfType<ScrollViewer>()
            .Single(control => control.Content is MarkdownRenderer);
}
