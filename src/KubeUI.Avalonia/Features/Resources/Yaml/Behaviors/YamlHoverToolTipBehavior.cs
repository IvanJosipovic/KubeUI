using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Styles;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

public sealed class YamlHoverToolTipBehavior : Behavior<TextEditor>
{
    private const double HoverPopupHorizontalOffset = 8;
    private const double HoverPopupVerticalOffset = 0;

    private ResourceYamlViewModel? _currentViewModel;
    private YamlEditorBehavior? _editorBehavior;
    private Popup? _hoverPopup;
    private int _hoverRequest;
    private ScrollViewer? _scrollViewer;
    private Task? _schemaLoadTask;
    private Vector? _tooltipScrollOffset;
    private YamlSchemaNode? _schemaRoot;
    private ClusterModelCatalog? _schemaCatalog;
    private GroupApiVersionKind _schemaKind;
    private long _schemaVersion = -1;

    /// <summary>Creates a YAML editor hover documentation behavior.</summary>
    public YamlHoverToolTipBehavior()
    {
    }

    internal YamlHoverToolTipBehavior(Popup hoverPopup)
    {
        _hoverPopup = hoverPopup;
    }

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
        AssociatedObject.TextArea.TextView.PointerHoverStopped += TextViewOnPointerHoverStopped;
        Application.Current?.ActualThemeVariantChanged += OnActualThemeVariantChanged;

        _editorBehavior = Interaction.GetBehaviors(AssociatedObject).OfType<YamlEditorBehavior>().SingleOrDefault();
        if (_editorBehavior != null)
        {
            _editorBehavior.CompletionOpened += EditorBehaviorOnCompletionOpened;
        }

