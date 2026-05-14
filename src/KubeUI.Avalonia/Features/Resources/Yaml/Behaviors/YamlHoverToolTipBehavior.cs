using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

public sealed class YamlHoverToolTipBehavior : Behavior<TextEditor>
{
    private static readonly TimeSpan HoverDelay = TimeSpan.FromMilliseconds(250);

    private ResourceYamlViewModel? _currentViewModel;
    private bool _hoverTooltipOpen;
    private ScrollViewer? _scrollViewer;
    private Vector? _tooltipScrollOffset;
    private readonly DispatcherTimer _hoverTimer = new() { Interval = HoverDelay };
    private Point? _pendingHoverPoint;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        AssociatedObject.DataContextChanged += OnDataContextChanged;
        AssociatedObject.TextChanged += OnTextChanged;
        AssociatedObject.LayoutUpdated += OnLayoutUpdated;
        AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += OnDetachedFromVisualTree;
        AssociatedObject.TextArea.TextView.PointerHover += TextViewOnPointerHover;
        AssociatedObject.TextArea.TextView.PointerMoved += TextViewOnPointerMoved;
        AssociatedObject.TextArea.TextView.PointerExited += TextViewOnPointerExited;
        _hoverTimer.Tick += HoverTimerOnTick;

        ToolTip.SetPlacement(AssociatedObject, PlacementMode.Pointer);
        ToolTip.SetVerticalOffset(AssociatedObject, 14);

