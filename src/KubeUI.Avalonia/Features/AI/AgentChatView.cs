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
    private ScrollViewer? _conversationScrollViewer;
    private bool _conversationIsAtEnd = true;

    protected override object Build(AgentChatViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var markdownRenderer = new MarkdownRenderer
        {
            MarkdownBuilder = vm.MarkdownBuilder,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var conversationScrollViewer = new ScrollViewer()
            .Ref(out _conversationScrollViewer)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
            .Content(markdownRenderer)
            .Row(0);
        conversationScrollViewer.PropertyChanged += ConversationScrollViewerOnPropertyChanged;

        return new Grid()
            .Rows("*,Auto")
            .Margin(12)
            .Children(
                conversationScrollViewer,
                new Grid()
                    .Row(1)
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
