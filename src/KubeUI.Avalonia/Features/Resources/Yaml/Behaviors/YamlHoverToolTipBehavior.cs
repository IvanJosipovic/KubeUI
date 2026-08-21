using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

public sealed class YamlHoverToolTipBehavior : Behavior<TextEditor>
{
    private ResourceYamlViewModel? _currentViewModel;
    private bool _hoverTooltipOpen;
    private int _hoverRequest;
    private ScrollViewer? _scrollViewer;
    private Task? _schemaLoadTask;
    private Vector? _tooltipScrollOffset;
    private YamlSchemaNode? _schemaRoot;
    private ClusterModelCatalog? _schemaCatalog;
    private GroupApiVersionKind _schemaKind;
    private long _schemaVersion = -1;

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
            AssociatedObject.TextArea.TextView.PointerHoverStopped -= TextViewOnPointerHoverStopped;
        }

        DetachScrollViewer();
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
        CloseHoverToolTip();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (_hoverTooltipOpen
            && _tooltipScrollOffset is Vector offset
            && GetCurrentScrollOffset() != offset)
        {
            CloseHoverToolTip();
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateCurrentViewModel(AssociatedObject?.DataContext as ResourceYamlViewModel);
        _schemaLoadTask = null;
        _schemaRoot = null;
        CloseHoverToolTip();
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        CloseHoverToolTip();
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
            CloseHoverToolTip();
        }
    }

    private async void TextViewOnPointerHover(object? sender, PointerEventArgs e)
    {
        var request = Interlocked.Increment(ref _hoverRequest);
        AttachScrollViewer();

        if (_currentViewModel?.Cluster is { } cluster)
        {
            try
            {
                _schemaLoadTask ??= cluster.Runtime.EnsureOpenApiSchemasAsync();
                await _schemaLoadTask.ConfigureAwait(true);
            }
            catch (Exception)
            {
                return;
            }
        }

        if (request != Volatile.Read(ref _hoverRequest))
        {
            return;
        }

        TryShowHoverTooltipAtPoint(e.GetPosition(AssociatedObject!.TextArea.TextView));
    }

    private void TextViewOnPointerHoverStopped(object? sender, PointerEventArgs e)
    {
        Interlocked.Increment(ref _hoverRequest);
        CloseHoverToolTip();
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
            CloseHoverToolTip();
        }
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
            GetSchemaRoot(_currentViewModel));

        if (context.Documentation == null || !IsWithinFieldName(offset, context))
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

    private static bool IsWithinFieldName(int offset, YamlContextResult context)
    {
        return offset >= context.Key.StartOffset && offset <= context.Key.EndOffset;
    }

    private YamlSchemaNode GetSchemaRoot(ResourceYamlViewModel vm)
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
