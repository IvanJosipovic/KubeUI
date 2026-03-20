using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Views;

namespace KubeUI.Avalonia.Behaviors;

public sealed class YamlEditorScrollBehavior : Behavior<TextEditor>
{
    private ScrollViewer? _scrollViewer;
    private IDisposable? _visibilitySubscription;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        // Watch visibility changes instead of Dock.Factory events. When the
        // editor becomes invisible (tab switched away) persist the offset; when
        // it becomes visible again attach the ScrollViewer and restore the offset.
        _visibilitySubscription = AssociatedObject.GetObservable(Visual.IsVisibleProperty)
            .Subscribe(visible =>
            {
                if (!visible)
                {
                    PersistScrollOffset();
                }
                else
                {
                    AttachScrollViewer();
                    Dispatcher.UIThread.Post(RestoreScrollOffset, DispatcherPriority.Loaded);
                }
            });

        AttachScrollViewer();
    }

    protected override void OnDetaching()
    {
        PersistScrollOffset();

        if (AssociatedObject != null)
        {
            _visibilitySubscription?.Dispose();
            _visibilitySubscription = null;

            DetachScrollViewer();
        }

        base.OnDetaching();
    }

    // Visibility-based handling replaces factory event-driven handling.

    private void PersistScrollOffset()
    {
        if (AssociatedObject?.DataContext is not ResourceYamlViewModel vm)
        {
            return;
        }

        // Try to use the attached ScrollViewer if available. If not yet attached,
        // attempt to locate it on demand so we don't lose the offset when the
        // behavior wasn't able to attach earlier (headless/test timing).
        var sv = _scrollViewer ?? AssociatedObject.GetScrollViewer();
        if (sv is not null)
        {
            // Cache for future use
            _scrollViewer = sv;
            vm.ScrollOffset = sv.Offset;
            return;
        }

        // Fallback: use TextEditor offsets
        vm.ScrollOffset = new Vector(AssociatedObject.HorizontalOffset, AssociatedObject.VerticalOffset);
    }

    private void RestoreScrollOffset()
    {
        if (AssociatedObject?.DataContext is not ResourceYamlViewModel vm || _scrollViewer == null)
        {
            return;
        }

        _scrollViewer.Offset = vm.ScrollOffset;
    }

    private void AttachScrollViewer()
    {
        if (AssociatedObject?.GetScrollViewer() is not ScrollViewer scrollViewer)
        {
            return;
        }

        if (ReferenceEquals(_scrollViewer, scrollViewer))
        {
            return;
        }

        DetachScrollViewer();
        _scrollViewer = scrollViewer;
        _scrollViewer.PropertyChanged += ScrollViewerOnPropertyChanged;
    }

    private void DetachScrollViewer()
    {
        if (_scrollViewer == null)
        {
            return;
        }

        _scrollViewer.PropertyChanged -= ScrollViewerOnPropertyChanged;
        _scrollViewer = null;
    }

    private void ScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ScrollViewer.OffsetProperty)
        {
            return;
        }

        PersistScrollOffset();
    }
}
