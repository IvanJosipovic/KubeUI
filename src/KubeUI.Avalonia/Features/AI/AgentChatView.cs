using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using LiveMarkdown.Avalonia;

namespace KubeUI.Avalonia.Features.AI;

/// <summary>
/// Represents the view for the agent chat interface.
/// </summary>
public sealed class AgentChatView : ViewBase<AgentChatViewModel>
{
    private ScrollViewer _conversationScrollViewer = null!;
    private bool _conversationIsAtEnd = true;

    protected override object Build(AgentChatViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        var controls = new Grid()
            .Rows("Auto,*,Auto")
            .Margin(12)
            .Children(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(4)
                    .Margin(0, 0, 0, 8)
                    .Children(
                        new TextBlock()
                            .Text(Assets.Resources.AgentChatView_AgentLabel),
                        new TextBlock()
                            .Text(vm, x => x.SelectedAgent.Name)),
                        new ScrollViewer()
                            .Ref(out _conversationScrollViewer)
                            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
                            .Row(1)
                            .Content(new MarkdownRenderer()
                                        {
                                            MarkdownBuilder = vm.MarkdownBuilder,
                                        }
                                        .HorizontalAlignment(HorizontalAlignment.Stretch)),
                        new Grid()
                            .Row(2)
                            .Cols("*,Auto,Auto")
                            .Children(
                                new TextBox()
                                    .Name("PromptEditor")
                                    .PlaceholderText(Assets.Resources.AgentChatView_PromptPlaceholder)
                                    .Text(vm, x => x.Prompt)
                                    .AcceptsReturn(false)
                                    .KeyBindings(new KeyBinding
                                    {
                                        Command = vm.SendCommand,
                                        Gesture = new KeyGesture(Key.Enter)
                                    })
                                    .Row(0),
                                new Button()
                                    .Name("SendButton")
                                    .Content(Assets.Resources.AgentChatView_Send)
                                    .Command(vm, x => x.SendCommand)
                                    .IsVisible(vm, x => !x.IsBusy)
                                    .Col(1),
                                new Button()
                                    .Name("CancelButton")
                                    .Content(Assets.Resources.AgentChatView_Cancel)
                                    .Command(vm, x => x.CancelCommand)
                                    .IsVisible(vm, x => x.IsBusy)
                                    .Col(2)))
                    .Background(Brushes.Transparent);

        _conversationScrollViewer.PropertyChanged += ConversationScrollViewerOnPropertyChanged;

        return controls;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_conversationScrollViewer is not null)
            _conversationScrollViewer.PropertyChanged -= ConversationScrollViewerOnPropertyChanged;
        _conversationScrollViewer = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void ConversationScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer ||
            (e.Property != ScrollViewer.OffsetProperty &&
             e.Property != ScrollViewer.ViewportProperty &&
             e.Property != ScrollViewer.ExtentProperty))
            return;

        if (e.Property == ScrollViewer.OffsetProperty)
        {
            _conversationIsAtEnd = scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y;
        }

        if (_conversationIsAtEnd)
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, double.PositiveInfinity);
    }
}