        UpdateCurrentViewModel(AssociatedObject.DataContext as ResourceYamlViewModel);
        AttachScrollViewer();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.LayoutUpdated -= OnLayoutUpdated;
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            AssociatedObject.TextArea.TextView.PointerHover -= TextViewOnPointerHover;
            AssociatedObject.TextArea.TextView.PointerMoved -= TextViewOnPointerMoved;
            AssociatedObject.TextArea.TextView.PointerExited -= TextViewOnPointerExited;
        }

        _hoverTimer.Tick -= HoverTimerOnTick;
        StopPendingHover();
        DetachScrollViewer();
        DetachViewModel(_currentViewModel);
        _currentViewModel = null;
        CloseHoverToolTip();

        base.OnDetaching();
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachScrollViewer();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        DetachScrollViewer();
        StopPendingHover();
        CloseHoverToolTip();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateCurrentViewModel(AssociatedObject?.DataContext as ResourceYamlViewModel);
        StopPendingHover();
        CloseHoverToolTip();
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        StopPendingHover();
        CloseHoverToolTip();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (!_hoverTooltipOpen || _tooltipScrollOffset == null)
        {
            return;
        }

        if (GetCurrentScrollOffset() != _tooltipScrollOffset.Value)
        {
            CloseHoverToolTip();
        }
    }

    private void UpdateCurrentViewModel(ResourceYamlViewModel? nextViewModel)
    {
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            return;
        }

        DetachViewModel(_currentViewModel);
        _currentViewModel = nextViewModel;
        AttachViewModel(nextViewModel);
    }

    private void AttachViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void DetachViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged -= ViewModelOnPropertyChanged;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ResourceYamlViewModel.Object)
            or nameof(ResourceYamlViewModel.ValidationDiagnostics)
            or nameof(ResourceYamlViewModel.HideNoisyFields))
        {
            StopPendingHover();
            CloseHoverToolTip();
        }
    }

    private void TextViewOnPointerHover(object? sender, PointerEventArgs e)
    {
        AttachScrollViewer();
        QueueHover(e.GetPosition(AssociatedObject!.TextArea.TextView));
    }

    private void TextViewOnPointerMoved(object? sender, PointerEventArgs e)
    {
        AttachScrollViewer();
        var point = e.GetPosition(AssociatedObject!.TextArea.TextView);
        TryShowHoverTooltipAtPoint(point, onlyWhenOpen: true);
        QueueHover(point);
    }

    private void TextViewOnPointerExited(object? sender, PointerEventArgs e)
    {
        StopPendingHover();
        CloseHoverToolTip();
    }

    private void HoverTimerOnTick(object? sender, EventArgs e)
    {
        _hoverTimer.Stop();

        if (_pendingHoverPoint is { } point)
        {
            TryShowHoverTooltipAtPoint(point);
        }
    }

    private void AttachScrollViewer()
    {
        if (AssociatedObject == null)
        {
            return;
        }

        if (_scrollViewer != null)
        {
            return;
        }

        _scrollViewer = AssociatedObject.GetScrollViewer();

        if (_scrollViewer != null)
        {
            _scrollViewer.PropertyChanged += ScrollViewerOnPropertyChanged;
        }
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
        if (e.Property == ScrollViewer.OffsetProperty)
        {
            StopPendingHover();
            CloseHoverToolTip();
        }
    }

    private void QueueHover(Point point)
    {
        _pendingHoverPoint = point;
        _hoverTimer.Stop();
        _hoverTimer.Start();
    }

    private void StopPendingHover()
    {
        _hoverTimer.Stop();
        _pendingHoverPoint = null;
    }

    private bool TryShowHoverTooltipAtOffset(int offset, bool onlyWhenOpen = false)
    {
        if (onlyWhenOpen && !_hoverTooltipOpen)
        {
            return false;
        }

        if (AssociatedObject?.Document == null)
        {
            return false;
        }

        if (TryCreateDocumentationTip(offset, out var documentationTip))
        {
            ShowHoverToolTip(documentationTip);
            return true;
        }

        var diagnosticMessage = YamlDiagnosticRenderingBehavior.GetRenderer(AssociatedObject)?.TryGetMessageAt(offset);
        if (string.IsNullOrEmpty(diagnosticMessage))
        {
            CloseHoverToolTip();
            return false;
        }

        ShowHoverToolTip(diagnosticMessage);
        return true;
    }

    private bool TryShowHoverTooltipAtPoint(Point point, bool onlyWhenOpen = false)
    {
        if (!TryGetPointerOffset(point, out var offset))
        {
            CloseHoverToolTip();
            return false;
        }

        return TryShowHoverTooltipAtOffset(offset, onlyWhenOpen);
    }

    private bool TryCreateDocumentationTip(int offset, out object tip)
    {
        tip = null!;

        if (AssociatedObject?.Document == null || _currentViewModel?.Object == null || _currentViewModel.Cluster == null)
        {
            return false;
        }

        var context = YamlSchemaContext.Resolve(
            AssociatedObject.Document,
            offset,
            _currentViewModel.Object.GetType(),
            _currentViewModel.Cluster.ModelCache);

        if (context.Documentation == null || context.CurrentProperty == null || !IsWithinFieldName(AssociatedObject.Document, offset, context))
        {
            return false;
        }

        tip = YamlDocumentationViewFactory.Create(context.Documentation);
        return true;
    }

    private void ShowHoverToolTip(object tip)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        ToolTip.SetTip(AssociatedObject, tip);
        ToolTip.SetIsOpen(AssociatedObject, true);
        _hoverTooltipOpen = true;
        _tooltipScrollOffset = GetCurrentScrollOffset();
    }

    private void CloseHoverToolTip()
    {
        if (AssociatedObject != null)
        {
            ToolTip.SetIsOpen(AssociatedObject, false);
            ToolTip.SetTip(AssociatedObject, null);
        }

        _hoverTooltipOpen = false;
        _tooltipScrollOffset = null;
    }

    private Vector GetCurrentScrollOffset()
    {
        if (AssociatedObject == null)
        {
            return default;
        }

        if (_scrollViewer != null)
        {
            return _scrollViewer.Offset;
        }

        return new Vector(AssociatedObject.HorizontalOffset, AssociatedObject.VerticalOffset);
    }

    private bool TryGetPointerOffset(Point point, out int offset)
    {
        offset = default;

        if (AssociatedObject?.Document == null)
        {
            return false;
        }

        AssociatedObject.TextArea.TextView.EnsureVisualLines();
        var visualPoint = point + AssociatedObject.TextArea.TextView.ScrollOffset;
        var position = AssociatedObject.TextArea.TextView.GetPosition(visualPoint);
        if (!position.HasValue)
        {
            position = AssociatedObject.TextArea.TextView.GetPositionFloor(visualPoint);
        }

        if (!position.HasValue)
        {
            return false;
        }

        offset = AssociatedObject.Document.GetOffset(position.Value.Location);
        return true;
    }

    private static bool IsWithinFieldName(TextDocument document, int offset, YamlContextResult context)
    {
        _ = document;
        return offset >= context.Key.StartOffset && offset <= context.Key.EndOffset;
    }
}
