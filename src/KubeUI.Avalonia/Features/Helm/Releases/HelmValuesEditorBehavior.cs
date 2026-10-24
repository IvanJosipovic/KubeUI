using System.ComponentModel;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace KubeUI.Avalonia.Features.Helm.Releases;

internal sealed class HelmValuesEditorBehavior : Behavior<TextEditor>
{
    private HelmReleaseEditorViewModel? _viewModel;
    private ScrollViewer? _editorScrollViewer;
    private ScrollViewer? _formScrollViewer;
    private Vector? _pendingEditorScrollOffset;
    private Vector? _pendingFormScrollOffset;
    private bool _updatingEditor;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.DataContextChanged += DataContextChanged;
        AssociatedObject.TextChanged += EditorTextChanged;
        AssociatedObject.LayoutUpdated += LayoutUpdated;
        SetViewModel(AssociatedObject.DataContext as HelmReleaseEditorViewModel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DataContextChanged -= DataContextChanged;
        AssociatedObject.TextChanged -= EditorTextChanged;
        AssociatedObject.LayoutUpdated -= LayoutUpdated;
        SetViewModel(null);
        _editorScrollViewer = null;
        _formScrollViewer = null;
        _pendingEditorScrollOffset = null;
        _pendingFormScrollOffset = null;
        base.OnDetaching();
    }

    private void DataContextChanged(object? sender, EventArgs args)
        => SetViewModel(AssociatedObject.DataContext as HelmReleaseEditorViewModel);

    private void SetViewModel(HelmReleaseEditorViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= ViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
            UpdateEditor(_viewModel.ValuesYaml);
        }
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(HelmReleaseEditorViewModel.ValuesYaml) && _viewModel is not null)
        {
            UpdateEditor(_viewModel.ValuesYaml);
        }
    }

    private void EditorTextChanged(object? sender, EventArgs args)
    {
        if (!_updatingEditor && _viewModel is not null)
        {
            _viewModel.ValuesYaml = AssociatedObject.Text;
        }
    }

    private void UpdateEditor(string text)
    {
        if (string.Equals(AssociatedObject.Text, text, StringComparison.Ordinal))
        {
            return;
        }

        AttachScrollViewers();
        _pendingEditorScrollOffset = _editorScrollViewer?.Offset;
        _pendingFormScrollOffset = _formScrollViewer?.Offset;
        var caretOffset = AssociatedObject.CaretOffset;
        _updatingEditor = true;
        try
        {
            AssociatedObject.Text = text;
            AssociatedObject.CaretOffset = Math.Min(caretOffset, text.Length);
        }
        finally
        {
            _updatingEditor = false;
        }
    }

    private void LayoutUpdated(object? sender, EventArgs args)
    {
        AttachScrollViewers();

        if (_pendingEditorScrollOffset is Vector editorOffset)
        {
            _pendingEditorScrollOffset = null;
            if (_editorScrollViewer is not null)
            {
                _editorScrollViewer.Offset = editorOffset;
            }
        }

        if (_pendingFormScrollOffset is Vector formOffset)
        {
            _pendingFormScrollOffset = null;
            if (_formScrollViewer is not null)
            {
                _formScrollViewer.Offset = formOffset;
            }
        }
    }

    private void AttachScrollViewers()
    {
        _editorScrollViewer = AssociatedObject.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        _formScrollViewer = AssociatedObject.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
    }
}