        UpdateCurrentViewModel(AssociatedObject.DataContext as ResourceYamlViewModel);
        AttachScrollViewer();
    }

    protected override void OnDetaching()
    {
        Interlocked.Increment(ref _hoverRequest);
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.LayoutUpdated -= OnLayoutUpdated;
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            AssociatedObject.TextArea.TextView.PointerHover -= TextViewOnPointerHover;
            AssociatedObject.TextArea.TextView.PointerHoverStopped -= TextViewOnPointerHoverStopped;
            Application.Current?.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        }

        DetachScrollViewer();
        if (_editorBehavior != null)
        {
            _editorBehavior.CompletionOpened -= EditorBehaviorOnCompletionOpened;
            _editorBehavior = null;
        }

        DetachViewModel(_currentViewModel);
        _currentViewModel = null;
        _schemaLoadTask = null;
        _schemaRoot = null;
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
        InvalidateHover();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (_hoverPopup?.IsOpen == true
            && _tooltipScrollOffset is Vector offset
            && GetCurrentScrollOffset() != offset)
        {
            InvalidateHover();
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateCurrentViewModel(AssociatedObject?.DataContext as ResourceYamlViewModel);
        _schemaLoadTask = null;
        _schemaRoot = null;
        InvalidateHover();
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        InvalidateHover();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        InvalidateHover();
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
            InvalidateHover();
        }
    }

    private async void TextViewOnPointerHover(object? sender, PointerEventArgs e)
    {
        var request = Interlocked.Increment(ref _hoverRequest);
        AttachScrollViewer();
        if (_currentViewModel?.Cluster is { } cluster)
        {
            if (!await EnsureSchemaLoadAsync(cluster.Runtime.EnsureOpenApiSchemasAsync).ConfigureAwait(true))
            {
                return;
            }
        }

        if (request != Volatile.Read(ref _hoverRequest))
        {
            return;
        }

        var textView = AssociatedObject!.TextArea.TextView;
        var point = e.GetPosition(textView);
        TryShowHoverTooltipAtPoint(point);
    }

    internal async Task<bool> EnsureSchemaLoadAsync(Func<Task> loadSchemas)
    {
        try
        {
            _schemaLoadTask ??= loadSchemas();
            await _schemaLoadTask.ConfigureAwait(true);
            return true;
        }
        catch (Exception)
        {
            _schemaLoadTask = null;
            return false;
        }
    }

    private void TextViewOnPointerHoverStopped(object? sender, PointerEventArgs e)
    {
        Interlocked.Increment(ref _hoverRequest);
        CloseHoverToolTip();
    }

    private void EditorBehaviorOnCompletionOpened(object? sender, EventArgs e)
    {
        InvalidateHover();
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
            InvalidateHover();
        }
    }

    private bool TryShowHoverTooltipAtOffset(int offset, bool onlyWhenOpen = false)
    {
        if (_editorBehavior?.IsCompletionOpen == true)
        {
            InvalidateHover();
            return false;
        }

        if (onlyWhenOpen && _hoverPopup?.IsOpen != true)
        {
            return false;
        }

        if (AssociatedObject?.Document == null)
        {
            return false;
        }

        if (TryCreateDocumentationTip(offset, out var documentationTip))
        {
            ShowHoverToolTip(documentationTip, offset);
            return true;
        }

        var diagnosticMessage = YamlDiagnosticRenderingBehavior.GetRenderer(AssociatedObject)?.TryGetMessageAt(offset);
        if (string.IsNullOrEmpty(diagnosticMessage))
        {
            InvalidateHover();
            return false;
        }

        var diagnosticTip = YamlDocumentationViewFactory.CreateCodeText(diagnosticMessage);
        diagnosticTip.TextWrapping = TextWrapping.Wrap;
        diagnosticTip.MaxWidth = 520;
        ShowHoverToolTip(diagnosticTip, offset);
        return true;
    }

    private bool TryShowHoverTooltipAtPoint(Point point, bool onlyWhenOpen = false)
    {
        if (!TryGetPointerOffset(point, out var offset))
        {
            InvalidateHover();
            return false;
        }

        return TryShowHoverTooltipAtOffset(offset, onlyWhenOpen);
    }

    private bool TryCreateDocumentationTip(int offset, out Control? tip)
    {
        tip = null;

        if (AssociatedObject?.Document == null || _currentViewModel?.Object == null || _currentViewModel.Cluster == null)
        {
            return false;
        }

        var context = YamlSchemaContext.Resolve(
            AssociatedObject.Document,
            offset,
            GetSchemaRoot(_currentViewModel));

        if (context.Documentation == null || !IsWithinFieldName(offset, context))
        {
            return false;
        }

        tip = YamlDocumentationViewFactory.Create(context.Documentation);
        return true;
    }

    private void ShowHoverToolTip(Control tip, int offset)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        var textView = AssociatedObject.TextArea.TextView;
        _hoverPopup ??= new Popup();
        _hoverPopup.PlacementTarget = textView;
        _hoverPopup.Placement = PlacementMode.AnchorAndGravity;
        _hoverPopup.PlacementAnchor = PopupAnchor.TopLeft;
        _hoverPopup.PlacementGravity = PopupGravity.BottomRight;
        _hoverPopup.PlacementConstraintAdjustment = PopupPositionerConstraintAdjustment.SlideX
            | PopupPositionerConstraintAdjustment.SlideY;
        _hoverPopup.IsLightDismissEnabled = false;
        _hoverPopup.Child ??= new ContentControl
        {
            Foreground = ApplicationBrushResources.GetBrush("SystemBaseHighColor"),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(4),
            IsHitTestVisible = false,
            Background = ApplicationBrushResources.GetBrush("SystemRegionBrush"),
        };

        ((ContentControl)_hoverPopup.Child!).Content = tip;
        var location = AssociatedObject.Document!.GetLocation(offset);
        var position = new TextViewPosition(location.Line, location.Column);
        var lineTop = textView.GetVisualPosition(position, VisualYPosition.LineTop) - textView.ScrollOffset;
        var lineBottom = textView.GetVisualPosition(position, VisualYPosition.LineBottom) - textView.ScrollOffset;
        _hoverPopup.PlacementRect = new Rect(
            lineTop.X,
            lineTop.Y,
            1,
            Math.Max(1, lineBottom.Y - lineTop.Y));
        _hoverPopup.HorizontalOffset = HoverPopupHorizontalOffset;
        _hoverPopup.VerticalOffset = HoverPopupVerticalOffset;
        _hoverPopup.IsOpen = true;
        _tooltipScrollOffset = GetCurrentScrollOffset();
    }

    private void CloseHoverToolTip()
    {
        if (_hoverPopup != null)
        {
            _hoverPopup.IsOpen = false;
            _hoverPopup.Child = null;
        }

        _tooltipScrollOffset = null;
    }

    private void InvalidateHover()
    {
        Interlocked.Increment(ref _hoverRequest);
        CloseHoverToolTip();
    }

    private Vector GetCurrentScrollOffset()
    {
        if (AssociatedObject == null)
        {
            return default;
        }

        return _scrollViewer?.Offset
            ?? new Vector(AssociatedObject.HorizontalOffset, AssociatedObject.VerticalOffset);
    }

    private bool TryGetPointerOffset(Point point, out int offset)
    {
        offset = default;

        if (AssociatedObject?.Document == null)
        {
            return false;
        }

        var textView = AssociatedObject.TextArea.TextView;
        textView.EnsureVisualLines();
        var visualPoint = point + textView.ScrollOffset;
        var position = textView.GetPosition(visualPoint);
        if (!position.HasValue)
        {
            position = textView.GetPositionFloor(visualPoint);
        }

        if (!position.HasValue)
        {
            return false;
        }

        var lineNumber = position.Value.Location.Line;
        var line = AssociatedObject.Document.GetLineByNumber(lineNumber);
        var lineStart = new TextViewPosition(lineNumber, 1);
        var lineEnd = new TextViewPosition(lineNumber, line.Length + 1);
        var top = textView.GetVisualPosition(lineStart, VisualYPosition.LineTop).Y;
        var bottom = textView.GetVisualPosition(lineStart, VisualYPosition.LineBottom).Y;
        var left = textView.GetVisualPosition(lineStart, VisualYPosition.TextTop).X;
        var right = textView.GetVisualPosition(lineEnd, VisualYPosition.TextTop).X;
        if (visualPoint.Y < top || visualPoint.Y >= bottom
            || visualPoint.X < left || visualPoint.X >= right)
        {
            return false;
        }

        offset = AssociatedObject.Document.GetOffset(position.Value.Location);
        return true;
    }

    private static bool IsWithinFieldName(int offset, YamlContextResult context)
    {
        return offset >= context.Key.StartOffset && offset <= context.Key.EndOffset;
    }

    internal YamlSchemaNode GetSchemaRoot(ResourceYamlViewModel vm)
    {
        var catalog = vm.Cluster!.Runtime.ModelCatalog;
        var kind = vm.ResourceKind;
        var version = catalog.OpenApiSchemas.Version;
        if (_schemaRoot is null
            || !ReferenceEquals(_schemaCatalog, catalog)
            || _schemaKind != kind
            || _schemaVersion != version)
        {
            _schemaRoot = YamlSchemaContext.CreateRoot(kind, catalog);
            _schemaCatalog = catalog;
            _schemaKind = kind;
            _schemaVersion = version;
        }

        return _schemaRoot;
    }
}
